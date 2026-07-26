using Sonic_Tracker.Games;

namespace Sonic_Tracker.Maps;

public static class SceneIndexRegistry
{
    private static readonly IReadOnlyList<SceneIndexDefinition> Scenes =
        BuildScenes();

    public static bool TryFind(
        GameId game,
        int sceneIndex,
        out SceneIndexDefinition? scene)
    {
        scene = Scenes.FirstOrDefault(candidate =>
            candidate.Game == game &&
            candidate.SceneIndex == sceneIndex);

        return scene is not null;
    }

    private static IReadOnlyList<SceneIndexDefinition> BuildScenes()
    {
        var scenes = new List<SceneIndexDefinition>();

        AddSonic1Scenes(scenes);
        AddSonic2Scenes(scenes);
        AddSonicCDScenes(scenes);
        AddSonic3KScenes(scenes);

        return scenes;
    }

    private static void AddSonic1Scenes(
        ICollection<SceneIndexDefinition> scenes)
    {
        Add(scenes, GameId.Sonic1, 0, "GreenHill", "Green Hill Zone", 1);
        Add(scenes, GameId.Sonic1, 1, "GreenHill", "Green Hill Zone", 2);
        Add(scenes, GameId.Sonic1, 2, "GreenHill", "Green Hill Zone", 3);
        Add(scenes, GameId.Sonic1, 3, "Marble", "Marble Zone", 1);
        Add(scenes, GameId.Sonic1, 4, "Marble", "Marble Zone", 2);
        Add(scenes, GameId.Sonic1, 5, "Marble", "Marble Zone", 3);
        Add(scenes, GameId.Sonic1, 6, "SpringYard", "Spring Yard Zone", 1);
        Add(scenes, GameId.Sonic1, 7, "SpringYard", "Spring Yard Zone", 2);
        Add(scenes, GameId.Sonic1, 8, "SpringYard", "Spring Yard Zone", 3);
        Add(scenes, GameId.Sonic1, 9, "Labyrinth", "Labyrinth Zone", 1);
        Add(scenes, GameId.Sonic1, 10, "Labyrinth", "Labyrinth Zone", 2);
        Add(scenes, GameId.Sonic1, 11, "Labyrinth", "Labyrinth Zone", 3);
        Add(scenes, GameId.Sonic1, 12, "StarLight", "Star Light Zone", 1);
        Add(scenes, GameId.Sonic1, 13, "StarLight", "Star Light Zone", 2);
        Add(scenes, GameId.Sonic1, 14, "StarLight", "Star Light Zone", 3);
        Add(scenes, GameId.Sonic1, 15, "ScrapBrain", "Scrap Brain Zone", 1);
        Add(scenes, GameId.Sonic1, 16, "ScrapBrain", "Scrap Brain Zone", 2);
        Add(scenes, GameId.Sonic1, 17, "ScrapBrain", "Scrap Brain Zone", 3);
        Add(scenes, GameId.Sonic1, 18, "Final", "Final Zone", 1);
    }

    private static void AddSonic2Scenes(
        ICollection<SceneIndexDefinition> scenes)
    {
        Add(scenes, GameId.Sonic2, 0, "EmeraldHill", "Emerald Hill Zone", 1);
        Add(scenes, GameId.Sonic2, 1, "EmeraldHill", "Emerald Hill Zone", 2);
        Add(scenes, GameId.Sonic2, 2, "ChemicalPlant", "Chemical Plant Zone", 1);
        Add(scenes, GameId.Sonic2, 3, "ChemicalPlant", "Chemical Plant Zone", 2);
        Add(scenes, GameId.Sonic2, 4, "AquaticRuin", "Aquatic Ruin Zone", 1);
        Add(scenes, GameId.Sonic2, 5, "AquaticRuin", "Aquatic Ruin Zone", 2);
        Add(scenes, GameId.Sonic2, 6, "CasinoNight", "Casino Night Zone", 1);
        Add(scenes, GameId.Sonic2, 7, "CasinoNight", "Casino Night Zone", 2);
        Add(scenes, GameId.Sonic2, 8, "HillTop", "Hill Top Zone", 1);
        Add(scenes, GameId.Sonic2, 9, "HillTop", "Hill Top Zone", 2);
        Add(scenes, GameId.Sonic2, 10, "MysticCave", "Mystic Cave Zone", 1);
        Add(scenes, GameId.Sonic2, 11, "MysticCave", "Mystic Cave Zone", 2);
        Add(scenes, GameId.Sonic2, 12, "OilOcean", "Oil Ocean Zone", 1);
        Add(scenes, GameId.Sonic2, 13, "OilOcean", "Oil Ocean Zone", 2);
        Add(scenes, GameId.Sonic2, 14, "Metropolis", "Metropolis Zone", 1);
        Add(scenes, GameId.Sonic2, 15, "Metropolis", "Metropolis Zone", 2);
        Add(scenes, GameId.Sonic2, 16, "Metropolis", "Metropolis Zone", 3);
        Add(scenes, GameId.Sonic2, 17, "SkyChase", "Sky Chase Zone", null);
        Add(scenes, GameId.Sonic2, 18, "WingFortress", "Wing Fortress Zone", null);
        Add(scenes, GameId.Sonic2, 19, "DeathEgg", "Death Egg Zone", 1);
        Add(scenes, GameId.Sonic2, 20, "HiddenPalace", "Hidden Palace Zone", null);
    }

