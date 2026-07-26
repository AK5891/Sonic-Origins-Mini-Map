using Sonic_Tracker.Audio;

namespace Sonic_Tracker.Games;

public interface IGameAudioProfile
{
    GameId Game { get; }

    string DisplayName { get; }

    IReadOnlyList<string> ProcessNames { get; }

    IReadOnlyList<AudioTrackDefinition> Tracks { get; }
}