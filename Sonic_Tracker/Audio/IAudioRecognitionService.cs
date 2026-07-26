using Sonic_Tracker.Games;

namespace Sonic_Tracker.Audio;

public interface IAudioRecognitionService
{
    bool IsInitialized { get; }

    Task InitializeAsync(
        IGameAudioProfile profile,
        CancellationToken cancellationToken = default);

    Task<AudioRecognitionResult?> RecognizeFileAsync(
        string audioFilePath,
        CancellationToken cancellationToken = default);
}