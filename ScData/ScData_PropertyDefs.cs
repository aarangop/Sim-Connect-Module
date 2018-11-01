using LockheedMartin.Prepar3D.SimConnect;
using System;
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
            new PropertyDef("BRAKE PARKING POSITION", "Parking Brake Position", null, SIMCONNECT_DATATYPE.FLOAT64),
            new PropertyDef("FLAPS HANDLE PERCENT", "Flaps pct", "Percent", SIMCONNECT_DATATYPE.FLOAT32),
            new PropertyDef("FLAPS HANDLE INDEX", "Flaps", null, SIMCONNECT_DATATYPE.FLOAT32)
        };

        private static List<PropertyDef> _aircraftFlightInstrumentationDataPropDefs = new List<PropertyDef>()
        {
            new PropertyDef("ATTITUDE INDICATOR PITCH DEGREES", "Pitch Angle", "degrees", SIMCONNECT_DATATYPE.FLOAT64),
            new PropertyDef("ATTITUDE INDICATOR BANK DEGREES", "Bank Angle", "degrees", SIMCONNECT_DATATYPE.FLOAT64),
            new PropertyDef("WISKEY COMPASS INDICATION DEGREES", "Heading", "degrees", SIMCONNECT_DATATYPE.FLOAT64),
            new PropertyDef("INDICATED ALTITUDE", "Altitude", "Feet", SIMCONNECT_DATATYPE.FLOAT64),
            new PropertyDef("AIRSPEED INDICATED", "IAS", "Knots", SIMCONNECT_DATATYPE.FLOAT64),
            new PropertyDef("VERTICAL SPEED", "Vertical Speed", "Feet per second", SIMCONNECT_DATATYPE.FLOAT64)
        };

        private static List<PropertyDef> _aircraftLandingGearDataPropDefs = new List<PropertyDef>()
        {
            new PropertyDef("GEAR TOTAL PCT EXTENDED", "Gear Extension", "Percentage", SIMCONNECT_DATATYPE.FLOAT32)
        };

        private static Dictionary<SIMVAR_CATEGORY, List<PropertyDef>> _propertyDefs = new Dictionary<SIMVAR_CATEGORY, List<PropertyDef>>(){};
    }
}