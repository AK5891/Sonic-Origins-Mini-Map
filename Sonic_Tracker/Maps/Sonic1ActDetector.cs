using Sonic_Tracker.Memory;

namespace Sonic_Tracker.Maps;

public sealed class Sonic1ActDetector
{
    private const double JumpDistance = 256;
    private const double EndProgressRatio = 0.95;
    private static readonly TimeSpan DetectionWindow = TimeSpan.FromSeconds(15);

    private string? _zoneId;
    private SonicPosition? _previousPosition;
    private bool _reachedEnd;
    private DateTime _deadlineUtc;
    private int? _candidateAct;
    private int _candidateReadings;

    public void SetZone(string zoneId)
    {
        if (string.Equals(_zoneId, zoneId, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        _zoneId = zoneId;
        _previousPosition = null;
        _reachedEnd = false;
        Arm();
    }

    public int? Observe(
        SonicPosition position,
        string? zoneId,
        int currentAct,
        double currentMapWidth)
    {
        if (string.IsNullOrWhiteSpace(zoneId))
        {
            return null;
        }

        SetZone(zoneId);

        bool jumped = false;

        if (_previousPosition is SonicPosition previous)
        {
            double deltaX = position.X - previous.X;
            double deltaY = position.Y - previous.Y;
            jumped = Math.Sqrt(deltaX * deltaX + deltaY * deltaY) >= JumpDistance;

            if (string.Equals(zoneId, "ScrapBrain", StringComparison.OrdinalIgnoreCase) &&
                currentAct == 2 &&
                Math.Abs(deltaX) >= JumpDistance &&
                Math.Abs(position.X - 2944) <= 128)
            {
                _previousPosition = position;
                _reachedEnd = false;
                return 3;
            }
        }

        _previousPosition = position;

        if (currentMapWidth > 0 &&
            position.X >= currentMapWidth * EndProgressRatio)
        {
            _reachedEnd = true;
        }

        if (jumped)
        {
            Arm();
        }

        if (DateTime.UtcNow > _deadlineUtc)
        {
            ResetCandidate();
            return null;
        }

        IReadOnlyList<LevelStartDefinition> matches =
            Sonic1LevelStartRegistry.FindMatches(zoneId, position.X, position.Y);

        if (matches.Count == 0)
        {
            ResetCandidate();
            return null;
        }

        int? detectedAct = null;

        if (matches.Count == 1)
        {
            detectedAct = matches[0].ActNumber;
        }
        else if (_reachedEnd && currentAct < 3)
        {
            detectedAct = currentAct + 1;
        }

        if (!detectedAct.HasValue || detectedAct == currentAct)
        {
            return null;
        }

        if (_candidateAct == detectedAct)
        {
            _candidateReadings++;
        }
        else
        {
            _candidateAct = detectedAct;
            _candidateReadings = 1;
        }

        if (_candidateReadings < 2)
        {
            return null;
        }

        _reachedEnd = false;
        _deadlineUtc = DateTime.MinValue;
        ResetCandidate();
        return detectedAct;
    }

    private void Arm()
    {
        _deadlineUtc = DateTime.UtcNow + DetectionWindow;
        ResetCandidate();
    }

    private void ResetCandidate()
    {
        _candidateAct = null;
        _candidateReadings = 0;
    }
}
