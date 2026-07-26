namespace Sonic_Tracker.Audio;

public sealed record AudioRecognitionResult(
    AudioTrackDefinition Track,
    double Confidence)
{
    public bool IsZoneTrack =>
        Track.Kind == AudioTrackKind.Zone;

    public bool IsTemporaryTrack =>
        Track.Kind == AudioTrackKind.Temporary;
}