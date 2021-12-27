using Ryujinx.Audio.Backends.Common;
using Ryujinx.Audio.Common;
using Ryujinx.Common.Logging;
using Ryujinx.Memory;
using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading;

using static SDL2.SDL;

namespace Ryujinx.Audio.Backends.SDL2
{
    class SDL2HardwareDeviceSession : HardwareDeviceSessionOutputBase
    {
        private SDL2HardwareDeviceDriver _driver;
        private ConcurrentQueue<SDL2AudioBuffer> _queuedBuffers;
        private DynamicRingBuffer _ringBuffer;
        private ulong _playedSampleCount;
        private ManualResetEvent _updateRequiredEvent;
        private uint _outputStream;
        private SDL_AudioCallback _callbackDelegate;
        private int _bytesPerFrame;
        private uint _sampleCount;
        private bool _started;
        private float _volume;
        private ushort _nativeSampleFormat;

        public SDL2HardwareDeviceSession(SDL2HardwareDeviceDriver driver, IVirtualMemoryManager memoryManager, SampleFormat requestedSampleFormat, uint requestedSampleRate, uint requestedChannelCount, float requestedVolume) : base(memoryManager, requestedSampleFormat, requestedSampleRate, requestedChannelCount)
        {
            _driver = driver;
            _updateRequiredEvent = _driver.GetUpdateRequiredEvent();
            _queuedBuffers = new ConcurrentQueue<SDL2AudioBuffer>();
            _ringBuffer = new DynamicRingBuffer();
            _callbackDelegate = Update;
            _bytesPerFrame = BackendHelper.GetSampleSize(RequestedSampleFormat) * (int)RequestedChannelCount;
            _nativeSampleFormat = SDL2HardwareDeviceDriver.GetSDL2Format(RequestedSampleFormat);
            _sampleCount = uint.MaxValue;
            _started = false;
            _volume = requestedVolume;
        }

        private void EnsureAudioStreamSetup(AudioBuffer buffer)
        {
            uint bufferSampleCount = (uint)GetSampleCount(buffer);
            bool needAudioSetup = _outputStream == 0 ||
                (bufferSampleCount >= Constants.TargetSampleCount && bufferSampleCount < _sampleCount);

            if (needAudioSetup)
            {
                _sampleCount = Math.Max(Constants.TargetSampleCount, bufferSampleCount);

                uint newOutputStream = SDL2HardwareDeviceDriver.OpenStream(RequestedSampleFormat, RequestedSampleRate, RequestedChannelCount, _sampleCount, _callbackDelegate);

                if (newOutputStream == 0)
                {
                    // No stream in place, this is unexpected.
                    throw new InvalidOperationException($"OpenStream failed with error: \"{SDL_GetError()}\"");
                }
                else
                {
                    if (_outputStream != 0)
                    {
                        SDL_CloseAudioDevice(_outputStream);
                    }

                    _outputStream = newOutputStream;

                    SDL_PauseAudioDevice(_outputStream, _started ? 0 : 1);

                    Logger.Info?.Print(LogClass.Audio, $"New audio stream setup with a target sample count of {_sampleCount}");
                }
            }
        }

        private unsafe void Update(IntPtr userdata, IntPtr stream, int streamLength)
        {
            Span<byte> streamSpan = new Span<byte>((void*)stream, streamLength);

            int maxFrameCount = (int)GetSampleCount(streamLength);
            int bufferedFrames = _ringBuffer.Length / _bytesPerFrame;

            int frameCount = Math.Min(bufferedFrames, maxFrameCount);

            if (frameCount == 0)
            {
                // SDL2 left the responsibility to the user to clear the buffer.
                streamSpan.Fill(0);

                return;
            }

            byte[] samples = new byte[frameCount * _bytesPerFrame];

            _ringBuffer.Read(samples, 0, samples.Length);

            fixed (byte* p = samples)
            {
                IntPtr pStreamSrc = (IntPtr)p;

                // Zero the dest buffer
                streamSpan.Fill(0);

                // Apply volume to written data
                SDL_MixAudioFormat(stream, pStreamSrc, _nativeSampleFormat, (uint)samples.Length, (int)(_volume * SDL_MIX_MAXVOLUME));
            }

            ulong sampleCount = GetSampleCount(samples.Length);

            ulong availaibleSampleCount = sampleCount;

            bool needUpdate = false;

            while (availaibleSampleCount > 0 && _queuedBuffers.TryPeek(out SDL2AudioBuffer driverBuffer))
            {
                ulong sampleStillNeeded = driverBuffer.SampleCount - Interlocked.Read(ref driverBuffer.SamplePlayed);
                ulong playedAudioBufferSampleCount = Math.Min(sampleStillNeeded, availaibleSampleCount);

                ulong currentSamplePlayed = Interlocked.Add(ref driverBuffer.SamplePlayed, playedAudioBufferSampleCount);
                availaibleSampleCount -= playedAudioBufferSampleCount;

                if (currentSamplePlayed == driverBuffer.SampleCount)
                {
                    _queuedBuffers.TryDequeue(out _);

                    needUpdate = true;
                }

                Interlocked.Add(ref _playedSampleCount, playedAudioBufferSampleCount);
            }

            // Notify the output if needed.
            if (needUpdate)
            {
                _updateRequiredEvent.Set();
            }
        }

        public override ulong GetPlayedSampleCount()
        {
            return Interlocked.Read(ref _playedSampleCount);
        }

        public override float GetVolume()
        {
            return _volume;
        }

        public override void PrepareToClose() { }

        public override void QueueBuffer(AudioBuffer buffer)
        {
            EnsureAudioStreamSetup(buffer);

            SDL2AudioBuffer driverBuffer = new SDL2AudioBuffer(buffer.DataPointer, GetSampleCount(buffer));

            _ringBuffer.Write(buffer.Data, 0, buffer.Data.Length);

            _queuedBuffers.Enqueue(driverBuffer);
        }

        public override void SetVolume(float volume)
        {
            _volume = volume;
        }

        public override void Start()
        {
            if (!_started)
            {
                if (_outputStream != 0)
                {
                    SDL_PauseAudioDevice(_outputStream, 0);
                }

                _started = true;
            }
        }

        public override void Stop()
        {
            if (_started)
            {
                if (_outputStream != 0)
                {
                    SDL_PauseAudioDevice(_outputStream, 1);
                }

                _started = false;
            }
        }

        public override void UnregisterBuffer(AudioBuffer buffer) { }

        public override bool WasBufferFullyConsumed(AudioBuffer buffer)
        {
            if (!_queuedBuffers.TryPeek(out SDL2AudioBuffer driverBuffer))
            {
                return true;
            }

            return driverBuffer.DriverIdentifier != buffer.DataPointer;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && _driver.Unregister(this))
            {
                PrepareToClose();
                Stop();

                if (_outputStream != 0)
                {
                    SDL_CloseAudioDevice(_outputStream);
                }
            }
        }

        public override void Dispose()
        {
            Dispose(true);
        }
    }
}
