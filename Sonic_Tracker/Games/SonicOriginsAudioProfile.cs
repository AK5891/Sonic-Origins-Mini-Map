using System.IO;
using Sonic_Tracker.Audio;

namespace Sonic_Tracker.Games;

public sealed class SonicOriginsAudioProfile : IGameAudioProfile
{
    public GameId Game => GameId.Unknown;
    public string DisplayName => "Sonic Origins";
    public IReadOnlyList<string> ProcessNames { get; } = ["SonicOrigins"];
    public IReadOnlyList<AudioTrackDefinition> Tracks { get; }

    public SonicOriginsAudioProfile()
    {
        Tracks =
        [
            .. KeepRequiredTracks(
                new Sonic1AudioProfile().Tracks),
            .. KeepRequiredTracks(
                new Sonic2AudioProfile().Tracks),
            .. new SonicCDAudioProfile().Tracks,
            .. new Sonic3KAudioProfile().Tracks,
            new AudioTrackDefinition(
                TrackId: "ORIGINS_MAIN_MENU",
                Game: GameId.SonicOriginsMenu,
                DisplayName: "Sonic Origins Main Menu",
                ZoneId: "OriginsMainMenu",
                Kind: AudioTrackKind.Menu,
                ReferenceFilePath: Path.Combine(
                    AppContext.BaseDirectory,
                    "Assets",
                    "Audio",
                    "SonicOrigins",
                    "MainMenu.wav"))
        ];
    }

    private static IEnumerable<AudioTrackDefinition>
        KeepRequiredTracks(
            IEnumerable<AudioTrackDefinition> tracks) =>
        tracks.Where(track =>
            string.Equals(
                track.ZoneId,
                "SpecialStage",
                StringComparison.OrdinalIgnoreCase) ||
            string.Equals(
                track.ZoneId,
                "TitleScreen",
                StringComparison.OrdinalIgnoreCase));
}
