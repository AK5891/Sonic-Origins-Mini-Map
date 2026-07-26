namespace Sonic_Tracker.Maps;

public static class Sonic1LevelStartRegistry
{
    private static readonly IReadOnlyList<LevelStartDefinition> Starts =
    [
        new("GreenHill", 1, 80, 944),
        new("GreenHill", 2, 80, 252),
        new("GreenHill", 3, 80, 944),
        new("Marble", 1, 48, 612),
        new("Marble", 2, 48, 612),
        new("Marble", 3, 48, 356),
        new("SpringYard", 1, 48, 957),
        new("SpringYard", 2, 48, 445),
        new("SpringYard", 3, 48, 236),
        new("Labyrinth", 1, 60, 108),
        new("Labyrinth", 2, 80, 236),
        new("Labyrinth", 3, 80, 748),
        new("StarLight", 1, 64, 716),
        new("StarLight", 2, 64, 332),
        new("StarLight", 3, 64, 332),
        new("ScrapBrain", 1, 48, 1164),
        new("ScrapBrain", 2, 48, 1868),
        new("Final", 1, 316, 428)
    ];

    public static IReadOnlyList<LevelStartDefinition> FindMatches(
        string zoneId,
        double x,
        double y) =>
        Starts
            .Where(start =>
                string.Equals(start.ZoneId, zoneId, StringComparison.OrdinalIgnoreCase) &&
                start.Contains(x, y))
            .OrderBy(start => start.ActNumber)
            .ToArray();
}
