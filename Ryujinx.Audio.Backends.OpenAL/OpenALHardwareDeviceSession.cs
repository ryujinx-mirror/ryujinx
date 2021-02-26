using OpenTK.Audio.OpenAL;
using Ryujinx.Audio.Backends.Common;
using Ryujinx.Audio.Common;
using Ryujinx.Memory;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Ryujinx.Audio.Backends.OpenAL
{
    class OpenALHardwareDeviceSession : HardwareDeviceSessionOutputBase
    {
        private OpenALHardwareDeviceDriver _driver;
        private int _sourceId;
        private ALFormat _targetFormat;
        private bool _isActive;
        private Queue<OpenALAudioBuffer> _queuedBuffers;
        private ulong _playedSampleCount;

        private object _lock = new object();

        public OpenALHardwareDeviceSession(OpenALHardwareDeviceDriver driver, IVirtualMemoryManager memoryManager, SampleFormat requestedSampleFormat, uint requestedSampleRate, uint requestedChannelCount) : base(memoryManager, requestedSampleFormat, requestedSampleRate, requestedChannelCount)
        {
            _driver = driver;
            _queuedBuffers = new Queue<OpenALAudioBuffer>();
            _sourceId = AL.GenSource();
            _targetFormat = GetALFormat();
            _isActive = false;
            _playedSampleCount = 0;
        }

        private ALFormat GetALFormat()
        {
            switch (RequestedSampleFormat)
            {
                case SampleFormat.PcmInt16:
                    switch (RequestedChannelCount)
                    {
                        case 1:
                            return ALFormat.Mono16;
                        case 2:
                            return ALFormat.Stereo16;
                        case 6:
                            return ALFormat.Multi51Chn16Ext;
                        default:
                            throw new NotImplementedException($"Unsupported channel config {RequestedChannelCount}");
                    }
                default:
                    throw new NotImplementedException($"Unsupported sample format {RequestedSampleFormat}");
            }
        }

        public override void PrepareToClose() { }

        private void StartIfNotPlaying()
        {
            AL.GetSource(_sourceId, ALGetSourcei.SourceState, out int stateInt);

            ALSourceState State = (ALSourceState)stateInt;

            if (State != ALSourceState.Playing)
            {
                AL.SourcePlay(_sourceId);
            }
        }

        public override void QueueBuffer(AudioBuffer buffer)
        {
            lock (_lock)
            {
                OpenALAudioBuffer driverBuffer = new OpenALAudioBuffer
                {
                    DriverIdentifier = buffer.DataPointer,
                    BufferId         = AL.GenBuffer(),
                    SampleCount      = GetSampleCount(buffer)
                };

                AL.BufferData(driverBuffer.BufferId, _targetFormat, buffer.Data, (int)buffer.DataSize, (int)RequestedSampleRate);

                _queuedBuffers.Enqueue(driverBuffer);

                AL.SourceQueueBuffer(_sourceId, driverBuffer.BufferId);

                if (_isActive)
                {
                    StartIfNotPlaying();
                }
            }
        }

        public override void SetVolume(float volume)
        {
            lock (_lock)
            {
                AL.Source(_sourceId, ALSourcef.Gain, volume);
            }
        }

        public override float GetVolume()
        {
            AL.GetSource(_sourceId, ALSourcef.Gain, out float volume);

            return volume;
        }

        public override void Start()
        {
            lock (_lock)
            {
                _isActive = true;

                StartIfNotPlaying();
            }
        }

        public override void Stop()
        {
            lock (_lock)
            {
                SetVolume(0.0f);

                AL.SourceStop(_sourceId);

                _isActive = false;
            }
        }

        public override void UnregisterBuffer(AudioBuffer buffer) {}

        public override bool WasBufferFullyConsumed(AudioBuffer buffer)
        {
            lock (_lock)
            {
                if (!_queuedBuffers.TryPeek(out OpenALAudioBuffer driverBuffer))
                {
                    return true;
                }

                return driverBuffer.DriverIdentifier != buffer.DataPointer;
            }
        }

        public override ulong GetPlayedSampleCount()
        {
            lock (_lock)
            {
                return _playedSampleCount;
            }
        }

        public bool Update()
        {
            lock (_lock)
            {
                if (_isActive)
                {
                    AL.GetSource(_sourceId, ALGetSourcei.BuffersProcessed, out int releasedCount);

                    if (releasedCount > 0)
                    {
                        int[] bufferIds = new int[releasedCount];

                        AL.SourceUnqueueBuffers(_sourceId, releasedCount, bufferIds);

                        int i = 0;

                        while (_queuedBuffers.TryPeek(out OpenALAudioBuffer buffer) && i < bufferIds.Length)
                        {
                            if (buffer.BufferId == bufferIds[i])
                            {
                                _playedSampleCount += buffer.SampleCount;

                                _queuedBuffers.TryDequeue(out _);

                                i++;
                            }
                        }

                        Debug.Assert(i < bufferIds.Length, "Unknown buffer id");

                        AL.DeleteBuffers(bufferIds);
                    }

                    return releasedCount > 0;
                }

                return false;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (_lock)
                {
                    PrepareToClose();
                    Stop();

                    AL.DeleteSource(_sourceId);

                    _driver.Unregister(this);
                }
            }
        }

        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
