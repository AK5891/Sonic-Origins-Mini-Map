namespace Sonic_Tracker.Maps;

public enum SonicCDPastObjectType
{
    MetalSonicGenerator,
    RobotCarrier
}

public readonly record struct SonicCDPastObjectLocation(
    SonicCDPastObjectType Type,
    double X,
    double Y);
