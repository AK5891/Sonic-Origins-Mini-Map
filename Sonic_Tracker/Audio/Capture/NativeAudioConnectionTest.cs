using System.Diagnostics;
using System.IO;

namespace Sonic_Tracker.Audio.Capture;

public static class NativeAudioConnectionTest
{
    public static string Run()
    {
        int version = NativeAudioMethods.GetVersion();
        int additionResult = NativeAudioMethods.Add(20, 22);

        return
            $"Native DLL version: {version}\n" +
            $"Native addition test: 20 + 22 = {additionResult}";
    }

    public static string TestProcess(int processId)
    {
        if (processId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(processId));
        }

        IntPtr captureHandle =
            NativeAudioMethods.Create();

        if (captureHandle == IntPtr.Zero)
        {
            throw new InvalidOperationException(
                "The native capture object could not be created.");
        }

        try
        {
            int setResult = NativeAudioMethods.SetProcess(
                captureHandle,
                checked((uint)processId));

            if (setResult == 0)
            {
                throw new InvalidOperationException(
                    "The native DLL could not open the selected process.");
            }

            int returnedProcessId =
                NativeAudioMethods.GetProcessId(captureHandle);

            bool isRunning =
                NativeAudioMethods.IsProcessRunning(
                    captureHandle) != 0;

            return
                $"Native DLL version: " +
                $"{NativeAudioMethods.GetVersion()}\n" +
                $"Requested PID: {processId}\n" +
                $"Native stored PID: {returnedProcessId}\n" +
                $"Process running: {isRunning}";
        }
        finally
        {
            NativeAudioMethods.Destroy(captureHandle);
        }
    }

    public static Process? FindSonicOrigins()
    {
        string[] possibleNames =
        {
            "SonicOrigins",
            "Sonic Origins"
        };

        foreach (string name in possibleNames)
        {
            string normalizedName =
                Path.GetFileNameWithoutExtension(name);

            Process? process = Process
                .GetProcessesByName(normalizedName)
                .FirstOrDefault(candidate =>
                {
                    try
                    {
                        return !candidate.HasExited;
                    }
                    catch
                    {
                        return false;
                    }
                });

            if (process is not null)
            {
                return process;
            }
        }

        return null;
    }


    public static async Task<string> TestCaptureAsync(
    int processId,
    CancellationToken cancellationToken = default)
    {
        IntPtr captureHandle =
            NativeAudioMethods.Create();

        if (captureHandle == IntPtr.Zero)
        {
            throw new InvalidOperationException(
                "The native capture object could not be created.");
        }

        try
        {
            int setResult = NativeAudioMethods.SetProcess(
                captureHandle,
                checked((uint)processId));

            if (setResult == 0)
            {
                throw new InvalidOperationException(
                    "The native DLL could not select Sonic Origins.");
            }

            int startResult =
                NativeAudioMethods.StartCapture(captureHandle);

            if (startResult == 0)
            {
                int error =
                    NativeAudioMethods.GetLastError(captureHandle);

                throw new InvalidOperationException(
                    $"Capture could not start. HRESULT: 0x{error:X8}");
            }

            const int startupTimeoutMilliseconds = 5000;
            int waitedMilliseconds = 0;

            while (NativeAudioMethods.IsCapturing(
                       captureHandle) == 0)
            {
                int error =
                    NativeAudioMethods.GetLastError(captureHandle);

                if (error < 0)
                {
                    throw new InvalidOperationException(
                        $"WASAPI initialization failed. " +
                        $"HRESULT: 0x{error:X8}");
                }

                if (waitedMilliseconds >=
                    startupTimeoutMilliseconds)
                {
                    throw new TimeoutException(
                        "Audio capture did not start within five seconds.");
                }

                await Task.Delay(
                    100,
                    cancellationToken);

                waitedMilliseconds += 100;
            }

            await Task.Delay(
                TimeSpan.FromSeconds(5),
                cancellationToken);

            ulong capturedBytes =
                NativeAudioMethods.GetCapturedByteCount(
                    captureHandle);

            int sampleRate =
                NativeAudioMethods.GetSampleRate(
                    captureHandle);

            int channels =
                NativeAudioMethods.GetChannelCount(
                    captureHandle);

            int bitsPerSample =
                NativeAudioMethods.GetBitsPerSample(
                    captureHandle);

            int lastError =
                NativeAudioMethods.GetLastError(
                    captureHandle);

            return
                $"Native DLL version: " +
                $"{NativeAudioMethods.GetVersion()}\n" +
                $"Process ID: {processId}\n" +
                $"Sample rate: {sampleRate} Hz\n" +
                $"Channels: {channels}\n" +
                $"Bits per sample: {bitsPerSample}\n" +
                $"Captured bytes: {capturedBytes:N0}\n" +
                $"Last HRESULT: 0x{lastError:X8}";
        }
        finally
        {
            NativeAudioMethods.StopCapture(
                captureHandle);

            NativeAudioMethods.Destroy(
                captureHandle);
        }
    }


    public static async Task<string> CaptureWavAsync(
    int processId,
    TimeSpan duration,
    CancellationToken cancellationToken = default)
    {
        IntPtr captureHandle =
            NativeAudioMethods.Create();

        if (captureHandle == IntPtr.Zero)
        {
            throw new InvalidOperationException(
                "The native capture object could not be created.");
        }

        try
        {
            if (NativeAudioMethods.SetProcess(
                    captureHandle,
                    checked((uint)processId)) == 0)
            {
                throw new InvalidOperationException(
                    "Sonic Origins could not be selected.");
            }

            if (NativeAudioMethods.StartCapture(
                    captureHandle) == 0)
            {
                int error =
                    NativeAudioMethods.GetLastError(captureHandle);

                throw new InvalidOperationException(
                    $"Capture could not start. HRESULT: 0x{error:X8}");
            }

            await WaitForCaptureStartupAsync(
                captureHandle,
                cancellationToken);

            await Task.Delay(
                duration,
                cancellationToken);

            NativeAudioMethods.StopCapture(
                captureHandle);

            int sampleRate =
                NativeAudioMethods.GetSampleRate(captureHandle);

            short channels = checked((short)
                NativeAudioMethods.GetChannelCount(captureHandle));

            short bitsPerSample = checked((short)
                NativeAudioMethods.GetBitsPerSample(captureHandle));

            byte[] pcmData =
                ReadAllBufferedAudio(captureHandle);

            if (pcmData.Length == 0)
            {
                throw new InvalidOperationException(
                    "Capture completed, but no PCM audio was received.");
            }

            string directory = Path.Combine(
                Path.GetTempPath(),
                "SonicTracker",
                "LiveAudio");

            Directory.CreateDirectory(directory);

            string outputPath = Path.Combine(
                directory,
                $"sonic_capture_{DateTime.Now:yyyyMMdd_HHmmss}.wav");

            WritePcmWaveFile(
                outputPath,
                pcmData,
                sampleRate,
                channels,
                bitsPerSample);

            return outputPath;
        }
        finally
        {
            NativeAudioMethods.StopCapture(
                captureHandle);

            NativeAudioMethods.Destroy(
                captureHandle);
        }
    }

    private static async Task WaitForCaptureStartupAsync(
        IntPtr captureHandle,
        CancellationToken cancellationToken)
    {
        for (int attempt = 0; attempt < 50; attempt++)
        {
            if (NativeAudioMethods.IsCapturing(
                    captureHandle) != 0)
            {
                return;
            }

            int error =
                NativeAudioMethods.GetLastError(captureHandle);

            if (error < 0)
            {
                throw new InvalidOperationException(
                    $"WASAPI initialization failed. " +
                    $"HRESULT: 0x{error:X8}");
            }

            await Task.Delay(
                100,
                cancellationToken);
        }

        throw new TimeoutException(
            "Audio capture did not start within five seconds.");
    }

    private static byte[] ReadAllBufferedAudio(
        IntPtr captureHandle)
    {
        ulong bufferedByteCount =
            NativeAudioMethods.GetBufferedByteCount(
                captureHandle);

        if (bufferedByteCount == 0)
        {
            return Array.Empty<byte>();
        }

        if (bufferedByteCount > int.MaxValue)
        {
            throw new InvalidOperationException(
                "The native audio buffer is too large.");
        }

        byte[] result =
            new byte[(int)bufferedByteCount];

        int totalRead = 0;

        while (totalRead < result.Length)
        {
            int remaining =
                result.Length - totalRead;

            byte[] chunk =
                new byte[remaining];

            int bytesRead =
                NativeAudioMethods.ReadBytes(
                    captureHandle,
                    chunk,
                    chunk.Length);

            if (bytesRead <= 0)
            {
                break;
            }

            Buffer.BlockCopy(
                chunk,
                0,
                result,
                totalRead,
                bytesRead);

            totalRead += bytesRead;
        }

        if (totalRead == result.Length)
        {
            return result;
        }

        Array.Resize(
            ref result,
            totalRead);

        return result;
    }

    private static void WritePcmWaveFile(
        string filePath,
        byte[] pcmData,
        int sampleRate,
        short channels,
        short bitsPerSample)
    {
        short blockAlign = checked((short)
            (channels * bitsPerSample / 8));

        int bytesPerSecond = checked(
            sampleRate * blockAlign);

        using FileStream fileStream =
            new(filePath, FileMode.Create, FileAccess.Write);

        using BinaryWriter writer =
            new(fileStream);

        writer.Write(
            System.Text.Encoding.ASCII.GetBytes("RIFF"));

        writer.Write(
            36 + pcmData.Length);

        writer.Write(
            System.Text.Encoding.ASCII.GetBytes("WAVE"));

        writer.Write(
            System.Text.Encoding.ASCII.GetBytes("fmt "));

        writer.Write(16);
        writer.Write((short)1);
        writer.Write(channels);
        writer.Write(sampleRate);
        writer.Write(bytesPerSecond);
        writer.Write(blockAlign);
        writer.Write(bitsPerSample);

        writer.Write(
            System.Text.Encoding.ASCII.GetBytes("data"));

        writer.Write(pcmData.Length);
        writer.Write(pcmData);
    }
}