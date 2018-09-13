using LockheedMartin.Prepar3D.SimConnect;
using System.Collections.Generic;

namespace SimConnectModule
{
    public static partial class ScData
    {
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

        private static List<PropertyDef> _aircraftControlsDataPropDefs = new List<PropertyDef>()
        {
            new PropertyDef("BRAKE PARKING INDICATOR", "Parking Brake Indicator", null, SIMCONNECT_DATATYPE.FLOAT64),
            new PropertyDef("BRAKE PARKING POSITION", "Parking Brake Position", null, SIMCONNECT_DATATYPE.FLOAT64)
        };

        private static Dictionary<SIMVAR_CATEGORY, List<PropertyDef>> _propertyDefs = new Dictionary<SIMVAR_CATEGORY, List<PropertyDef>>()
        {
            //{ SIMVAR_CATEGORY.ENGINE_DATA, _engineDataPropDefs },
            //{ SIMVAR_CATEGORY.AIRCRAFT_MISCELANEOUS, _aircraftMiscelaneousDataPropDefs },
            //{ SIMVAR_CATEGORY.CONTROLS, _aircraftControlsDataPropDefs }
        };
    }
}