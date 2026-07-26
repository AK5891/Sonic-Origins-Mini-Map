#include "pch.h"
#include "NativeAudioApi.h"

#include <Windows.h>
#undef min
#undef max
#include <audioclient.h>
#include <audioclientactivationparams.h>
#include <mmdeviceapi.h>
#include <propidl.h>
#include <wrl.h>

#include <atomic>
#include <cstdint>
#include <mutex>
#include <thread>

#include <algorithm>
#include <cstring>
#include <vector>

#pragma comment(lib, "Ole32.lib")
#pragma comment(lib, "Mmdevapi.lib")
#pragma comment(lib, "Avrt.lib")

using Microsoft::WRL::ComPtr;
using Microsoft::WRL::ClassicCom;
using Microsoft::WRL::FtmBase;
using Microsoft::WRL::RuntimeClass;
using Microsoft::WRL::RuntimeClassFlags;

namespace
{
    constexpr int CaptureSampleRate = 44100;
    constexpr int CaptureChannels = 2;
    constexpr int CaptureBitsPerSample = 16;

    class AudioActivationHandler final :
        public RuntimeClass<
        RuntimeClassFlags<ClassicCom>,
        FtmBase,
        IActivateAudioInterfaceCompletionHandler>
    {
    public:
        explicit AudioActivationHandler(HANDLE completedEvent)
            : _completedEvent(completedEvent)
        {
        }

        HRESULT STDMETHODCALLTYPE ActivateCompleted(
            IActivateAudioInterfaceAsyncOperation* operation) override
        {
            HRESULT activationResult = E_FAIL;
            ComPtr<IUnknown> activatedInterface;

            HRESULT result = operation->GetActivateResult(
                &activationResult,
                &activatedInterface);

            if (SUCCEEDED(result))
            {
                result = activationResult;
            }

            if (SUCCEEDED(result))
            {
                result = activatedInterface.As(&_audioClient);
            }

            _result = result;

            if (_completedEvent != nullptr)
            {
                SetEvent(_completedEvent);
            }

            return S_OK;
        }

        HRESULT Result() const
        {
            return _result;
        }

        ComPtr<IAudioClient> AudioClient() const
        {
            return _audioClient;
        }

    private:
        HANDLE _completedEvent = nullptr;
        HRESULT _result = E_PENDING;
        ComPtr<IAudioClient> _audioClient;
    };

    struct NativeCaptureState
    {
        DWORD processId = 0;
        HANDLE processHandle = nullptr;

        HANDLE stopEvent = nullptr;
        HANDLE sampleReadyEvent = nullptr;
        HANDLE activationCompletedEvent = nullptr;

        std::thread captureThread;

        std::atomic<bool> capturing = false;
        std::atomic<bool> starting = false;
        std::atomic<unsigned long long> capturedBytes = 0;
        std::atomic<long> lastError = S_OK;

        std::mutex audioBufferMutex;
        std::vector<unsigned char> audioBuffer;


        int sampleRate = CaptureSampleRate;
        int channels = CaptureChannels;
        int bitsPerSample = CaptureBitsPerSample;

        NativeCaptureState()
        {
            stopEvent = CreateEvent(
                nullptr,
                TRUE,
                FALSE,
                nullptr);

            sampleReadyEvent = CreateEvent(
                nullptr,
                FALSE,
                FALSE,
                nullptr);

            activationCompletedEvent = CreateEvent(
                nullptr,
                FALSE,
                FALSE,
                nullptr);
        }

        ~NativeCaptureState()
        {
            StopCapture();
            CloseCurrentProcess();

            if (activationCompletedEvent != nullptr)
            {
                CloseHandle(activationCompletedEvent);
                activationCompletedEvent = nullptr;
            }

            if (sampleReadyEvent != nullptr)
            {
                CloseHandle(sampleReadyEvent);
                sampleReadyEvent = nullptr;
            }

            if (stopEvent != nullptr)
            {
                CloseHandle(stopEvent);
                stopEvent = nullptr;
            }
        }

