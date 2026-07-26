using System.IO;
using Sonic_Tracker.Audio;

namespace Sonic_Tracker.Games;

public sealed class Sonic3KAudioProfile : IGameAudioProfile
{
    public GameId Game => GameId.Sonic3AndKnuckles;
    public string DisplayName => "Sonic 3 & Knuckles";
    public IReadOnlyList<string> ProcessNames { get; } =
        ["SonicOrigins"];

    public IReadOnlyList<AudioTrackDefinition> Tracks { get; }

    public Sonic3KAudioProfile()
    {
        string audioRoot = Path.Combine(
            AppContext.BaseDirectory,
            "Assets",
            "Audio",
            "Sonic3K",
            "Misc");

        Tracks =
        [
            Track(
                "S3K_BLUE_SPHERES",
                "Blue Spheres Special Stage",
                "BlueSpheres.wav"),
            Track(
                "S3K_GLOWBALL_BONUS",
                "Glowing Spheres Bonus Stage",
                "SpecialStageGlowball.wav"),
            Track(
                "S3K_GUMBALL_BONUS",
                "Gumball Machine Bonus Stage",
                "SpecialStageGumballMachine.wav"),
            Track(
                "S3K_SLOT_MACHINE_BONUS",
                "Slot Machine Bonus Stage",
                "SpecialStageSlotMachine.wav"),
            new AudioTrackDefinition(
                TrackId: "S3K_LEVEL_SELECT",
                Game: GameId.Sonic3AndKnuckles,
                DisplayName: "Sonic 3 & Knuckles Level Select",
                ZoneId: "LevelSelect",
                Kind: AudioTrackKind.Menu,
                ReferenceFilePath: Path.Combine(
                    audioRoot,
                    "LevelSelect.wav")),
            new AudioTrackDefinition(
                TrackId: "S3K_TITLE_SCREEN",
                Game: GameId.Sonic3AndKnuckles,
                DisplayName: "Sonic 3 & Knuckles Title Screen",
                ZoneId: "TitleScreen",
                Kind: AudioTrackKind.Menu,
                ReferenceFilePath: Path.Combine(
                    audioRoot,
                    "TitleScreen.wav"))
        ];

        AudioTrackDefinition Track(
            string id,
            string name,
            string file) =>
            new(
                id,
                GameId.Sonic3AndKnuckles,
                name,
                "SpecialStage",
                AudioTrackKind.Temporary,
                Path.Combine(audioRoot, file));
    }
}
