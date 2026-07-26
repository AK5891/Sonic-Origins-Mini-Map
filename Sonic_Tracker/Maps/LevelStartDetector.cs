using Sonic_Tracker.Memory;

namespace Sonic_Tracker.Maps;

public sealed class LevelStartDetector
{
    private const double JumpDistance = 256;
    private static readonly TimeSpan DetectionWindow = TimeSpan.FromSeconds(15);

    private SonicPosition? _previousPosition;
    private LevelStartDefinition? _candidate;
    private int _candidateReadings;
    private DateTime _detectionDeadlineUtc = DateTime.UtcNow + DetectionWindow;

    public void Arm()
    {
        _detectionDeadlineUtc = DateTime.UtcNow + DetectionWindow;
        _candidate = null;
        _candidateReadings = 0;
    }

    public LevelStartDefinition? Observe(
        SonicPosition position,
        string? recognizedZoneId)
    {
        if (_previousPosition is SonicPosition previous)
        {
            double deltaX = position.X - previous.X;
            double deltaY = position.Y - previous.Y;
            double distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

            if (distance >= JumpDistance)
            {
                Arm();
            }
        }

        _previousPosition = position;

        if (string.IsNullOrWhiteSpace(recognizedZoneId) ||
            DateTime.UtcNow > _detectionDeadlineUtc)
        {
            ResetCandidate();
            return null;
        }

        LevelStartDefinition? match =
            LevelStartRegistry.FindClosest(
                recognizedZoneId,
                position.X,
                position.Y);

        if (match is null)
        {
            ResetCandidate();
            return null;
        }

        if (_candidate == match)
        {
            _candidateReadings++;
        }
        else
        {
            _candidate = match;
            _candidateReadings = 1;
        }

        if (_candidateReadings < 2)
        {
            return null;
        }

        _detectionDeadlineUtc = DateTime.MinValue;
        ResetCandidate();
        return match;
    }

    private void ResetCandidate()
    {
        _candidate = null;
        _candidateReadings = 0;
    }
}
