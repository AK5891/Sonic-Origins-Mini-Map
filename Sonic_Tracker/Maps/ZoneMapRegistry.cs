using Sonic_Tracker.Games;

namespace Sonic_Tracker.Maps;

public static class ZoneMapRegistry
{
    private static readonly IReadOnlyList<ZoneMapDefinition> Zones =
        BuildZones();

    public static ZoneMapDefinition DefaultZone =>
        Zones[0];

    public static IReadOnlyList<ZoneMapDefinition> All =>
        Zones;

    public static bool TryFindByTrackName(
        string trackName,
        out ZoneMapDefinition? zone)
    {
        zone = Zones.FirstOrDefault(candidate =>
            candidate.MatchesTrackName(trackName));

        return zone is not null;
    }

    public static bool TryFind(
        string zoneId,
        int actNumber,
        out ZoneMapDefinition? zone)
    {
        zone = Zones.FirstOrDefault(candidate =>
            string.Equals(
                candidate.ZoneId,
                zoneId,
                StringComparison.OrdinalIgnoreCase) &&
            candidate.ActNumber == actNumber);

        return zone is not null;
    }

    public static bool TryFind(
        GameId game,
        string zoneId,
        int? actNumber,
        string? variant,
        out ZoneMapDefinition? zone)
    {
        zone = Zones.FirstOrDefault(candidate =>
            candidate.Game == game &&
            string.Equals(
                candidate.ZoneId,
                zoneId,
                StringComparison.OrdinalIgnoreCase) &&
            candidate.ActNumber == actNumber &&
            string.Equals(
                candidate.Variant,
                variant,
                StringComparison.OrdinalIgnoreCase));

        return zone is not null;
    }

    private static IReadOnlyList<ZoneMapDefinition> BuildZones()
    {
        var zones = new List<ZoneMapDefinition>();

        AddSonic2Zones(zones);
        AddSonic1Zones(zones);
        AddSonicCDZones(zones);
        AddSonic3KZones(zones);

        return zones;
    }

    private static void AddSonic2Zones(
        ICollection<ZoneMapDefinition> zones)
    {
        Add(zones, GameId.Sonic2, "EmeraldHill", "Emerald Hill Zone", 1, MapPaths.Sonic2.EmeraldHillAct1, null, "Emerald Hill");
        Add(zones, GameId.Sonic2, "EmeraldHill", "Emerald Hill Zone", 2, MapPaths.Sonic2.EmeraldHillAct2, null, "Emerald Hill");
        Add(zones, GameId.Sonic2, "ChemicalPlant", "Chemical Plant Zone", 1, MapPaths.Sonic2.ChemicalPlantAct1, null, "Chemical Plant");
        Add(zones, GameId.Sonic2, "ChemicalPlant", "Chemical Plant Zone", 2, MapPaths.Sonic2.ChemicalPlantAct2, null, "Chemical Plant");
        Add(zones, GameId.Sonic2, "AquaticRuin", "Aquatic Ruin Zone", 1, MapPaths.Sonic2.AquaticRuinAct1, null, "Aquatic Ruin");
        Add(zones, GameId.Sonic2, "AquaticRuin", "Aquatic Ruin Zone", 2, MapPaths.Sonic2.AquaticRuinAct2, null, "Aquatic Ruin");
        Add(zones, GameId.Sonic2, "CasinoNight", "Casino Night Zone", 1, MapPaths.Sonic2.CasinoNightAct1, null, "Casino Night");
        Add(zones, GameId.Sonic2, "CasinoNight", "Casino Night Zone", 2, MapPaths.Sonic2.CasinoNightAct2, null, "Casino Night");
        Add(zones, GameId.Sonic2, "HillTop", "Hill Top Zone", 1, MapPaths.Sonic2.HillTopAct1, null, "Hill Top");
        Add(zones, GameId.Sonic2, "HillTop", "Hill Top Zone", 2, MapPaths.Sonic2.HillTopAct2, null, "Hill Top");
        Add(zones, GameId.Sonic2, "MysticCave", "Mystic Cave Zone", 1, MapPaths.Sonic2.MysticCaveAct1, null, "Mystic Cave");
        Add(zones, GameId.Sonic2, "MysticCave", "Mystic Cave Zone", 2, MapPaths.Sonic2.MysticCaveAct2, null, "Mystic Cave");
        Add(zones, GameId.Sonic2, "HiddenPalace", "Hidden Palace Zone", null, MapPaths.Sonic2.HiddenPalace, null, "Hidden Palace");
        Add(zones, GameId.Sonic2, "OilOcean", "Oil Ocean Zone", 1, MapPaths.Sonic2.OilOceanAct1, null, "Oil Ocean");
        Add(zones, GameId.Sonic2, "OilOcean", "Oil Ocean Zone", 2, MapPaths.Sonic2.OilOceanAct2, null, "Oil Ocean");
        Add(zones, GameId.Sonic2, "Metropolis", "Metropolis Zone", 1, MapPaths.Sonic2.MetropolisAct1, null, "Metropolis");
        Add(zones, GameId.Sonic2, "Metropolis", "Metropolis Zone", 2, MapPaths.Sonic2.MetropolisAct2, null, "Metropolis");
        Add(zones, GameId.Sonic2, "Metropolis", "Metropolis Zone", 3, MapPaths.Sonic2.MetropolisAct3, null, "Metropolis");
        Add(zones, GameId.Sonic2, "WingFortress", "Wing Fortress Zone", null, MapPaths.Sonic2.WingFortressAct1, null, "Wing Fortress");
        Add(zones, GameId.Sonic2, "DeathEgg", "Death Egg Zone", 1, MapPaths.Sonic2.DeathEggAct1, null, "Death Egg");
    }

