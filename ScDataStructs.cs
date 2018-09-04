// this is how you declare a data structure so that
// simconnect knows how to fill it/read it.
using System.Runtime.InteropServices;

namespace SimConnectModule
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct EngineDataStruct
    {
        public int NUMBER_OF_ENGINES;
        public double GENERAL_ENG_THROTTLE_LEVER_POSITION_0;
        public double GENERAL_ENG_THROTTLE_LEVER_POSITION_1;
        public double GENERAL_ENG_THROTTLE_LEVER_POSITION_2;
        public double GENERAL_ENG_THROTTLE_LEVER_POSITION_3;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct AircraftMiscelaneousDataStruct
    {
        public int CABIN_SEATBELTS_ALERT_SWITCH;
        public int CABIN_NO_SMOKING_ALERT_SWITCH;
    }
}