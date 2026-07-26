using System.IO;
using Sonic_Tracker.Audio;

namespace Sonic_Tracker.Games;

public sealed class Sonic2AudioProfile : IGameAudioProfile
{
    public GameId Game => GameId.Sonic2;
    public string DisplayName => "Sonic the Hedgehog 2";
    public IReadOnlyList<string> ProcessNames { get; } =
        ["SonicOrigins"];

    public IReadOnlyList<AudioTrackDefinition> Tracks { get; }

    public Sonic2AudioProfile()
    {
        string audioRoot = Path.Combine(
            AppContext.BaseDirectory,
            "Assets",
            "Audio",
            "Sonic2",
            "Misc");

        Tracks =
        [
            new AudioTrackDefinition(
                TrackId: "S2_SPECIAL_STAGE",
                Game: GameId.Sonic2,
                DisplayName: "Sonic the Hedgehog 2 Special Stage",
                ZoneId: "SpecialStage",
                Kind: AudioTrackKind.Temporary,
                ReferenceFilePath: Path.Combine(
                    audioRoot,
                    "SpecialStage.wav")),
            new AudioTrackDefinition(
                TrackId: "S2_TITLE_SCREEN",
                Game: GameId.Sonic2,
                DisplayName: "Sonic the Hedgehog 2 Title Screen",
                ZoneId: "TitleScreen",
                Kind: AudioTrackKind.Menu,
                ReferenceFilePath: Path.Combine(
                    audioRoot,
                    "TitleScreen.wav"))
        ];
    }
}
