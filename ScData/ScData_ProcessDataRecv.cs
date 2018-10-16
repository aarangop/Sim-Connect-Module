using LockheedMartin.Prepar3D.SimConnect;
using PilotAssistDll.Models;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SimConnectModule
{
    public static partial class ScData
    {
        public static void ProcessDataRecv(object data, SIMVAR_CATEGORY category)
        {
            switch (category)
            {
                case SIMVAR_CATEGORY.ENGINE_DATA:
                    EngineDataStruct engData = (EngineDataStruct)data;
                    SimulationVariable.SetValue(_monitoredSimVars["GENERAL ENG THROTTLE LEVER POSITION:1"], (float)(Math.Truncate(engData.GENERAL_ENG_THROTTLE_LEVER_POSITION_1 * 1000) / 1000));
                    SimulationVariable.SetValue(_monitoredSimVars["GENERAL ENG THROTTLE LEVER POSITION:2"], (float)(Math.Truncate(engData.GENERAL_ENG_THROTTLE_LEVER_POSITION_2 * 1000) / 1000));
                    break;

                case SIMVAR_CATEGORY.AIRCRAFT_MISCELANEOUS:
                    AircraftMiscelaneousDataStruct miscData = (AircraftMiscelaneousDataStruct)data;
                    SimulationVariable.SetValue(_monitoredSimVars["CABIN NO SMOKING ALERT SWITCH"], miscData.CABIN_NO_SMOKING_ALERT_SWITCH);
                    SimulationVariable.SetValue(_monitoredSimVars["CABIN SEATBELTS ALERT SWITCH"], miscData.CABIN_SEATBELTS_ALERT_SWITCH);
                    break;

                case SIMVAR_CATEGORY.CONTROLS:
                    AircraftControlsDataStruct controlsData = (AircraftControlsDataStruct)data;
                    SimulationVariable.SetValue(_monitoredSimVars["BRAKE PARKING INDICATOR"], controlsData.BRAKE_PARKING_INDICATOR);
                    SimulationVariable.SetValue(_monitoredSimVars["BRAKE PARKING POSITION"], controlsData.BRAKE_PARKING_POSITION);
                    break;

                case SIMVAR_CATEGORY.OTHER:
                    break;
            }
        }
    }
}