        void CloseCurrentProcess()
        {
            if (processHandle != nullptr)
            {
                CloseHandle(processHandle);
                processHandle = nullptr;
            }

            processId = 0;
        }

        void StopCapture()
        {
            if (stopEvent != nullptr)
            {
                SetEvent(stopEvent);
            }

            if (captureThread.joinable())
            {
                captureThread.join();
            }

            capturing = false;
            starting = false;
        }

        bool EventsCreatedSuccessfully() const
        {
            return stopEvent != nullptr &&
                sampleReadyEvent != nullptr &&
                activationCompletedEvent != nullptr;
        }
    };

    void DrainCapturedAudio(
        IAudioCaptureClient* captureClient,
        NativeCaptureState* state)
    {
        while (true)
        {
            UINT32 packetFrames = 0;

            HRESULT result =
                captureClient->GetNextPacketSize(&packetFrames);

            if (FAILED(result))
            {
                state->lastError = result;
                return;
            }

            if (packetFrames == 0)
            {
                return;
            }

            BYTE* audioData = nullptr;
            UINT32 frameCount = 0;
            DWORD flags = 0;

            result = captureClient->GetBuffer(
                &audioData,
                &frameCount,
                &flags,
                nullptr,
                nullptr);

            if (FAILED(result))
            {
                state->lastError = result;
                return;
            }

            const unsigned int bytesPerFrame =
                state->channels *
                state->bitsPerSample /
                8;

            const unsigned long long packetBytes =
                static_cast<unsigned long long>(frameCount) *
                bytesPerFrame;

            state->capturedBytes.fetch_add(packetBytes);

            {
                std::lock_guard<std::mutex> lock(
                    state->audioBufferMutex);

                const size_t oldSize =
                    state->audioBuffer.size();

                const size_t additionalSize =
                    static_cast<size_t>(packetBytes);

                state->audioBuffer.resize(
                    oldSize + additionalSize);

                unsigned char* destination =
                    state->audioBuffer.data() + oldSize;

                if ((flags & AUDCLNT_BUFFERFLAGS_SILENT) != 0 ||
                    audioData == nullptr)
                {
                    std::memset(
                        destination,
                        0,
                        additionalSize);
                }
                else
                {
                    std::memcpy(
                        destination,
                        audioData,
                        additionalSize);
                }
            }

            result = captureClient->ReleaseBuffer(frameCount);

            state->capturedBytes.fetch_add(packetBytes);

            result = captureClient->ReleaseBuffer(frameCount);

            if (FAILED(result))
            {
                state->lastError = result;
                return;
            }
        }
    }