    private static void AddSonic1Zones(
        ICollection<ZoneMapDefinition> zones)
    {
        Add(zones, GameId.Sonic1, "GreenHill", "Green Hill Zone", 1, MapPaths.Sonic1.GreenHillAct1, null, "Green Hill");
        Add(zones, GameId.Sonic1, "GreenHill", "Green Hill Zone", 2, MapPaths.Sonic1.GreenHillAct2, null, "Green Hill");
        Add(zones, GameId.Sonic1, "GreenHill", "Green Hill Zone", 3, MapPaths.Sonic1.GreenHillAct3, null, "Green Hill");
        Add(zones, GameId.Sonic1, "Marble", "Marble Zone", 1, MapPaths.Sonic1.MarbleAct1, null, "Marble");
        Add(zones, GameId.Sonic1, "Marble", "Marble Zone", 2, MapPaths.Sonic1.MarbleAct2, null, "Marble");
        Add(zones, GameId.Sonic1, "Marble", "Marble Zone", 3, MapPaths.Sonic1.MarbleAct3, null, "Marble");
        Add(zones, GameId.Sonic1, "SpringYard", "Spring Yard Zone", 1, MapPaths.Sonic1.SpringYardAct1, null, "Spring Yard");
        Add(zones, GameId.Sonic1, "SpringYard", "Spring Yard Zone", 2, MapPaths.Sonic1.SpringYardAct2, null, "Spring Yard");
        Add(zones, GameId.Sonic1, "SpringYard", "Spring Yard Zone", 3, MapPaths.Sonic1.SpringYardAct3, null, "Spring Yard");
        Add(zones, GameId.Sonic1, "Labyrinth", "Labyrinth Zone", 1, MapPaths.Sonic1.LabyrinthAct1, null, "Labyrinth");
        Add(zones, GameId.Sonic1, "Labyrinth", "Labyrinth Zone", 2, MapPaths.Sonic1.LabyrinthAct2, null, "Labyrinth");
        Add(zones, GameId.Sonic1, "Labyrinth", "Labyrinth Zone", 3, MapPaths.Sonic1.LabyrinthAct3, null, "Labyrinth");
        Add(zones, GameId.Sonic1, "StarLight", "Star Light Zone", 1, MapPaths.Sonic1.StarLightAct1, null, "Star Light");
        Add(zones, GameId.Sonic1, "StarLight", "Star Light Zone", 2, MapPaths.Sonic1.StarLightAct2, null, "Star Light");
        Add(zones, GameId.Sonic1, "StarLight", "Star Light Zone", 3, MapPaths.Sonic1.StarLightAct3, null, "Star Light");
        Add(zones, GameId.Sonic1, "ScrapBrain", "Scrap Brain Zone", 1, MapPaths.Sonic1.ScrapBrainAct1, null, "Scrap Brain");
        Add(zones, GameId.Sonic1, "ScrapBrain", "Scrap Brain Zone", 2, MapPaths.Sonic1.ScrapBrainAct2, null, "Scrap Brain");
        Add(zones, GameId.Sonic1, "ScrapBrain", "Scrap Brain Zone", 3, MapPaths.Sonic1.ScrapBrainAct3, null, "Scrap Brain");
    }

