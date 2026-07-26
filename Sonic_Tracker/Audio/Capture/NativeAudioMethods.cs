using System.Runtime.InteropServices;

namespace Sonic_Tracker.Audio.Capture;

internal static class NativeAudioMethods
{
    private const string DllName =
        "SonicTracker.NativeAudio.dll";

    [DllImport(
        DllName,
        EntryPoint = "NativeAudio_GetVersion",
        CallingConvention = CallingConvention.Cdecl)]
    internal static extern int GetVersion();

    [DllImport(
        DllName,
        EntryPoint = "NativeAudio_Add",
        CallingConvention = CallingConvention.Cdecl)]
    internal static extern int Add(
        int first,
        int second);

    [DllImport(
        DllName,
        EntryPoint = "NativeAudio_Create",
        CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr Create();

    [DllImport(
        DllName,
        EntryPoint = "NativeAudio_SetProcess",
        CallingConvention = CallingConvention.Cdecl)]
    internal static extern int SetProcess(
        IntPtr captureHandle,
        uint processId);

    [DllImport(
        DllName,
        EntryPoint = "NativeAudio_GetProcessId",
        CallingConvention = CallingConvention.Cdecl)]
    internal static extern int GetProcessId(
        IntPtr captureHandle);

    [DllImport(
        DllName,
        EntryPoint = "NativeAudio_IsProcessRunning",
        CallingConvention = CallingConvention.Cdecl)]
    internal static extern int IsProcessRunning(
        IntPtr captureHandle);

    [DllImport(
        DllName,
        EntryPoint = "NativeAudio_StartCapture",
        CallingConvention = CallingConvention.Cdecl)]
    internal static extern int StartCapture(
        IntPtr captureHandle);

    [DllImport(
        DllName,
        EntryPoint = "NativeAudio_StopCapture",
        CallingConvention = CallingConvention.Cdecl)]
    internal static extern void StopCapture(
        IntPtr captureHandle);

    [DllImport(
        DllName,
        EntryPoint = "NativeAudio_IsCapturing",
        CallingConvention = CallingConvention.Cdecl)]
    internal static extern int IsCapturing(
        IntPtr captureHandle);

    [DllImport(
        DllName,
        EntryPoint = "NativeAudio_GetCapturedByteCount",
        CallingConvention = CallingConvention.Cdecl)]
    internal static extern ulong GetCapturedByteCount(
        IntPtr captureHandle);

    [DllImport(
        DllName,
        EntryPoint = "NativeAudio_GetBufferedByteCount",
        CallingConvention = CallingConvention.Cdecl)]
    internal static extern ulong GetBufferedByteCount(
        IntPtr captureHandle);

    [DllImport(
        DllName,
        EntryPoint = "NativeAudio_ReadBytes",
        CallingConvention = CallingConvention.Cdecl)]
    internal static extern int ReadBytes(
        IntPtr captureHandle,
        [Out] byte[] destination,
        int destinationCapacity);

    [DllImport(
        DllName,
        EntryPoint = "NativeAudio_ClearBuffer",
        CallingConvention = CallingConvention.Cdecl)]
    internal static extern void ClearBuffer(
        IntPtr captureHandle);

    [DllImport(
        DllName,
        EntryPoint = "NativeAudio_GetSampleRate",
        CallingConvention = CallingConvention.Cdecl)]
    internal static extern int GetSampleRate(
        IntPtr captureHandle);

    [DllImport(
        DllName,
        EntryPoint = "NativeAudio_GetChannelCount",
        CallingConvention = CallingConvention.Cdecl)]
    internal static extern int GetChannelCount(
        IntPtr captureHandle);

    [DllImport(
        DllName,
        EntryPoint = "NativeAudio_GetBitsPerSample",
        CallingConvention = CallingConvention.Cdecl)]
    internal static extern int GetBitsPerSample(
        IntPtr captureHandle);

    [DllImport(
        DllName,
        EntryPoint = "NativeAudio_GetLastError",
        CallingConvention = CallingConvention.Cdecl)]
    internal static extern int GetLastError(
        IntPtr captureHandle);

    [DllImport(
        DllName,
        EntryPoint = "NativeAudio_Destroy",
        CallingConvention = CallingConvention.Cdecl)]
    internal static extern void Destroy(
        IntPtr captureHandle);
}