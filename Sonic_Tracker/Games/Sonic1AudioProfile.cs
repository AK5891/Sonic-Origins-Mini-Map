using System.IO;
using Sonic_Tracker.Audio;

namespace Sonic_Tracker.Games;

public sealed class Sonic1AudioProfile : IGameAudioProfile
{
    public GameId Game => GameId.Sonic1;
    public string DisplayName => "Sonic the Hedgehog";

    public IReadOnlyList<string> ProcessNames { get; } = ["SonicOrigins"];
    public IReadOnlyList<AudioTrackDefinition> Tracks { get; }

    public Sonic1AudioProfile()
    {
        string audioRoot = Path.Combine(
            AppContext.BaseDirectory, "Assets", "Audio", "Sonic1");

        Tracks =
        [
            new AudioTrackDefinition(
                TrackId: "S1_SPECIAL_STAGE",
                Game: GameId.Sonic1,
                DisplayName: "Sonic the Hedgehog Special Stage",
                ZoneId: "SpecialStage",
                Kind: AudioTrackKind.Temporary,
                ReferenceFilePath: Path.Combine(
                    audioRoot,
                    "Misc",
                    "SpecialStage.wav")),
            new AudioTrackDefinition(
                TrackId: "S1_TITLE_SCREEN",
                Game: GameId.Sonic1,
                DisplayName: "Sonic the Hedgehog Title Screen",
                ZoneId: "TitleScreen",
                Kind: AudioTrackKind.Menu,
                ReferenceFilePath: Path.Combine(
                    audioRoot,
                    "Misc",
                    "TitleScreen.wav"))
        ];
    }
}