    HRESULT ActivateProcessAudioClient(
        NativeCaptureState* state,
        ComPtr<IAudioClient>& audioClient)
    {
        ResetEvent(state->activationCompletedEvent);

        AUDIOCLIENT_ACTIVATION_PARAMS activationParameters = {};

        activationParameters.ActivationType =
            AUDIOCLIENT_ACTIVATION_TYPE_PROCESS_LOOPBACK;

        activationParameters
            .ProcessLoopbackParams
            .TargetProcessId = state->processId;

        activationParameters
            .ProcessLoopbackParams
            .ProcessLoopbackMode =
            PROCESS_LOOPBACK_MODE_INCLUDE_TARGET_PROCESS_TREE;

        PROPVARIANT activationVariant;
        PropVariantInit(&activationVariant);

        activationVariant.vt = VT_BLOB;
        activationVariant.blob.cbSize =
            sizeof(activationParameters);

        activationVariant.blob.pBlobData =
            reinterpret_cast<BYTE*>(
                &activationParameters);

        ComPtr<AudioActivationHandler> handler =
            Microsoft::WRL::Make<AudioActivationHandler>(
                state->activationCompletedEvent);

        if (handler == nullptr)
        {
            return E_OUTOFMEMORY;
        }

        ComPtr<IActivateAudioInterfaceAsyncOperation> operation;

        HRESULT result = ActivateAudioInterfaceAsync(
            VIRTUAL_AUDIO_DEVICE_PROCESS_LOOPBACK,
            __uuidof(IAudioClient),
            &activationVariant,
            handler.Get(),
            &operation);

        if (FAILED(result))
        {
            return result;
        }

        HANDLE waitHandles[] =
        {
            state->activationCompletedEvent,
            state->stopEvent
        };

        DWORD waitResult = WaitForMultipleObjects(
            2,
            waitHandles,
            FALSE,
            10000);

        if (waitResult == WAIT_OBJECT_0 + 1)
        {
            return HRESULT_FROM_WIN32(ERROR_CANCELLED);
        }

        if (waitResult != WAIT_OBJECT_0)
        {
            return HRESULT_FROM_WIN32(ERROR_TIMEOUT);
        }

        result = handler->Result();

        if (FAILED(result))
        {
            return result;
        }

        audioClient = handler->AudioClient();

        return audioClient != nullptr
            ? S_OK
            : E_POINTER;
    }

    void CaptureThreadMain(NativeCaptureState* state)
    {
        HRESULT result = CoInitializeEx(
            nullptr,
            COINIT_MULTITHREADED);

        const bool comInitialized =
            SUCCEEDED(result);

        if (result == RPC_E_CHANGED_MODE)
        {
            result = S_OK;
        }

        if (FAILED(result))
        {
            state->lastError = result;
            state->starting = false;
            return;
        }

        ComPtr<IAudioClient> audioClient;

        result = ActivateProcessAudioClient(
            state,
            audioClient);

        if (FAILED(result))
        {
            state->lastError = result;
            state->starting = false;

            if (comInitialized)
            {
                CoUninitialize();
            }

            return;
        }

        WAVEFORMATEX captureFormat = {};

        captureFormat.wFormatTag = WAVE_FORMAT_PCM;
        captureFormat.nChannels =
            static_cast<WORD>(state->channels);

        captureFormat.nSamplesPerSec =
            state->sampleRate;

        captureFormat.wBitsPerSample =
            static_cast<WORD>(state->bitsPerSample);

        captureFormat.nBlockAlign =
            captureFormat.nChannels *
            captureFormat.wBitsPerSample /
            8;

        captureFormat.nAvgBytesPerSec =
            captureFormat.nSamplesPerSec *
            captureFormat.nBlockAlign;

        DWORD streamFlags =
            AUDCLNT_STREAMFLAGS_LOOPBACK |
            AUDCLNT_STREAMFLAGS_EVENTCALLBACK |
            AUDCLNT_STREAMFLAGS_AUTOCONVERTPCM |
            AUDCLNT_STREAMFLAGS_SRC_DEFAULT_QUALITY;

        result = audioClient->Initialize(
            AUDCLNT_SHAREMODE_SHARED,
            streamFlags,
            0,
            0,
            &captureFormat,
            nullptr);

        if (FAILED(result))
        {
            state->lastError = result;
            state->starting = false;

            if (comInitialized)
            {
                CoUninitialize();
            }

            return;
        }

        result = audioClient->SetEventHandle(
            state->sampleReadyEvent);

        if (FAILED(result))
        {
            state->lastError = result;
            state->starting = false;

            if (comInitialized)
            {
                CoUninitialize();
            }

            return;
        }

        ComPtr<IAudioCaptureClient> captureClient;

        result = audioClient->GetService(
            IID_PPV_ARGS(&captureClient));

        if (FAILED(result))
        {
            state->lastError = result;
            state->starting = false;

            if (comInitialized)
            {
                CoUninitialize();
            }

            return;
        }

        result = audioClient->Start();

        if (FAILED(result))
        {
            state->lastError = result;
            state->starting = false;

            if (comInitialized)
            {
                CoUninitialize();
            }

            return;
        }

        state->lastError = S_OK;
        state->capturing = true;
        state->starting = false;

        HANDLE waitHandles[] =
        {
            state->stopEvent,
            state->sampleReadyEvent
        };

        while (true)
        {
            DWORD waitResult = WaitForMultipleObjects(
                2,
                waitHandles,
                FALSE,
                INFINITE);

            if (waitResult == WAIT_OBJECT_0)
            {
                break;
            }

            if (waitResult == WAIT_OBJECT_0 + 1)
            {
                DrainCapturedAudio(
                    captureClient.Get(),
                    state);

                continue;
            }

            state->lastError =
                HRESULT_FROM_WIN32(GetLastError());

            break;
        }

        audioClient->Stop();

        state->capturing = false;
        state->starting = false;

        if (comInitialized)
        {
            CoUninitialize();
        }
    }
}

