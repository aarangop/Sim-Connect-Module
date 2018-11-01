// this is how you declare a data structure so that
// simconnect knows how to fill it/read it.
using LockheedMartin.Prepar3D.SimConnect;
using System.Runtime.InteropServices;

namespace SimConnectModule
{
    public static partial class ScData
    {
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

        #region Engine Data Struct

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct EngineDataStruct
        {
            public double GENERAL_ENG_THROTTLE_LEVER_POSITION_1;
            public double GENERAL_ENG_THROTTLE_LEVER_POSITION_2;
        }
        #endregion

        #region Aircraft Miscelaneous Data Struct

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct AircraftMiscelaneousDataStruct
        {
            public bool CABIN_SEATBELTS_ALERT_SWITCH;
            public bool CABIN_NO_SMOKING_ALERT_SWITCH;
        }
        #endregion

        #region Aircraft Controls Data Struct

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct AircraftControlsDataStruct
        {
            public bool BRAKE_PARKING_INDICATOR;
            public bool BRAKE_PARKING_POSITION;
        }
        #endregion

        #region Aircraft Flight Instrumentation Data
        // This struct lives in Pilot-Assist-Models since it has to be used in the HoloLens too.
        #endregion

        #region Aircraft Landing Gear Data

        #endregion
    }
}