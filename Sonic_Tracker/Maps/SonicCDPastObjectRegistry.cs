namespace Sonic_Tracker.Maps;

public static class SonicCDPastObjectRegistry
{
    // Scene entity coordinates are bottom-center anchors. Sonic CD's Past
    // map PNGs preserve these game pixel coordinates and only crop unused
    // space from the right edge.
    private static readonly IReadOnlyDictionary<
        string,
        IReadOnlyList<SonicCDPastObjectLocation>> ObjectsByMap =
            new Dictionary<
                string,
                IReadOnlyList<SonicCDPastObjectLocation>>(
                StringComparer.OrdinalIgnoreCase)
            {
                [PastMap("PalmtreePanic", 1)] =
                [
                    Generator(5776, 788),
                    Carrier(9064, 158)
                ],
                [PastMap("PalmtreePanic", 2)] =
                [
                    Generator(8136, 531),
                    Carrier(6400, 192)
                ],
                [PastMap("CollisionChaos", 1)] =
                [
                    Generator(7112, 501),
                    Carrier(4992, 624)
                ],
                [PastMap("CollisionChaos", 2)] =
                [
                    Generator(5428, 1173),
                    Carrier(3168, 621)
                ],
                [PastMap("TIdalTempest", 1)] =
                [
                    Generator(2256, 1397),
                    Carrier(5216, 736)
                ],
                [PastMap("TIdalTempest", 2)] =
                [
                    Generator(3348, 1973),
                    Carrier(5208, 992)
                ],
                [PastMap("QuartzQuadrant", 1)] =
                [
                    Generator(6288, 437),
                    Carrier(3584, 208)
                ],
                [PastMap("QuartzQuadrant", 2)] =
                [
                    Generator(128, 373),
                    Carrier(7328, 272)
                ],
                [PastMap("WackyWorkbench", 1)] =
                [
                    Generator(5300, 949),
                    Carrier(6776, 1696)
                ],
                [PastMap("WackyWorkbench", 2)] =
                [
                    Generator(5138, 1973),
                    Carrier(1344, 512)
                ],
                [PastMap("StardustSpeedway", 1)] =
                [
                    Generator(7824, 661),
                    Carrier(3824, 1216)
                ],
                [PastMap("StardustSpeedway", 2)] =
                [
                    Generator(5888, 1365),
                    Carrier(752, 1216)
                ],
                [PastMap("MetallicMadness", 1)] =
                [
                    Carrier(6728, 1152)
                ],
                [PastMap("MetallicMadness", 2)] =
                [
                    Carrier(1352, 1408)
                ]
            };

    public static IReadOnlyList<SonicCDPastObjectLocation> GetLocations(
        ZoneMapDefinition zone)
    {
        return ObjectsByMap.TryGetValue(
            zone.MapResource,
            out IReadOnlyList<SonicCDPastObjectLocation>? locations)
                ? locations
                : [];
    }

    private static string PastMap(
        string zoneFileName,
        int actNumber) =>
        MapPaths.SonicCD.Scene(
            zoneFileName,
            actNumber,
            "Past");

    private static SonicCDPastObjectLocation Generator(
        double x,
        double y) =>
        new(
            SonicCDPastObjectType.MetalSonicGenerator,
            x,
            y);

    private static SonicCDPastObjectLocation Carrier(
        double x,
        double y) =>
        new(
            SonicCDPastObjectType.RobotCarrier,
            x,
            y);
}
