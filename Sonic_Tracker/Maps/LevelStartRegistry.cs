namespace Sonic_Tracker.Maps;

public static class LevelStartRegistry
{
    private static readonly IReadOnlyList<LevelStartDefinition> Starts =
    [
        new("EmeraldHill", 1, 68, 661),
        new("EmeraldHill", 2, 64, 692),
        new("ChemicalPlant", 1, 56, 496),
        new("ChemicalPlant", 2, 52, 304),
        new("AquaticRuin", 1, 108, 897),
        new("AquaticRuin", 2, 64, 896),
        new("CasinoNight", 1, 72, 688),
        new("CasinoNight", 2, 76, 1424),
        new("MysticCave", 1, 104, 1712),
        new("MysticCave", 2, 64, 1456),
        new("HillTop", 1, 68, 1012),
        new("HillTop", 2, 64, 1716),
        new("Metropolis", 1, 36, 656),
        new("Metropolis", 2, 56, 1520),
        new("Metropolis", 3, 40, 528),
        new("OilOcean", 1, 96, 1712),
        new("OilOcean", 2, 92, 1392),
        new("DeathEgg", 1, 52, 304)
    ];

    public static LevelStartDefinition? FindClosest(
        string zoneId,
        double x,
        double y) =>
        Starts
            .Where(start =>
                string.Equals(start.ZoneId, zoneId, StringComparison.OrdinalIgnoreCase) &&
                start.Contains(x, y))
            .OrderBy(start =>
                Math.Pow(x - start.StartX, 2) +
                Math.Pow(y - start.StartY, 2))
            .FirstOrDefault();
}
