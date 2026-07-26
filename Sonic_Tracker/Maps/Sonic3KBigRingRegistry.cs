namespace Sonic_Tracker.Maps;

public static class Sonic3KBigRingRegistry
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyList<BigRingLocation>>
        RingsByMap =
            new Dictionary<string, IReadOnlyList<BigRingLocation>>(
                StringComparer.OrdinalIgnoreCase)
            {
                [MapPaths.Sonic3K.Scene("AngelIslandZone1")] =
                [
                    new(2216, 1055),
                    new(7712, 943)
                ],
                [MapPaths.Sonic3K.Scene("AngelIslandZone2")] =
                [
                    new(2328, 1071),
                    new(9752, 1455)
                ],
                [MapPaths.Sonic3K.Scene("HydrocityZone1")] =
                [
                    new(5152, 1439),
                    new(12576, 1175)
                ],
                [MapPaths.Sonic3K.Scene("HydrocityZone2")] =
                [
                    new(9888, 1567),
                    new(14240, 1183)
                ],
                [MapPaths.Sonic3K.Scene("MarbleGardenZone1")] =
                [
                    new(1695, 2331),
                    new(3359, 1947),
                    new(3743, 3227),
                    new(3999, 2587),
                    new(5407, 539),
                    new(6303, 2331),
                    new(6815, 1179),
                    new(9503, 539)
                ],
                [MapPaths.Sonic3K.Scene("MarbleGardenZone2")] =
                [
                    new(2079, 1183),
                    new(5407, 2335),
                    new(9759, 1695)
                ],
                [MapPaths.Sonic3K.Scene("CarnivalNightZone1")] =
                [
                    new(3232, 299),
                    new(3488, 2087),
                    new(6944, 939),
                    new(9120, 551),
                    new(11680, 1579),
                    new(11936, 2731)
                ],
                [MapPaths.Sonic3K.Scene("CarnivalNightZone2")] =
                [
                    new(3872, 2091),
                    new(5024, 555),
                    new(5664, 1323),
                    new(12448, 683),
                    new(17824, 2475)
                ],
                [MapPaths.Sonic3K.Scene("IceCapZone1")] =
                [
                    new(17048, 1383),
                    new(18200, 231)
                ],
                [MapPaths.Sonic3K.Scene("IceCapZone2")] =
                [
                    new(3296, 2855),
                    new(10848, 2855),
                    new(14816, 1063)
                ],
                [MapPaths.Sonic3K.Scene("LaunchBaseZone1")] =
                [
                    new(672, 1823),
                    new(2208, 287),
                    new(9120, 2079)
                ],
                [MapPaths.Sonic3K.Scene("LaunchBaseZone2")] =
                [
                    new(1440, 1695),
                    new(2208, 415),
                    new(7200, 2719),
                    new(12064, 543),
                    new(12192, 1183)
                ],
                [MapPaths.Sonic3K.Scene("MushroomHillZone1")] =
                [
                    new(416, 1631),
                    new(1184, 2527),
                    new(8736, 1055),
                    new(11552, 1695),
                    new(14496, 1823)
                ],
                [MapPaths.Sonic3K.Scene("MushroomHillZone2")] =
                [
                    new(4128, 927),
                    new(7328, 1567),
                    new(9120, 2079),
                    new(10528, 671),
                    new(11680, 2463),
                    new(14752, 1567)
                ],
                [MapPaths.Sonic3K.Scene("FlyingBatteryZone1")] =
                [
                    new(7456, 1439),
                    new(9376, 1951)
                ],
                [MapPaths.Sonic3K.Scene("FlyingBatteryZone2")] =
                [
                    new(6176, 1823),
                    new(9632, 1823)
                ],
                [MapPaths.Sonic3K.Scene("SandopolisZone1")] =
                [
                    new(2720, 1935),
                    new(6432, 1423),
                    new(7200, 2191),
                    new(9504, 271),
                    new(10400, 1295),
                    new(14176, 1983)
                ],
                [MapPaths.Sonic3K.Scene("SandopolisZone2")] =
                [
                    new(1696, 1679),
                    new(5408, 1423),
                    new(14624, 1503),
                    new(20512, 799)
                ],
                [MapPaths.Sonic3K.Scene("LavaReefZone1")] =
                [
                    new(4383, 1691),
                    new(5791, 1439),
                    new(6687, 539)
                ],
                [MapPaths.Sonic3K.Scene("LavaReefZone2")] =
                [
                    new(5152, 2847),
                    new(5536, 1695),
                    new(7072, 2079),
                    new(8480, 415),
                    new(9760, 1567)
                ]
            };

    public static IReadOnlyList<BigRingLocation> GetLocations(
        ZoneMapDefinition zone)
    {
        if (zone.Game != Games.GameId.Sonic3AndKnuckles)
        {
            return [];
        }

        return RingsByMap.TryGetValue(
            zone.MapResource,
            out IReadOnlyList<BigRingLocation>? locations)
                ? locations
                : [];
    }
}
