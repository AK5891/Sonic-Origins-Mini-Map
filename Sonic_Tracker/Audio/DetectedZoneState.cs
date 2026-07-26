using Sonic_Tracker.Games;

namespace Sonic_Tracker.Audio;

public sealed class DetectedZoneState
{
    public GameId CurrentGame { get; private set; } =
        GameId.Unknown;

    public string CurrentZoneId { get; private set; } =
        string.Empty;

    public string CurrentZoneName { get; private set; } =
        string.Empty;

    public double LastConfidence { get; private set; }

    public bool HasZone =>
        CurrentGame != GameId.Unknown &&
        !string.IsNullOrWhiteSpace(CurrentZoneId);

    public bool Apply(AudioRecognitionResult? result)
    {
        if (result is null)
        {
            return false;
        }

        if (!result.IsZoneTrack)
        {
            return false;
        }

        bool changed =
            CurrentGame != result.Track.Game ||
            !string.Equals(
                CurrentZoneId,
                result.Track.ZoneId,
                StringComparison.OrdinalIgnoreCase);

        CurrentGame = result.Track.Game;
        CurrentZoneId = result.Track.ZoneId;
        CurrentZoneName = result.Track.DisplayName;
        LastConfidence = result.Confidence;

        return changed;
    }

    public void Reset()
    {
        CurrentGame = GameId.Unknown;
        CurrentZoneId = string.Empty;
        CurrentZoneName = string.Empty;
        LastConfidence = 0;
    }
}