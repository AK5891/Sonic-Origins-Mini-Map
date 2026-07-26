using Sonic_Tracker.Games;
using SoundFingerprinting;
using SoundFingerprinting.Audio;
using SoundFingerprinting.Builder;
using SoundFingerprinting.Data;
using SoundFingerprinting.InMemory;
using System.IO;

namespace Sonic_Tracker.Audio;

public sealed class SoundFingerprintRecognitionService
    : IAudioRecognitionService
{
    private readonly IModelService _modelService;
    private readonly IAudioService _audioService;

    private readonly Dictionary<string, AudioTrackDefinition>
        _tracksById = new(StringComparer.OrdinalIgnoreCase);

    private readonly double _minimumConfidence;

    public bool IsInitialized { get; private set; }

    public SoundFingerprintRecognitionService(
        double minimumConfidence = 0.75)
    {
        if (minimumConfidence is < 0 or > 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(minimumConfidence),
                "Confidence must be between 0 and 1.");
        }

        _minimumConfidence = minimumConfidence;
        _modelService = new InMemoryModelService();
        _audioService = new SoundFingerprintingAudioService();
    }

    public async Task InitializeAsync(
        IGameAudioProfile profile,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(profile);

        if (IsInitialized)
        {
            throw new InvalidOperationException(
                "The recognition service has already been initialized.");
        }

        foreach (AudioTrackDefinition definition in profile.Tracks)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!File.Exists(definition.ReferenceFilePath))
            {
                continue;
            }

            TrackInfo trackInfo = new(
                definition.TrackId,
                definition.DisplayName,
                profile.DisplayName);

            var fingerprints = await FingerprintCommandBuilder.Instance
                .BuildFingerprintCommand()
                .From(definition.ReferenceFilePath)
                .UsingServices(_audioService)
                .Hash();

            _modelService.Insert(trackInfo, fingerprints);
            _tracksById[definition.TrackId] = definition;
        }

        if (_tracksById.Count == 0)
        {
            throw new InvalidOperationException(
                "No audio reference files were found. Check the " +
                "Assets/Audio folder and file Build Actions.");
        }

        IsInitialized = true;
    }

    public async Task<AudioRecognitionResult?> RecognizeFileAsync(
        string audioFilePath,
        CancellationToken cancellationToken = default)
    {
        if (!IsInitialized)
        {
            throw new InvalidOperationException(
                "InitializeAsync must be called before recognition.");
        }

        if (string.IsNullOrWhiteSpace(audioFilePath))
        {
            throw new ArgumentException(
                "An audio file path is required.",
                nameof(audioFilePath));
        }

        if (!File.Exists(audioFilePath))
        {
            throw new FileNotFoundException(
                "The query audio file was not found.",
                audioFilePath);
        }

        cancellationToken.ThrowIfCancellationRequested();

        var queryResult = await QueryCommandBuilder.Instance
            .BuildQueryCommand()
            .From(audioFilePath)
            .UsingServices(_modelService, _audioService)
            .Query();

        var match = queryResult.BestMatch?.Audio;

        if (match is null)
        {
            return null;
        }

        if (match.Confidence < _minimumConfidence)
        {
            return null;
        }

        if (!_tracksById.TryGetValue(
                match.Track.Id,
                out AudioTrackDefinition? definition))
        {
            return null;
        }

        return new AudioRecognitionResult(
            definition,
            match.Confidence);
    }
}