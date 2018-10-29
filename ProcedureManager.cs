using MongoDB.Bson;
using PilotAssistDll.Helpers;
using PilotAssistDll.Models;
using PilotAssistModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace SimConnectModule
{
    public static class ProcedureManager
    {
        #region Private Fields

        private static Procedure _activeProcedure;
        private static ProcedureItem _activeItem;
        private static ProcedureItem _latestItemSent;
        private static object _activeItemValue;
        private static int _activeItemIndex = -1;

        private static bool _procedureCompleted;
        private static bool _stopProcedureLoop = false;
        private static string _mqttClientId;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Public Properties

        public delegate void OnActiveProcedureChangedDelegate(Procedure procedure);
        public delegate void OnActiveItemChangedDelegate(ProcedureItem procedureItem);
        public delegate void OnActiveItemValueChangedDelegate(ProcedureItem procedureItem, object newValue);

        public static event OnActiveProcedureChangedDelegate OnActiveProcedureChanged;
        public static event OnActiveProcedureChangedDelegate OnProcedureCompleted;
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
            OnProcedureCompleted += NotifyProcedureCompleted;
        }

        #endregion

        #region Methods

        private static void NotifyProcedureCompleted(Procedure procedure)
        {
            procedure.SetModelStruct();

            byte[] msg = ModelSerializer.StrucToByteArray(procedure.Model);

            MqttManager.Client.Publish(MqttTopics.ServerPublishTopics[MqttTopics.ServerPublish.ProcedureCompleted], msg);
        }

        private static void DeactivateActiveProcedure()
        {
            _activeItemIndex = -1;
            ActiveProcedure = null;
            ActiveItem = null;
            _latestItemSent = null;
        }

        private static void MqttManager_ConnectionStatusChanged(object sender, ConnectionChangedEventArgs e)
        {
            if (MqttManager.Client == null) return;

            if (_mqttClientId != MqttManager.Client.ClientId )
            {
                _mqttClientId = MqttManager.Client.ClientId;
                MqttManager.Client.MqttMsgPublishReceived += Client_MqttMsgPublishReceived;
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
                log.Info("Available procedures request received");
                SendAvailableProcedures();
                return;
            }

            if (e.Topic == MqttTopics.ServerReceiveTopics[MqttTopics.ServerReceive.RequestProcedureStart])
            {
                ProcedureStruct procedure = ModelSerializer.ByteArrayToStruct<ProcedureStruct>(e.Message);
                ObjectId procedureId = new ObjectId(procedure.Id);

                Procedure activeProcedure = Procedure.AllItems.Find(pr => ObjectId.Equals(procedureId, pr.Id));

                if (activeProcedure != null)
                {
                    log.Info($"Procedure {activeProcedure.InvokeName} found.");
                    await ActivateProcedure(activeProcedure);
                }
                else
                {
                    log.Warn("Procedure received via MQTT not found.");
                }
                return;
            }

            if (e.Topic == MqttTopics.ServerReceiveTopics[MqttTopics.ServerReceive.ActivateNextItem])
            {
                ForceNextItem();
            }
            log.Info(e);
        }

        /// <summary>
        /// Send serialized models of each available procedure to the Mqtt broker.
        /// </summary>
        private static void SendAvailableProcedures()
        {
            
            string topic = MqttTopics.ServerPublishTopics[MqttTopics.ServerPublish.AvailableProcedures];
            int proceduresSent = 0;
            foreach (Procedure p in Procedure.AllItems)
            {
                p.SetModelStruct();
                byte[] msg = ModelSerializer.StrucToByteArray(p.Model);
                MqttManager.Client.Publish(topic, msg);
                proceduresSent++;
            }
            log.Info($"{proceduresSent} procedures published to {topic} via broker {MqttManager.BrokerAddress}");
        }

        public async static Task ActivateProcedure(Procedure procedure)
        {
            if (procedure.Items.Count == 0)
            {
                log.Warn($"Procedure {procedure.InvokeName} has zero items in its Items Collection.");
                throw new ArgumentException($"The procedure {procedure.Name} has zero items in it's Items collection and can't be monitored.");
            }

            ActiveProcedure = procedure;

            _activeItemIndex = 0;

            _procedureCompleted = false;

            try
            {
                IEnumerable<SIMVAR_CATEGORY> categories = ActiveProcedure.Items.Select(x => (SIMVAR_CATEGORY)x.SimVar.Category).Distinct();

                // Register all the data structs for each sim var category in the items list.
                foreach (SIMVAR_CATEGORY cat in categories)
                {
                    await ScManagedLib.RegisterDataStruct(cat);
                }

                await StartProcedureLoop();
            }
            catch(Exception e)
            {
                log.Error($"Error while registering data structs for {procedure.InvokeName}. Error: {e.Message}");
            }
        }

        public static void AbortActiveProcedure()
        {
            if (_activeProcedure == null) return;

            _stopProcedureLoop = true;
        }

        private static async Task ProcedureLoop()
        {
            while (!_stopProcedureLoop && !_procedureCompleted)
            {
                if (await _activeItem.SimVar.Assert(_activeItem.Target))
                {
                    if (_activeItemIndex == ActiveProcedure.Items.Count - 1)
                    {
                        _procedureCompleted = true;
                        OnProcedureCompleted?.Invoke(ActiveProcedure);
                    }
                    else
                    {
                        ActiveItem = ActiveProcedure.Items[++_activeItemIndex];
                        await Task.Delay(1500);
                        SendActiveItem();
                    }
                }
                
                // Lower the frequency of the loop to avoid false positives,
                // For example if the thrust lever passes through the desired target value.
                await Task.Delay(500);
            }

            _stopProcedureLoop = false;

            DeactivateActiveProcedure();
        }

        private static async Task<bool> StartProcedureLoop()
        {
            _activeItemIndex = 0;
            ActiveItem = ActiveProcedure.Items[_activeItemIndex];
            SendActiveItem();

            _stopProcedureLoop = false;

            await ProcedureLoop();

            return false;
        }

        private static void SendActiveItem()
        {
            if (_latestItemSent != ActiveItem)
            {
                ActiveItem.SetModelStruct();
                MqttManager.PublishDataStruct<ProcedureItemStruct>(ActiveItem.Model, MqttTopics.ServerPublishTopics[MqttTopics.ServerPublish.ActiveProcedureItem]);
                _latestItemSent = ActiveItem;
            }
        }

        public static void ForceNextItem()
        {
            if (_activeItemIndex == -1) return;
            if (ActiveProcedure == null) return;

            _activeItemIndex++;

            if (_activeItemIndex > ActiveProcedure.Items.Count - 1)
            {
                // Complete procedure
                _procedureCompleted = true;
                OnProcedureCompleted?.Invoke(ActiveProcedure);
            }
            else
            {
                ActiveItem = ActiveProcedure.Items[_activeItemIndex];
                SendActiveItem();
            }

        }

        #endregion
    }
}
