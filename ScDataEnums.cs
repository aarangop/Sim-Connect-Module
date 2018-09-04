namespace SimConnectModule
{
    public enum SIMVAR_CATEGORY
    {
        ENGINE_DATA,
        FUEL_DATA,
        LIGHTS,
        POSITION_AND_SPEED,
        FLIGHT_INSTRUMENTATION,
        AVIONICS,
        CONTROLS,
        AUTOPILOT,
        LANDING_GEAR,
        ENVIRONMENT,
        AIRCRAFT_MISCELANEOUS,
        CREW_INPUT_VARIABLE,
        OTHER
    }

    public enum SIMVAR_SOURCE
    {
        SIMCONNECT,
        PILOT
    }
}