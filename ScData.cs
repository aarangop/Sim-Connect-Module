using LockheedMartin.Prepar3D.SimConnect;
using MongoDB.Driver;
using PilotAssistLib.Models;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SimConnectModule
{

    public static class ScData
    { 
        #region Private fields

        private static Dictionary<SIMVAR_CATEGORY, bool> _registeredDataStructs = new Dictionary<SIMVAR_CATEGORY, bool>();
        private static Dictionary<string, SimulationVariable> _monitoredSimVars = new Dictionary<string, SimulationVariable>();

        #region Data structs property definitions

        private static List<PropertyDef> _engineDataPropDefs = new List<PropertyDef>()
        {
            new PropertyDef("NUMBER OF ENGINES", "Number of Engines", null, SIMCONNECT_DATATYPE.INT64),
            new PropertyDef("GENERAL ENG THROTTLE LEVER POSITION:0", "Thrust Lever Position 1", "percent", SIMCONNECT_DATATYPE.FLOAT64),
            new PropertyDef("GENERAL ENG THROTTLE LEVER POSITION:1", "Thrust Lever Position 2", "percent", SIMCONNECT_DATATYPE.FLOAT64),
            new PropertyDef("GENERAL ENG THROTTLE LEVER POSITION:2", "Thrust Lever Position 3", "percent", SIMCONNECT_DATATYPE.FLOAT64),
            new PropertyDef("GENERAL ENG THROTTLE LEVER POSITION:3", "Thrust Lever Position 4", "percent", SIMCONNECT_DATATYPE.FLOAT64),
        };

        private static List<PropertyDef> _aircraftMiscelaneousDataPropDefs = new List<PropertyDef>()
        {
            new PropertyDef("CABIN SEATBELTS ALERT SWITCH", "Seatbelts Sign", null, SIMCONNECT_DATATYPE.INT32),
            new PropertyDef("CABIN NO SMOKING ALERT SWITCH", "No-Smoking Sign", null, SIMCONNECT_DATATYPE.INT32)
        };

        private static Dictionary<SIMVAR_CATEGORY, List<PropertyDef>> _propertyDefs = new Dictionary<SIMVAR_CATEGORY, List<PropertyDef>>()
        {
            { SIMVAR_CATEGORY.ENGINE_DATA, _engineDataPropDefs },
            { SIMVAR_CATEGORY.AIRCRAFT_MISCELANEOUS, _aircraftMiscelaneousDataPropDefs }
        };
            #endregion

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

        #endregion
      
        #region PropertyDef struct definition

        /// <summary>
        /// Struct to standarize the property definitions for simulation variables.
        /// </summary>
        public struct PropertyDef
        {
            public string Name;
            public string DisplayName;
            public string Units;
            public SIMCONNECT_DATATYPE DataType;

            public PropertyDef(string name, string displayName, string units, SIMCONNECT_DATATYPE dataType)
            {
                Name = name;
                DisplayName = displayName;
                Units = units;
                DataType = dataType;
            }
        }
        #endregion

        #region Methods

        /// <summary>
        /// Register a data struct on simConnect and add it's members to the SimConnect Data Definition
        /// </summary>
        /// <param name="sc">SimConnect instance</param>
        /// <param name="cat">Variable category to register</param>
        public async static void RegisterStruct(SimConnect sc, SIMVAR_CATEGORY cat)
        {
            List<FieldInfo> fields = null;

            // List with the properties of the variables in the data struct
            List<PropertyDef> varProperties = null;

            // Switch the variable categories and register the appropriate struct
            switch (cat)
            {
                case SIMVAR_CATEGORY.ENGINE_DATA:

                    // Don't proceed with registration if the struct was already registered
                    if (_registeredDataStructs.ContainsKey(SIMVAR_CATEGORY.ENGINE_DATA)) return;

                    // Get the fields in the struct
                    fields = new List<FieldInfo>(typeof(EngineDataStruct).GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public));

                    // Add the added variable category to the dictionary
                    _registeredDataStructs.Add(SIMVAR_CATEGORY.ENGINE_DATA, true);

                    sc.RegisterDataDefineStruct<EngineDataStruct>(cat);

                    break;

                case SIMVAR_CATEGORY.AIRCRAFT_MISCELANEOUS:
                    // Don't proceed with registration if the struct was already registered
                    if (_registeredDataStructs.ContainsKey(SIMVAR_CATEGORY.AIRCRAFT_MISCELANEOUS)) return;

                    // Get the fields in the struct
                    fields = new List<FieldInfo>(typeof(AircraftMiscelaneousDataStruct).GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public));

                    // Add the added variable category to the dictionary
                    _registeredDataStructs.Add(SIMVAR_CATEGORY.AIRCRAFT_MISCELANEOUS, true);

                    sc.RegisterDataDefineStruct<AircraftMiscelaneousDataStruct>(cat);

                    break;
            }

            // Exit if the struct has no fields
            if (fields == null) return;

            varProperties = _propertyDefs[cat];

            // Loop struct fields and add the corresponding variables to the Data Definition
            for (int i = 0; i < fields.Count; i++)
            {
                FieldInfo field = fields[i];
                PropertyDef varProps = varProperties[i];

                string varLookupName = field.Name;

                // Reformat the name of variables with index
                // Example: GENERAL_ENG_THROTTLE_LEVER_POSITION_0 to GENERAL_ENG_THROTTLE_LEVER_POSITION:0
                string pattern = @"_(?<index>[0-9][0-9]*)$";
                Match match = Regex.Match(field.Name, pattern);

                if (match.Success)
                {
                    varLookupName = Regex.Replace(varLookupName, pattern, m => $":{m.Groups[1]}");
                }

                // Replace underscores by spaces
                varLookupName = varLookupName.Replace('_', ' ');

                var filter = Builders<SimulationVariable>.Filter.Eq(simvar => simvar.Name, varLookupName);

                List<SimulationVariable> results = await SimulationVariable.Find(filter);
                SimulationVariable sv;

                if (results.Count > 0)
                {
                    sv = results[0];
                }
                else
                {
                    sv = new SimulationVariable(varProps.Name, varProps.DisplayName, (int)cat, varProps.Units, (int)varProps.DataType, (int)SIMVAR_SOURCE.SIMCONNECT);
                    SimulationVariable.AddNew(sv);
                }

                if (!_monitoredSimVars.ContainsKey(sv.Name))
                {
                    _monitoredSimVars.Add(sv.Name, sv);
                }

                SIMCONNECT_DATATYPE dataType = (SIMCONNECT_DATATYPE)sv.DataType;
                sc.AddToDataDefinition(cat, sv.Name, sv.Units, dataType, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            }
        }

        /// <summary>
        /// Registers all data structs on the specified simconnect instance.
        /// </summary>
        public static void RegisterDataStructs(SimConnect sc)
        {
            RegisterStruct(sc, SIMVAR_CATEGORY.ENGINE_DATA);
            RegisterStruct(sc, SIMVAR_CATEGORY.AIRCRAFT_MISCELANEOUS);
        }

        public static void ProcessDataRecv(SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data)
        {
            switch ((SIMVAR_CATEGORY)data.dwRequestID)
            {
                case SIMVAR_CATEGORY.ENGINE_DATA:
                    EngineDataStruct engData = (EngineDataStruct)data.dwData[0];

                    _monitoredSimVars["NUMBER OF ENGINES"].SetValue(engData.NUMBER_OF_ENGINES);
                    _monitoredSimVars["GENERAL ENG THROTTLE LEVER POSITION:0"].SetValue(engData.GENERAL_ENG_THROTTLE_LEVER_POSITION_0);
                    _monitoredSimVars["GENERAL ENG THROTTLE LEVER POSITION:1"].SetValue(engData.GENERAL_ENG_THROTTLE_LEVER_POSITION_1);
                    _monitoredSimVars["GENERAL ENG THROTTLE LEVER POSITION:2"].SetValue(engData.GENERAL_ENG_THROTTLE_LEVER_POSITION_2);
                    _monitoredSimVars["GENERAL ENG THROTTLE LEVER POSITION:3"].SetValue(engData.GENERAL_ENG_THROTTLE_LEVER_POSITION_3);
                    
                    break;
            }
        }
        #endregion
    }
}