int __cdecl NativeAudio_GetVersion()
{
    return 4;
}

int __cdecl NativeAudio_Add(
    int first,
    int second)
{
    return first + second;
}

void* __cdecl NativeAudio_Create()
{
    try
    {
        auto* state = new NativeCaptureState();

        if (!state->EventsCreatedSuccessfully())
        {
            delete state;
            return nullptr;
        }

        return state;
    }
    catch (...)
    {
        return nullptr;
    }
}

int __cdecl NativeAudio_SetProcess(
    void* captureHandle,
    unsigned long processId)
{
    if (captureHandle == nullptr || processId == 0)
    {
        return 0;
    }

    auto* state =
        static_cast<NativeCaptureState*>(captureHandle);

    state->StopCapture();
    state->CloseCurrentProcess();

    HANDLE processHandle = OpenProcess(
        PROCESS_QUERY_LIMITED_INFORMATION |
        SYNCHRONIZE,
        FALSE,
        processId);

    if (processHandle == nullptr)
    {
        state->lastError =
            HRESULT_FROM_WIN32(GetLastError());

        return 0;
    }

    state->processId = processId;
    state->processHandle = processHandle;
    state->lastError = S_OK;

    return 1;
}

int __cdecl NativeAudio_GetProcessId(
    void* captureHandle)
{
    if (captureHandle == nullptr)
    {
        return 0;
    }

    auto* state =
        static_cast<NativeCaptureState*>(captureHandle);

    return static_cast<int>(state->processId);
}

int __cdecl NativeAudio_IsProcessRunning(
    void* captureHandle)
{
    if (captureHandle == nullptr)
    {
        return 0;
    }

    auto* state =
        static_cast<NativeCaptureState*>(captureHandle);

    if (state->processHandle == nullptr)
    {
        return 0;
    }

    DWORD waitResult = WaitForSingleObject(
        state->processHandle,
        0);

    return waitResult == WAIT_TIMEOUT ? 1 : 0;
}

int __cdecl NativeAudio_StartCapture(
    void* captureHandle)
{
    if (captureHandle == nullptr)
    {
        return 0;
    }

    auto* state =
        static_cast<NativeCaptureState*>(captureHandle);

    if (state->processId == 0 ||
        state->processHandle == nullptr)
    {
        return 0;
    }

    if (state->capturing || state->starting)
    {
        return 1;
    }

    if (state->captureThread.joinable())
    {
        state->captureThread.join();
    }

    ResetEvent(state->stopEvent);
    ResetEvent(state->sampleReadyEvent);

    state->capturedBytes = 0;
    state->lastError = S_OK;
    {
        std::lock_guard<std::mutex> lock(
            state->audioBufferMutex);

        state->audioBuffer.clear();
    }
    state->starting = true;

    try
    {
        state->captureThread =
            std::thread(CaptureThreadMain, state);

        return 1;
    }
    catch (...)
    {
        state->starting = false;
        state->lastError = E_FAIL;

        return 0;
    }
}