    private static void AddSonicCDZones(
        ICollection<ZoneMapDefinition> zones)
    {
        (string Id, string Name, string FileName)[] zoneData =
        [
            ("PalmtreePanic", "Palmtree Panic Zone", "PalmtreePanic"),
            ("CollisionChaos", "Collision Chaos Zone", "CollisionChaos"),
            ("TidalTempest", "Tidal Tempest Zone", "TIdalTempest"),
            ("QuartzQuadrant", "Quartz Quadrant Zone", "QuartzQuadrant"),
            ("WackyWorkbench", "Wacky Workbench Zone", "WackyWorkbench"),
            ("StardustSpeedway", "Stardust Speedway Zone", "StardustSpeedway"),
            ("MetallicMadness", "Metallic Madness Zone", "MetallicMadness")
        ];

        (string DisplayName, string FileName)[] periods =
        [
            ("Present", "Present"),
            ("Past", "Past"),
            ("Good Future", "GoodFuture"),
            ("Bad Future", "BadFuture")
        ];

        foreach ((string zoneId, string zoneName, string fileName) in zoneData)
        {
            foreach ((string period, string periodFileName) in periods)
            {
                Add(
                    zones,
                    GameId.SonicCD,
                    zoneId,
                    zoneName,
                    1,
                    MapPaths.SonicCD.Scene(fileName, 1, periodFileName),
                    period);

                Add(
                    zones,
                    GameId.SonicCD,
                    zoneId,
                    zoneName,
                    2,
                    MapPaths.SonicCD.Scene(fileName, 2, periodFileName),
                    period);
            }

            Add(
                zones,
                GameId.SonicCD,
                zoneId,
                zoneName,
                3,
                MapPaths.SonicCD.Scene(fileName, 3, "GoodFuture"),
                "Good Future");

            Add(
                zones,
                GameId.SonicCD,
                zoneId,
                zoneName,
                3,
                MapPaths.SonicCD.Scene(fileName, 3, "BadFuture"),
                "Bad Future");
        }
    }

