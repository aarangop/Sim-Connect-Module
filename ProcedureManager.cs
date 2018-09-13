using PilotAssistDll.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        #region Methods

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

        #endregion
    }
}
