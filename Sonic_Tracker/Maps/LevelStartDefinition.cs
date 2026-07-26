namespace Sonic_Tracker.Maps;

public sealed record LevelStartDefinition(
    string ZoneId,
    int ActNumber,
    double StartX,
    double StartY,
    double ToleranceX = 32,
    double ToleranceY = 32)
{
    public bool Contains(double x, double y) =>
        Math.Abs(x - StartX) <= ToleranceX &&
        Math.Abs(y - StartY) <= ToleranceY;
}