    private static void AddSonicCDScenes(
        ICollection<SceneIndexDefinition> scenes)
    {
        (string Id, string Name, int BaseIndex)[] zones =
        [
            ("PalmtreePanic", "Palmtree Panic Zone", 0),
            ("CollisionChaos", "Collision Chaos Zone", 10),
            ("TidalTempest", "Tidal Tempest Zone", 20),
            ("QuartzQuadrant", "Quartz Quadrant Zone", 30),
            ("WackyWorkbench", "Wacky Workbench Zone", 40),
            ("StardustSpeedway", "Stardust Speedway Zone", 50),
            ("MetallicMadness", "Metallic Madness Zone", 60)
        ];

        (string Name, int Offset)[] periods =
        [
            ("Present", 0),
            ("Past", 1),
            ("Good Future", 2),
            ("Bad Future", 3)
        ];

        foreach ((string zoneId, string zoneName, int baseIndex) in zones)
        {
            foreach ((string period, int periodOffset) in periods)
            {
                Add(
                    scenes,
                    GameId.SonicCD,
                    baseIndex + periodOffset,
                    zoneId,
                    zoneName,
                    1,
                    period);

                Add(
                    scenes,
                    GameId.SonicCD,
                    baseIndex + 4 + periodOffset,
                    zoneId,
                    zoneName,
                    2,
                    period);
            }

            Add(
                scenes,
                GameId.SonicCD,
                baseIndex + 8,
                zoneId,
                zoneName,
                3,
                "Good Future");

            Add(
                scenes,
                GameId.SonicCD,
                baseIndex + 9,
                zoneId,
                zoneName,
                3,
                "Bad Future");
        }
    }

    private static void AddSonic3KScenes(
        ICollection<SceneIndexDefinition> scenes)
    {
        Add(scenes, GameId.Sonic3AndKnuckles, 0, "AngelIsland", "Angel Island Zone", 1, "Normal");
        Add(scenes, GameId.Sonic3AndKnuckles, 1, "AngelIsland", "Angel Island Zone", 2);
        Add(scenes, GameId.Sonic3AndKnuckles, 2, "Hydrocity", "Hydrocity Zone", 1);
        Add(scenes, GameId.Sonic3AndKnuckles, 3, "Hydrocity", "Hydrocity Zone", 2);
        Add(scenes, GameId.Sonic3AndKnuckles, 4, "MarbleGarden", "Marble Garden Zone", 1);
        Add(scenes, GameId.Sonic3AndKnuckles, 5, "MarbleGarden", "Marble Garden Zone", 2);
        Add(scenes, GameId.Sonic3AndKnuckles, 6, "CarnivalNight", "Carnival Night Zone", 1);
        Add(scenes, GameId.Sonic3AndKnuckles, 7, "CarnivalNight", "Carnival Night Zone", 2);
        Add(scenes, GameId.Sonic3AndKnuckles, 8, "IceCap", "IceCap Zone", 1);
        Add(scenes, GameId.Sonic3AndKnuckles, 9, "IceCap", "IceCap Zone", 2);
        Add(scenes, GameId.Sonic3AndKnuckles, 10, "LaunchBase", "Launch Base Zone", 1);
        Add(scenes, GameId.Sonic3AndKnuckles, 11, "LaunchBase", "Launch Base Zone", 2);
        Add(scenes, GameId.Sonic3AndKnuckles, 12, "MushroomHill", "Mushroom Hill Zone", 1);
        Add(scenes, GameId.Sonic3AndKnuckles, 13, "MushroomHill", "Mushroom Hill Zone", 2);
        Add(scenes, GameId.Sonic3AndKnuckles, 14, "FlyingBattery", "Flying Battery Zone", 1);
        Add(scenes, GameId.Sonic3AndKnuckles, 15, "FlyingBattery", "Flying Battery Zone", 2);
        Add(scenes, GameId.Sonic3AndKnuckles, 16, "Sandopolis", "Sandopolis Zone", 1);
        Add(scenes, GameId.Sonic3AndKnuckles, 17, "Sandopolis", "Sandopolis Zone", 2);
        Add(scenes, GameId.Sonic3AndKnuckles, 18, "LavaReef", "Lava Reef Zone", 1);
        Add(scenes, GameId.Sonic3AndKnuckles, 19, "LavaReef", "Lava Reef Zone", 2);
        Add(scenes, GameId.Sonic3AndKnuckles, 20, "LavaReefBoss", "Lava Reef Final Boss", null);
        Add(scenes, GameId.Sonic3AndKnuckles, 21, "HiddenPalace3K", "Hidden Palace Zone", 1);
        Add(scenes, GameId.Sonic3AndKnuckles, 22, "SkySanctuarySonic", "Sky Sanctuary Zone", null, "Sonic Route");
        Add(scenes, GameId.Sonic3AndKnuckles, 23, "SkySanctuaryKnuckles", "Sky Sanctuary Zone", null, "Knuckles Route");
        Add(scenes, GameId.Sonic3AndKnuckles, 24, "DeathEgg3K", "Death Egg Zone", 1);
        Add(scenes, GameId.Sonic3AndKnuckles, 25, "DeathEgg3K", "Death Egg Zone", 2);
        Add(scenes, GameId.Sonic3AndKnuckles, 26, "DeathEggBoss", "Death Egg Final Boss", null);
        Add(scenes, GameId.Sonic3AndKnuckles, 27, "Doomsday", "Doomsday Zone", null);
    }

    private static void Add(
        ICollection<SceneIndexDefinition> scenes,
        GameId game,
        int sceneIndex,
        string zoneId,
        string displayName,
        int? actNumber,
        string? variant = null)
    {
        scenes.Add(
            new SceneIndexDefinition(
                game,
                sceneIndex,
                zoneId,
                displayName,
                actNumber,
                variant));
    }
}
