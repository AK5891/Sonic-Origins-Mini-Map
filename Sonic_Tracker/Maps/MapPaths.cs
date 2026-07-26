namespace Sonic_Tracker.Maps;

public static class MapPaths
{
    public static class Sonic1
    {
        public const string GreenHillAct1 = "assets/Maps/Sonic1/GreenHillZone1.png";
        public const string GreenHillAct2 = "assets/Maps/Sonic1/GreenHillZone2.png";
        public const string GreenHillAct3 = "assets/Maps/Sonic1/GreenHillZone3.png";
        public const string MarbleAct1 = "assets/Maps/Sonic1/MarbleZone1.png";
        public const string MarbleAct2 = "assets/Maps/Sonic1/MarbleZone2.png";
        public const string MarbleAct3 = "assets/Maps/Sonic1/MarbleZone3.png";
        public const string SpringYardAct1 = "assets/Maps/Sonic1/SpringYardZone1.png";
        public const string SpringYardAct2 = "assets/Maps/Sonic1/SpringYardZone2.png";
        public const string SpringYardAct3 = "assets/Maps/Sonic1/SpringYardZone3.png";
        public const string LabyrinthAct1 = "assets/Maps/Sonic1/LabyrinthZone1.png";
        public const string LabyrinthAct2 = "assets/Maps/Sonic1/LabyrinthZone2.png";
        public const string LabyrinthAct3 = "assets/Maps/Sonic1/LabyrinthZone3.png";
        public const string StarLightAct1 = "assets/Maps/Sonic1/StarLightZone1.png";
        public const string StarLightAct2 = "assets/Maps/Sonic1/StarLightZone2.png";
        public const string StarLightAct3 = "assets/Maps/Sonic1/StarLightZone3.png";
        public const string ScrapBrainAct1 = "assets/Maps/Sonic1/ScrapBrainZone1.png";
        public const string ScrapBrainAct2 = "assets/Maps/Sonic1/ScrapBrainZone2.png";
        public const string ScrapBrainAct3 = "assets/Maps/Sonic1/ScrapBrainZone3.png";
        public const string SpecialStage1 = "assets/Maps/Sonic1/SpecialStage1.png";
        public const string SpecialStage2 = "assets/Maps/Sonic1/SpecialStage2.png";
        public const string SpecialStage3 = "assets/Maps/Sonic1/SpecialStage3.png";
        public const string SpecialStage4 = "assets/Maps/Sonic1/SpecialStage4.png";
        public const string SpecialStage5 = "assets/Maps/Sonic1/SpecialStage5.png";
        public const string SpecialStage6 = "assets/Maps/Sonic1/SpecialStage6.png";
    }

    public static class Sonic2
    {
        public const string EmeraldHillAct1 =
            "assets/Maps/Sonic2/EmeraldHillZone1.png";

        public const string EmeraldHillAct2 =
            "assets/Maps/Sonic2/EmeraldHillZone2.png";

        public const string ChemicalPlantAct1 =
            "assets/Maps/Sonic2/ChemicalPlantZone1.png";

        public const string ChemicalPlantAct2 =
            "assets/Maps/Sonic2/ChemicalPlantZone2.png";

        public const string AquaticRuinAct1 =
            "assets/Maps/Sonic2/AquaticRuinZone1.png";

        public const string AquaticRuinAct2 =
            "assets/Maps/Sonic2/AquaticRuinZone2.png";

        public const string CasinoNightAct1 =
            "assets/Maps/Sonic2/CasinoNightZone1.png";

        public const string CasinoNightAct2 =
            "assets/Maps/Sonic2/CasinoNightZone2.png";

        public const string HillTopAct1 =
            "assets/Maps/Sonic2/HillTopZone1.png";

        public const string HillTopAct2 =
            "assets/Maps/Sonic2/HillTopZone2.png";

        public const string MysticCaveAct1 =
            "assets/Maps/Sonic2/MysticCaveZone1.png";

        public const string MysticCaveAct2 =
            "assets/Maps/Sonic2/MysticCaveZone2.png";

        public const string HiddenPalace =
            "assets/Maps/Sonic2/HiddenPalaceZone.png";

        public const string OilOceanAct1 =
            "assets/Maps/Sonic2/OilOceanZone1.png";

        public const string OilOceanAct2 =
            "assets/Maps/Sonic2/OilOceanZone2.png";

        public const string MetropolisAct1 =
            "assets/Maps/Sonic2/MetropolisZone1.png";

        public const string MetropolisAct2 =
            "assets/Maps/Sonic2/MetropolisZone2.png";

        public const string MetropolisAct3 =
            "assets/Maps/Sonic2/MetropolisZone3.png";

        public const string WingFortressAct1 =
            "assets/Maps/Sonic2/WingedFortressZone1.png";

        public const string DeathEggAct1 =
            "assets/Maps/Sonic2/DeathEggZone1.png";
    }

    public static class SonicCD
    {
        public static string Scene(
            string zoneFileName,
            int actNumber,
            string periodFileName) =>
            $"assets/Maps/SonicCD/{zoneFileName}Zone{actNumber}{periodFileName}.png";
    }

    public static class Sonic3K
    {
        public static string Scene(
            string fileName) =>
            $"assets/Maps/Sonic3K/{fileName}.png";
    }
}
