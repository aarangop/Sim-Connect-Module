using PilotAssistDll.Models;
using System.Collections.Generic;

namespace SimConnectModule
{
    public static partial class ScData
    { 
        #region Private fields

        private static Dictionary<SIMVAR_CATEGORY, bool> _registeredDataStructs = new Dictionary<SIMVAR_CATEGORY, bool>();
        private static Dictionary<string, SimulationVariable> _monitoredSimVars = new Dictionary<string, SimulationVariable>();

        #endregion

        #region Public properties

        /// <summary>
        /// Dictionary with the property definitions for the data available data structs
        /// per Simulation Variable Category.
        /// </summary>
        public static Dictionary<SIMVAR_CATEGORY, List<PropertyDef>> PropertyDefs { get => _propertyDefs; }

        /// <summary>
        /// Dictionary with a reference to all the monitored simulation variables.
        /// Can be used to provide access to the current value of any of the 
        /// monitored simulation variables.
        /// </summary>
        public static Dictionary<string, SimulationVariable> MonitoredSimVars { get => _monitoredSimVars; }

        /// <summary>
        /// Dictionary with boolean values that indicate whether a SIMVAR_CATEGORY and
        /// its corresponding data has been registered on SimConnect.
        /// </summary>
        public static Dictionary<SIMVAR_CATEGORY, bool> RegisteredDataStructs { get => _registeredDataStructs; }


        #endregion
    }
}
