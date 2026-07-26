#pragma once

#include <Windows.h>

#ifdef SONICTRACKERNATIVEAUDIO_EXPORTS
#define NATIVE_AUDIO_API __declspec(dllexport)
#else
#define NATIVE_AUDIO_API __declspec(dllimport)
#endif

extern "C"
{
    NATIVE_AUDIO_API int __cdecl NativeAudio_GetVersion();

    NATIVE_AUDIO_API int __cdecl NativeAudio_Add(
        int first,
        int second);

    NATIVE_AUDIO_API void* __cdecl NativeAudio_Create();

    NATIVE_AUDIO_API int __cdecl NativeAudio_SetProcess(
        void* captureHandle,
        unsigned long processId);

    NATIVE_AUDIO_API int __cdecl NativeAudio_GetProcessId(
        void* captureHandle);

    NATIVE_AUDIO_API int __cdecl NativeAudio_IsProcessRunning(
        void* captureHandle);

    NATIVE_AUDIO_API int __cdecl NativeAudio_StartCapture(
        void* captureHandle);

    NATIVE_AUDIO_API void __cdecl NativeAudio_StopCapture(
        void* captureHandle);

    NATIVE_AUDIO_API int __cdecl NativeAudio_IsCapturing(
        void* captureHandle);

    NATIVE_AUDIO_API unsigned long long __cdecl
        NativeAudio_GetCapturedByteCount(
            void* captureHandle);

    NATIVE_AUDIO_API int __cdecl NativeAudio_GetSampleRate(
        void* captureHandle);

    NATIVE_AUDIO_API int __cdecl NativeAudio_GetChannelCount(
        void* captureHandle);

    NATIVE_AUDIO_API int __cdecl NativeAudio_GetBitsPerSample(
        void* captureHandle);

    NATIVE_AUDIO_API long __cdecl NativeAudio_GetLastError(
        void* captureHandle);

    NATIVE_AUDIO_API unsigned long long __cdecl
        NativeAudio_GetBufferedByteCount(
            void* captureHandle);

    NATIVE_AUDIO_API int __cdecl NativeAudio_ReadBytes(
        void* captureHandle,
        unsigned char* destination,
        int destinationCapacity);

    NATIVE_AUDIO_API void __cdecl NativeAudio_ClearBuffer(
        void* captureHandle);

    NATIVE_AUDIO_API void __cdecl NativeAudio_Destroy(
        void* captureHandle);
}