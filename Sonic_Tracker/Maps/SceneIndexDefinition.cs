using Sonic_Tracker.Games;

namespace Sonic_Tracker.Maps;

public sealed record SceneIndexDefinition(
    GameId Game,
    int SceneIndex,
    string ZoneId,
    string DisplayName,
    int? ActNumber,
    string? Variant = null);
