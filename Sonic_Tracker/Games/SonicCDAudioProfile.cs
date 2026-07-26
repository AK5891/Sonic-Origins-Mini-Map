using System.IO;
using Sonic_Tracker.Audio;

namespace Sonic_Tracker.Games;

public sealed class SonicCDAudioProfile : IGameAudioProfile
{
    public GameId Game => GameId.SonicCD;
    public string DisplayName => "Sonic CD";
    public IReadOnlyList<string> ProcessNames { get; } =
        ["SonicOrigins"];

    public IReadOnlyList<AudioTrackDefinition> Tracks { get; }

    public SonicCDAudioProfile()
    {
        string audioRoot = Path.Combine(
            AppContext.BaseDirectory,
            "Assets",
            "Audio",
            "SonicCD");

        Tracks =
        [
            Track(
                "SCD_SPECIAL_STAGE",
                "Sonic CD Special Stage",
                "SpecialStage",
                AudioTrackKind.Temporary,
                "SpecialStage.wav"),
            Track(
                "SCD_TITLE_SCREEN",
                "Sonic CD Title Screen",
                "TitleScreen",
                AudioTrackKind.Menu,
                "TitleScreen.wav")
        ];

        AudioTrackDefinition Track(
            string id,
            string name,
            string zoneId,
            AudioTrackKind kind,
            string file) =>
            new(
                id,
                GameId.SonicCD,
                name,
                zoneId,
                kind,
                Path.Combine(audioRoot, file));
    }
}
