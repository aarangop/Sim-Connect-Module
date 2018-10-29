using LockheedMartin.Prepar3D.SimConnect;
using MongoDB.Driver;
using PilotAssistDll.Models;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SimConnectModule
{
    public static partial class ScData
    {
        /// <summary>
        /// Register a data struct on simConnect and add its members to the SimConnect Data Definition.
        /// </summary>
        /// <param name="sc">SimConnect instance</param>
        /// <param name="cat">Variable category to register</param>
        public async static Task RegisterStruct(SimConnect sc, SIMVAR_CATEGORY cat)
        {
            // Don't proceed with registration if the struct was already registered
            if (_registeredDataStructs.ContainsKey(cat)) return;

            // Register the struct that corresponds to the SIMVAR_CATEGORY and return it's type.
            Type structType = SimConnectRegisterDataDefineStruct(sc, cat);

            List<FieldInfo> fields = null;

            if (structType != null)
            {
                fields = new List<FieldInfo>(structType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public));
            }

            // Return when there is no struct associated with this category.
            if (fields == null) return;

            // Loop struct fields and add the corresponding variables to the Data Definition
            for (int i = 0; i < fields.Count; i++)
            {
                FieldInfo field = fields[i];

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
                    SimulationVariable.SetValueType(sv, field.FieldType);
                }
                else
                {
                    sv = new SimulationVariable(varLookupName, string.Empty, (int)cat, null, (int)GetSimConnectDataType(field.FieldType), field.FieldType, (int)SIMVAR_SOURCE.SIMCONNECT);
                    await SimulationVariable.AddNewAsync(sv);
                }

                if (!_monitoredSimVars.ContainsKey(sv.Name))
                {
                    _monitoredSimVars.Add(sv.Name, sv);
                }

                sc.AddToDataDefinition(cat, sv.Name, sv.Units, (SIMCONNECT_DATATYPE)sv.ScDataType, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            }

            // Add the added variable category to the dictionary of registered structs
            _registeredDataStructs.Add(cat, true);
        }

        private static SIMCONNECT_DATATYPE GetSimConnectDataType(Type t)
        {
            SIMCONNECT_DATATYPE res = 0;

            if (t == typeof(double))
            {
                res = SIMCONNECT_DATATYPE.FLOAT64;
            }
            else if (t == typeof(bool))
            {
                res = SIMCONNECT_DATATYPE.INT32;
            }
            else if (t == typeof(string))
            {
                res = SIMCONNECT_DATATYPE.STRING128;
            }

            return res;
        }

        /// <summary>
        /// Register a Struct in SimConnect that corresponds to a SIMVAR_CATEGORY and return the Struct's type.
        /// </summary>
        /// <param name="sc">SimConnect instance to register the struct.</param>
        /// <param name="cat">SIMVAR_CATEGORY that corresponds to the struct to be registered.</param>
        /// <returns></returns>
        private static Type SimConnectRegisterDataDefineStruct(SimConnect sc, SIMVAR_CATEGORY cat)
        {
            Type retType = null;

            // Switch the variable categories
            // Get the fields from the corresponding struct to 
            // set up the simulation variables to be monitored.
            // Finally, register the struct to the simconnect instance.
            switch (cat)
            {
                case SIMVAR_CATEGORY.ENGINE_DATA:
                    retType = typeof(EngineDataStruct);
                    sc.RegisterDataDefineStruct<EngineDataStruct>(cat);
                    break;

                case SIMVAR_CATEGORY.AIRCRAFT_MISCELANEOUS:
                    retType = typeof(AircraftMiscelaneousDataStruct);
                    sc.RegisterDataDefineStruct<AircraftMiscelaneousDataStruct>(cat);
                    break;

                case SIMVAR_CATEGORY.CONTROLS:
                    retType = typeof(AircraftControlsDataStruct);
                    sc.RegisterDataDefineStruct<AircraftControlsDataStruct>(cat);
                    break;

                case SIMVAR_CATEGORY.CREW_INPUT_VARIABLE:
                case SIMVAR_CATEGORY.OTHER:
                    // The category CREW_INPUT_VARIABLE and OTHER is not registered in simconnect.
                    // The data for this struct is obtained directly from the crew.
                    break;
            }

            return retType;
        }

        internal static void RequestDataAllDataStructs(SimConnect sc, int userId)
        {
            foreach(KeyValuePair<SIMVAR_CATEGORY, bool> category in _registeredDataStructs)
            {
                if (category.Value)
                {
                    sc.RequestDataOnSimObject(category.Key, category.Key, SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_PERIOD.SIM_FRAME, SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT, 0, 0, 0);
                }
            }
        }
    }
}