    private static void AddSonic3KZones(
        ICollection<ZoneMapDefinition> zones)
    {
        Add3K(
            zones,
            "AngelIsland",
            "Angel Island Zone",
            1,
            "AngelIslandZone1",
            variant: "Normal",
            mapOffsetX: -4865,
            mapOffsetY: -125);
        Add3K(
            zones,
            "AngelIsland",
            "Angel Island Zone",
            1,
            "AngelIslandZone1",
            variant: "Burnt",
            mapOffsetX: 7168,
            mapOffsetY: 1);
        Add3K(
            zones,
            "AngelIsland",
            "Angel Island Zone",
            2,
            "AngelIslandZone2",
            variant: null,
            mapOffsetX: -4226,
            mapOffsetY: 0);
        Add3K(
            zones,
            "Hydrocity",
            "Hydrocity Zone",
            1,
            "HydrocityZone1",
            variant: null,
            mapOffsetX: 1,
            mapOffsetY: 0);
        Add3K(
            zones,
            "Hydrocity",
            "Hydrocity Zone",
            2,
            "HydrocityZone2",
            variant: null,
            mapOffsetX: 0,
            mapOffsetY: -2);
        Add3K(zones, "MarbleGarden", "Marble Garden Zone", 1, "MarbleGardenZone1");
        Add3K(
            zones,
            "MarbleGarden",
            "Marble Garden Zone",
            2,
            "MarbleGardenZone2",
            variant: null,
            mapOffsetX: -256,
            mapOffsetY: -1);
        Add3K(zones, "CarnivalNight", "Carnival Night Zone", 1, "CarnivalNightZone1");
        Add3K(zones, "CarnivalNight", "Carnival Night Zone", 2, "CarnivalNightZone2");
        Add3K(
            zones,
            "IceCap",
            "IceCap Zone",
            1,
            "IceCapZone1",
            variant: null,
            mapOffsetX: -1,
            mapOffsetY: -193);
        Add3K(
            zones,
            "IceCap",
            "IceCap Zone",
            2,
            "IceCapZone2",
            variant: null,
            mapOffsetX: -1857,
            mapOffsetY: -1);
        Add3K(zones, "LaunchBase", "Launch Base Zone", 1, "LaunchBaseZone1");
        Add3K(
            zones,
            "LaunchBase",
            "Launch Base Zone",
            2,
            "LaunchBaseZone2",
            variant: null,
            mapOffsetX: -256,
            mapOffsetY: -1);
        Add3K(zones, "MushroomHill", "Mushroom Hill Zone", 1, "MushroomHillZone1");
        Add3K(zones, "MushroomHill", "Mushroom Hill Zone", 2, "MushroomHillZone2");
        Add3K(zones, "FlyingBattery", "Flying Battery Zone", 1, "FlyingBatteryZone1");
        Add3K(zones, "FlyingBattery", "Flying Battery Zone", 2, "FlyingBatteryZone2");
        Add3K(zones, "Sandopolis", "Sandopolis Zone", 1, "SandopolisZone1");
        Add3K(zones, "Sandopolis", "Sandopolis Zone", 2, "SandopolisZone2");
        Add3K(zones, "LavaReef", "Lava Reef Zone", 1, "LavaReefZone1");
        Add3K(zones, "LavaReef", "Lava Reef Zone", 2, "LavaReefZone2");
        Add3K(zones, "LavaReefBoss", "Lava Reef Final Boss", null, "LavaReefZoneBoss");
        Add3K(zones, "HiddenPalace3K", "Hidden Palace Zone", 1, "HiddenPalaceZone1");
        Add3K(
            zones,
            "SkySanctuarySonic",
            "Sky Sanctuary Zone",
            null,
            "SkySanctuaryZoneSonic",
            variant: "Sonic Route",
            mapOffsetX: 0,
            mapOffsetY: -3457);
        Add3K(zones, "SkySanctuaryKnuckles", "Sky Sanctuary Zone", null, "SkySanctuaryZoneKnuckles", "Knuckles Route");
        Add3K(zones, "DeathEgg3K", "Death Egg Zone", 1, "DeathEggZone1");
        Add3K(zones, "DeathEgg3K", "Death Egg Zone", 2, "DeathEggZone2");
        Add3K(zones, "Doomsday", "Doomsday Zone", null, "DoomsdayZone1");
    }

    private static void Add3K(
        ICollection<ZoneMapDefinition> zones,
        string zoneId,
        string zoneName,
        int? actNumber,
        string fileName,
        string? variant = null,
        double mapOffsetX = 0,
        double mapOffsetY = 0)
    {
        AddCore(
            zones,
            GameId.Sonic3AndKnuckles,
            zoneId,
            zoneName,
            actNumber,
            MapPaths.Sonic3K.Scene(fileName),
            variant,
            mapOffsetX,
            mapOffsetY,
            []);
    }

    private static void Add(
        ICollection<ZoneMapDefinition> zones,
        GameId game,
        string zoneId,
        string zoneName,
        int? actNumber,
        string mapResource,
        string? variant,
        params string[] trackNameKeywords)
    {
        AddCore(
            zones,
            game,
            zoneId,
            zoneName,
            actNumber,
            mapResource,
            variant,
            0,
            0,
            trackNameKeywords);
    }

    private static void AddCore(
        ICollection<ZoneMapDefinition> zones,
        GameId game,
        string zoneId,
        string zoneName,
        int? actNumber,
        string mapResource,
        string? variant,
        double mapOffsetX,
        double mapOffsetY,
        string[] trackNameKeywords)
    {
        zones.Add(
            new ZoneMapDefinition(
                game,
                zoneId,
                zoneName,
                actNumber,
                GetGameName(game),
                mapResource,
                variant,
                mapOffsetX,
                mapOffsetY,
                trackNameKeywords));
    }

    private static string GetGameName(
        GameId game) =>
        game switch
        {
            GameId.Sonic1 => "Sonic the Hedgehog",
            GameId.SonicCD => "Sonic CD",
            GameId.Sonic2 => "Sonic the Hedgehog 2",
            GameId.Sonic3AndKnuckles => "Sonic 3 & Knuckles",
            _ => "Sonic Origins"
        };
}
