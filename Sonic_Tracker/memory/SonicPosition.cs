namespace Sonic_Tracker.Memory;

public readonly record struct SonicPosition(
    double X,
    double Y,
    int RawX,
    int RawY);