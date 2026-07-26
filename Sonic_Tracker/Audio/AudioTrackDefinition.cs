using Sonic_Tracker.Games;

namespace Sonic_Tracker.Audio;

public sealed record AudioTrackDefinition(
    string TrackId,
    GameId Game,
    string DisplayName,
    string ZoneId,
    AudioTrackKind Kind,
    string ReferenceFilePath);