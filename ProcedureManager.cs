using MongoDB.Bson;
using PilotAssistDll.Helpers;
using PilotAssistDll.Models;
using PilotAssistModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace SimConnectModule
{
    public static class ProcedureManager
    {
        #region Private Fields

        private static Procedure _activeProcedure;
        private static ProcedureItem _activeItem;
        private static object _activeItemValue;
        private static int _activeItemIndex;

        private static bool _procedureCompleted;
        private static bool _stopProcedureLoop = false;
        private static string _mqttClientId;

        #endregion

        #region Public Properties

        public delegate void OnActiveProcedureChangedDelegate(Procedure procedure);
        public delegate void OnActiveItemChangedDelegate(ProcedureItem procedureItem);
        public delegate void OnActiveItemValueChangedDelegate(ProcedureItem procedureItem, object newValue);

        public static event OnActiveProcedureChangedDelegate OnActiveProcedureChanged;
        public static event OnActiveItemChangedDelegate OnActiveItemChanged;
        public static event OnActiveItemValueChangedDelegate OnActiveItemValueChanged;

        public static Procedure ActiveProcedure
        {
            get => _activeProcedure;
            private set
            {
                _activeProcedure = value;
                OnActiveProcedureChanged?.Invoke(_activeProcedure);
            }
        }
        public static ProcedureItem ActiveItem
        {
            get => _activeItem;
            private set
            {
                _activeItem = value;
                OnActiveItemChanged?.Invoke(_activeItem);
            }
        }

        public static object ActiveItemValue
        {
            get => _activeItemValue;
            set
            {
                if (value == _activeItemValue) return;

                _activeItemValue = value;
                OnActiveItemValueChanged(_activeItem, _activeItemValue);
            }
        }

        #endregion

        #region Constructor

        static ProcedureManager()
        {
            // Subscribe to mqtt receive events
            MqttManager.ConnectionStatusChanged += MqttManager_ConnectionStatusChanged;
            OnActiveItemChanged += ProcedureManager_OnActiveItemChanged;
        }

        #endregion

        #region Methods

        private static void MqttManager_ConnectionStatusChanged(object sender, ConnectionChangedEventArgs e)
        {
            if (MqttManager.Client == null) return;

            if (_mqttClientId != MqttManager.Client.ClientId )
            {
                _mqttClientId = MqttManager.Client.ClientId;
                MqttManager.Client.MqttMsgPublishReceived += Client_MqttMsgPublishReceived;
            }
        }

        private static void ProcedureManager_OnActiveItemChanged(ProcedureItem procedureItem)
        {
            if (ActiveProcedure != null)
            {
                ActiveItem.SetModelStruct();
                MqttManager.PublishDataStruct(ActiveItem.Model, MqttTopics.ServerPublishTopics[MqttTopics.ServerPublish.ActiveProcedureItem]);
            }
        }

        /// <summary>
        /// Handles data received by the mqtt client.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async static void Client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            if (e.Topic == MqttTopics.ServerReceiveTopics[MqttTopics.ServerReceive.RequestAvailableProcedures])
            {
                SendAvailableProcedures();
                return;
            }

            if (e.Topic == MqttTopics.ServerReceiveTopics[MqttTopics.ServerReceive.RequestProcedureStart])
            {
                string procedureId = Encoding.ASCII.GetString(e.Message);
                Procedure activeProcedure = Procedure.AllItems.Find(pr => pr.Id.ToString() == procedureId);

                if (activeProcedure != null)
                {
                    await ActivateProcedure(activeProcedure);
                }
                return;
            }
        }

        /// <summary>
        /// Send serialized models of each available procedure to the Mqtt broker.
        /// </summary>
        private static void SendAvailableProcedures()
        {
            foreach(Procedure p in Procedure.AllItems)
            {
                p.SetModelStruct();
                byte[] msg = ModelSerializer.StrucToByteArray(p.Model);
                MqttManager.Client.Publish(MqttTopics.ServerPublishTopics[MqttTopics.ServerPublish.AvailableProcedures], msg);
            }
        }

        public async static Task ActivateProcedure(Procedure procedure)
        {
            if (procedure.Items.Count == 0)
            {
                throw new ArgumentException($"The procedure {procedure.Name} has zero items in it's Items collection and can't be monitored.");
            }

            ActiveProcedure = procedure;

            _activeItemIndex = 0;

            _procedureCompleted = false;

            ActiveItem = _activeProcedure.Items[_activeItemIndex];

            IEnumerable<SIMVAR_CATEGORY> categories = ActiveProcedure.Items.Select(x => (SIMVAR_CATEGORY)x.SimVar.Category).Distinct();

            // Register all the data structs for each sim var category in the items list.
            foreach (SIMVAR_CATEGORY cat in categories)
            {
                await ScManagedLib.RegisterDataStruct(cat);
            }

            await StartProcedureLoop();
        }

        public static void AbortActiveProcedure()
        {
            if (_activeProcedure == null) return;

            _stopProcedureLoop = true;
        }

        private static async Task<bool> StartProcedureLoop()
        {
            await NextItem();

            while (!_stopProcedureLoop && !_procedureCompleted)
            {
                ScManagedLib.RequestSimData((SIMVAR_CATEGORY)_activeItem.SimVar.Category);
                
                if (_activeItem.SimVar.Assert(_activeItem.Target))
                {
                    _procedureCompleted = await NextItem();
                }

                await Task.Delay(100);
            }

            _stopProcedureLoop = false;

            ActiveProcedure = null;

            return false;
        }

        private static async Task<bool> NextItem()
        {
            if (_activeItemIndex == 0)
            {
                ActiveItem = ActiveProcedure.Items[0];

                return false;
            } else
            {
                await Task.Delay(2000);

                _activeItemIndex++;

                if (_activeItemIndex < ActiveProcedure.Items.Count)
                {
                    ActiveItem = ActiveProcedure.Items[_activeItemIndex];

                    return false;
                }

                ActiveItem = null;

                return true;
            }
        }

        private static void SendActiveItem()
        {
            ActiveItem.SetModelStruct();
            MqttManager.PublishDataStruct<ProcedureItemStruct>(ActiveItem.Model, MqttTopics.ServerPublishTopics[MqttTopics.ServerPublish.ActiveProcedureItem]);
        }

        #endregion
    }
}
