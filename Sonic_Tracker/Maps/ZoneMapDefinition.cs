using Sonic_Tracker.Games;

namespace Sonic_Tracker.Maps;

public sealed class ZoneMapDefinition
{
    public GameId Game { get; }

    public string ZoneId { get; }

    public string ZoneName { get; }

    public int? ActNumber { get; }

    public string GameName { get; }

    public string MapResource { get; }

    public string? Variant { get; }

    public double MapOffsetX { get; }

    public double MapOffsetY { get; }

    public IReadOnlyList<string> TrackNameKeywords { get; }

    public ZoneMapDefinition(
        GameId game,
        string zoneId,
        string zoneName,
        int? actNumber,
        string gameName,
        string mapResource,
        string? variant,
        double mapOffsetX,
        double mapOffsetY,
        params string[] trackNameKeywords)
    {
        Game = game;
        ZoneId = zoneId;
        ZoneName = zoneName;
        ActNumber = actNumber;
        GameName = gameName;
        MapResource = mapResource;
        Variant = variant;
        MapOffsetX = mapOffsetX;
        MapOffsetY = mapOffsetY;
        TrackNameKeywords = trackNameKeywords;
    }

    public string DisplayName =>
        string.Join(
            " - ",
            new[]
            {
                ZoneName,
                ActNumber.HasValue
                    ? $"Act {ActNumber.Value}"
                    : null,
                Variant
            }.Where(part =>
                !string.IsNullOrWhiteSpace(part)));

    public override string ToString() =>
        $"{GameName}: {DisplayName}";

    public bool MatchesTrackName(
        string trackName)
    {
        return TrackNameKeywords.Any(keyword =>
            trackName.Contains(
                keyword,
                StringComparison.OrdinalIgnoreCase));
    }
}