void __cdecl NativeAudio_StopCapture(
    void* captureHandle)
{
    if (captureHandle == nullptr)
    {
        return;
    }

    auto* state =
        static_cast<NativeCaptureState*>(captureHandle);

    state->StopCapture();
}

int __cdecl NativeAudio_IsCapturing(
    void* captureHandle)
{
    if (captureHandle == nullptr)
    {
        return 0;
    }

    auto* state =
        static_cast<NativeCaptureState*>(captureHandle);

    return state->capturing ? 1 : 0;
}

unsigned long long __cdecl
NativeAudio_GetCapturedByteCount(
    void* captureHandle)
{
    if (captureHandle == nullptr)
    {
        return 0;
    }

    auto* state =
        static_cast<NativeCaptureState*>(captureHandle);

    return state->capturedBytes.load();
}

int __cdecl NativeAudio_GetSampleRate(
    void* captureHandle)
{
    if (captureHandle == nullptr)
    {
        return 0;
    }

    auto* state =
        static_cast<NativeCaptureState*>(captureHandle);

    return state->sampleRate;
}

int __cdecl NativeAudio_GetChannelCount(
    void* captureHandle)
{
    if (captureHandle == nullptr)
    {
        return 0;
    }

    auto* state =
        static_cast<NativeCaptureState*>(captureHandle);

    return state->channels;
}

int __cdecl NativeAudio_GetBitsPerSample(
    void* captureHandle)
{
    if (captureHandle == nullptr)
    {
        return 0;
    }

    auto* state =
        static_cast<NativeCaptureState*>(captureHandle);

    return state->bitsPerSample;
}

long __cdecl NativeAudio_GetLastError(
    void* captureHandle)
{
    if (captureHandle == nullptr)
    {
        return E_POINTER;
    }

    auto* state =
        static_cast<NativeCaptureState*>(captureHandle);

    return state->lastError.load();
}


unsigned long long __cdecl
NativeAudio_GetBufferedByteCount(
    void* captureHandle)
{
    if (captureHandle == nullptr)
    {
        return 0;
    }

    auto* state =
        static_cast<NativeCaptureState*>(captureHandle);

    std::lock_guard<std::mutex> lock(
        state->audioBufferMutex);

    return static_cast<unsigned long long>(
        state->audioBuffer.size());
}

int __cdecl NativeAudio_ReadBytes(
    void* captureHandle,
    unsigned char* destination,
    int destinationCapacity)
{
    if (captureHandle == nullptr ||
        destination == nullptr ||
        destinationCapacity <= 0)
    {
        return 0;
    }

    auto* state =
        static_cast<NativeCaptureState*>(captureHandle);

    std::lock_guard<std::mutex> lock(
        state->audioBufferMutex);

    const size_t bytesToCopy = std::min(
        state->audioBuffer.size(),
        static_cast<size_t>(destinationCapacity));

    if (bytesToCopy == 0)
    {
        return 0;
    }

    std::memcpy(
        destination,
        state->audioBuffer.data(),
        bytesToCopy);

    state->audioBuffer.erase(
        state->audioBuffer.begin(),
        state->audioBuffer.begin() + bytesToCopy);

    return static_cast<int>(bytesToCopy);
}

void __cdecl NativeAudio_ClearBuffer(
    void* captureHandle)
{
    if (captureHandle == nullptr)
    {
        return;
    }

    auto* state =
        static_cast<NativeCaptureState*>(captureHandle);

    std::lock_guard<std::mutex> lock(
        state->audioBufferMutex);

    state->audioBuffer.clear();
}


void __cdecl NativeAudio_Destroy(
    void* captureHandle)
{
    if (captureHandle == nullptr)
    {
        return;
    }

    auto* state =
        static_cast<NativeCaptureState*>(captureHandle);

    delete state;
}