using Ryujinx.Audio.Backends.Common;
using Ryujinx.Audio.Common;
using Ryujinx.Memory;
using SoundIOSharp;
using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Ryujinx.Audio.Backends.SoundIo
{
    class SoundIoHardwareDeviceSession : HardwareDeviceSessionOutputBase
    {
        private SoundIoHardwareDeviceDriver _driver;
        private ConcurrentQueue<SoundIoAudioBuffer> _queuedBuffers;
        private SoundIOOutStream _outputStream;
        private DynamicRingBuffer _ringBuffer;
        private ulong _playedSampleCount;
        private ManualResetEvent _updateRequiredEvent;

        public SoundIoHardwareDeviceSession(SoundIoHardwareDeviceDriver driver, IVirtualMemoryManager memoryManager, SampleFormat requestedSampleFormat, uint requestedSampleRate, uint requestedChannelCount) : base(memoryManager, requestedSampleFormat, requestedSampleRate, requestedChannelCount)
        {
            _driver = driver;
            _updateRequiredEvent = _driver.GetUpdateRequiredEvent();
            _queuedBuffers = new ConcurrentQueue<SoundIoAudioBuffer>();
            _ringBuffer = new DynamicRingBuffer();

            SetupOutputStream();
        }

        private void SetupOutputStream()
        {
            _outputStream = _driver.OpenStream(RequestedSampleFormat, RequestedSampleRate, RequestedChannelCount);
            _outputStream.WriteCallback += Update;

            // TODO: Setup other callbacks (errors, ect).

            _outputStream.Open();
        }

        public override ulong GetPlayedSampleCount()
        {
            return Interlocked.Read(ref _playedSampleCount);
        }

        public override float GetVolume()
        {
            return _outputStream.Volume;
        }

        public override void PrepareToClose() { }

        public override void QueueBuffer(AudioBuffer buffer)
        {
            SoundIoAudioBuffer driverBuffer = new SoundIoAudioBuffer(buffer.DataPointer, GetSampleCount(buffer));

            _ringBuffer.Write(buffer.Data, 0, buffer.Data.Length);

            _queuedBuffers.Enqueue(driverBuffer);
        }

        public override void SetVolume(float volume)
        {
            _outputStream.SetVolume(volume);
        }

        public override void Start()
        {
            _outputStream.Start();
            _outputStream.Pause(false);

            _driver.FlushContextEvents();
        }

        public override void Stop()
        {
            _outputStream.Pause(true);

            _driver.FlushContextEvents();
        }

        public override void UnregisterBuffer(AudioBuffer buffer) {}

        public override bool WasBufferFullyConsumed(AudioBuffer buffer)
        {
            if (!_queuedBuffers.TryPeek(out SoundIoAudioBuffer driverBuffer))
            {
                return true;
            }

            return driverBuffer.DriverIdentifier != buffer.DataPointer;
        }

        private unsafe void Update(int minFrameCount, int maxFrameCount)
        {
            int bytesPerFrame = _outputStream.BytesPerFrame;
            uint bytesPerSample = (uint)_outputStream.BytesPerSample;

            int bufferedFrames = _ringBuffer.Length / bytesPerFrame;

            int frameCount = Math.Min(bufferedFrames, maxFrameCount);

            if (frameCount == 0)
            {
                return;
            }

            SoundIOChannelAreas areas = _outputStream.BeginWrite(ref frameCount);

            int channelCount = areas.ChannelCount;

            byte[] samples = new byte[frameCount * bytesPerFrame];

            _ringBuffer.Read(samples, 0, samples.Length);

            // This is a huge ugly block of code, but we save
            // a significant amount of time over the generic
            // loop that handles other channel counts.
            // TODO: Is this still right in 2021?

            // Mono
            if (channelCount == 1)
            {
                SoundIOChannelArea area = areas.GetArea(0);

                fixed (byte* srcptr = samples)
                {
                    if (bytesPerSample == 1)
                    {
                        for (int frame = 0; frame < frameCount; frame++)
                        {
                            ((byte*)area.Pointer)[0] = srcptr[frame * bytesPerFrame];

                            area.Pointer += area.Step;
                        }
                    }
                    else if (bytesPerSample == 2)
                    {
                        for (int frame = 0; frame < frameCount; frame++)
                        {
                            ((short*)area.Pointer)[0] = ((short*)srcptr)[frame * bytesPerFrame >> 1];

                            area.Pointer += area.Step;
                        }
                    }
                    else if (bytesPerSample == 4)
                    {
                        for (int frame = 0; frame < frameCount; frame++)
                        {
                            ((int*)area.Pointer)[0] = ((int*)srcptr)[frame * bytesPerFrame >> 2];

                            area.Pointer += area.Step;
                        }
                    }
                    else
                    {
                        for (int frame = 0; frame < frameCount; frame++)
                        {
                            Unsafe.CopyBlockUnaligned((byte*)area.Pointer, srcptr + (frame * bytesPerFrame), bytesPerSample);

                            area.Pointer += area.Step;
                        }
                    }
                }
            }
            // Stereo
            else if (channelCount == 2)
            {
                SoundIOChannelArea area1 = areas.GetArea(0);
                SoundIOChannelArea area2 = areas.GetArea(1);

                fixed (byte* srcptr = samples)
                {
                    if (bytesPerSample == 1)
                    {
                        for (int frame = 0; frame < frameCount; frame++)
                        {
                            // Channel 1
                            ((byte*)area1.Pointer)[0] = srcptr[(frame * bytesPerFrame) + 0];

                            // Channel 2
                            ((byte*)area2.Pointer)[0] = srcptr[(frame * bytesPerFrame) + 1];

                            area1.Pointer += area1.Step;
                            area2.Pointer += area2.Step;
                        }
                    }
                    else if (bytesPerSample == 2)
                    {
                        for (int frame = 0; frame < frameCount; frame++)
                        {
                            // Channel 1
                            ((short*)area1.Pointer)[0] = ((short*)srcptr)[(frame * bytesPerFrame >> 1) + 0];

                            // Channel 2
                            ((short*)area2.Pointer)[0] = ((short*)srcptr)[(frame * bytesPerFrame >> 1) + 1];

                            area1.Pointer += area1.Step;
                            area2.Pointer += area2.Step;
                        }
                    }
                    else if (bytesPerSample == 4)
                    {
                        for (int frame = 0; frame < frameCount; frame++)
                        {
                            // Channel 1
                            ((int*)area1.Pointer)[0] = ((int*)srcptr)[(frame * bytesPerFrame >> 2) + 0];

                            // Channel 2
                            ((int*)area2.Pointer)[0] = ((int*)srcptr)[(frame * bytesPerFrame >> 2) + 1];

                            area1.Pointer += area1.Step;
                            area2.Pointer += area2.Step;
                        }
                    }
                    else
                    {
                        for (int frame = 0; frame < frameCount; frame++)
                        {
                            // Channel 1
                            Unsafe.CopyBlockUnaligned((byte*)area1.Pointer, srcptr + (frame * bytesPerFrame) + (0 * bytesPerSample), bytesPerSample);

                            // Channel 2
                            Unsafe.CopyBlockUnaligned((byte*)area2.Pointer, srcptr + (frame * bytesPerFrame) + (1 * bytesPerSample), bytesPerSample);

                            area1.Pointer += area1.Step;
                            area2.Pointer += area2.Step;
                        }
                    }
                }
            }
            // Surround
            else if (channelCount == 6)
            {
                SoundIOChannelArea area1 = areas.GetArea(0);
                SoundIOChannelArea area2 = areas.GetArea(1);
                SoundIOChannelArea area3 = areas.GetArea(2);
                SoundIOChannelArea area4 = areas.GetArea(3);
                SoundIOChannelArea area5 = areas.GetArea(4);
                SoundIOChannelArea area6 = areas.GetArea(5);

                fixed (byte* srcptr = samples)
                {
                    if (bytesPerSample == 1)
                    {
                        for (int frame = 0; frame < frameCount; frame++)
                        {
                            // Channel 1
                            ((byte*)area1.Pointer)[0] = srcptr[(frame * bytesPerFrame) + 0];

                            // Channel 2
                            ((byte*)area2.Pointer)[0] = srcptr[(frame * bytesPerFrame) + 1];

                            // Channel 3
                            ((byte*)area3.Pointer)[0] = srcptr[(frame * bytesPerFrame) + 2];

                            // Channel 4
                            ((byte*)area4.Pointer)[0] = srcptr[(frame * bytesPerFrame) + 3];

                            // Channel 5
                            ((byte*)area5.Pointer)[0] = srcptr[(frame * bytesPerFrame) + 4];

                            // Channel 6
                            ((byte*)area6.Pointer)[0] = srcptr[(frame * bytesPerFrame) + 5];

                            area1.Pointer += area1.Step;
                            area2.Pointer += area2.Step;
                            area3.Pointer += area3.Step;
                            area4.Pointer += area4.Step;
                            area5.Pointer += area5.Step;
                            area6.Pointer += area6.Step;
                        }
                    }
                    else if (bytesPerSample == 2)
                    {
                        for (int frame = 0; frame < frameCount; frame++)
                        {
                            // Channel 1
                            ((short*)area1.Pointer)[0] = ((short*)srcptr)[(frame * bytesPerFrame >> 1) + 0];

                            // Channel 2
                            ((short*)area2.Pointer)[0] = ((short*)srcptr)[(frame * bytesPerFrame >> 1) + 1];

                            // Channel 3
                            ((short*)area3.Pointer)[0] = ((short*)srcptr)[(frame * bytesPerFrame >> 1) + 2];

                            // Channel 4
                            ((short*)area4.Pointer)[0] = ((short*)srcptr)[(frame * bytesPerFrame >> 1) + 3];

                            // Channel 5
                            ((short*)area5.Pointer)[0] = ((short*)srcptr)[(frame * bytesPerFrame >> 1) + 4];

                            // Channel 6
                            ((short*)area6.Pointer)[0] = ((short*)srcptr)[(frame * bytesPerFrame >> 1) + 5];

                            area1.Pointer += area1.Step;
                            area2.Pointer += area2.Step;
                            area3.Pointer += area3.Step;
                            area4.Pointer += area4.Step;
                            area5.Pointer += area5.Step;
                            area6.Pointer += area6.Step;
                        }
                    }
                    else if (bytesPerSample == 4)
                    {
                        for (int frame = 0; frame < frameCount; frame++)
                        {
                            // Channel 1
                            ((int*)area1.Pointer)[0] = ((int*)srcptr)[(frame * bytesPerFrame >> 2) + 0];

                            // Channel 2
                            ((int*)area2.Pointer)[0] = ((int*)srcptr)[(frame * bytesPerFrame >> 2) + 1];

                            // Channel 3
                            ((int*)area3.Pointer)[0] = ((int*)srcptr)[(frame * bytesPerFrame >> 2) + 2];

                            // Channel 4
                            ((int*)area4.Pointer)[0] = ((int*)srcptr)[(frame * bytesPerFrame >> 2) + 3];

                            // Channel 5
                            ((int*)area5.Pointer)[0] = ((int*)srcptr)[(frame * bytesPerFrame >> 2) + 4];

                            // Channel 6
                            ((int*)area6.Pointer)[0] = ((int*)srcptr)[(frame * bytesPerFrame >> 2) + 5];

                            area1.Pointer += area1.Step;
                            area2.Pointer += area2.Step;
                            area3.Pointer += area3.Step;
                            area4.Pointer += area4.Step;
                            area5.Pointer += area5.Step;
                            area6.Pointer += area6.Step;
                        }
                    }
                    else
                    {
                        for (int frame = 0; frame < frameCount; frame++)
                        {
                            // Channel 1
                            Unsafe.CopyBlockUnaligned((byte*)area1.Pointer, srcptr + (frame * bytesPerFrame) + (0 * bytesPerSample), bytesPerSample);

                            // Channel 2
                            Unsafe.CopyBlockUnaligned((byte*)area2.Pointer, srcptr + (frame * bytesPerFrame) + (1 * bytesPerSample), bytesPerSample);

                            // Channel 3
                            Unsafe.CopyBlockUnaligned((byte*)area3.Pointer, srcptr + (frame * bytesPerFrame) + (2 * bytesPerSample), bytesPerSample);

                            // Channel 4
                            Unsafe.CopyBlockUnaligned((byte*)area4.Pointer, srcptr + (frame * bytesPerFrame) + (3 * bytesPerSample), bytesPerSample);

                            // Channel 5
                            Unsafe.CopyBlockUnaligned((byte*)area5.Pointer, srcptr + (frame * bytesPerFrame) + (4 * bytesPerSample), bytesPerSample);

                            // Channel 6
                            Unsafe.CopyBlockUnaligned((byte*)area6.Pointer, srcptr + (frame * bytesPerFrame) + (5 * bytesPerSample), bytesPerSample);

                            area1.Pointer += area1.Step;
                            area2.Pointer += area2.Step;
                            area3.Pointer += area3.Step;
                            area4.Pointer += area4.Step;
                            area5.Pointer += area5.Step;
                            area6.Pointer += area6.Step;
                        }
                    }
                }
            }
            // Every other channel count
            else
            {
                SoundIOChannelArea[] channels = new SoundIOChannelArea[channelCount];

                // Obtain the channel area for each channel
                for (int i = 0; i < channelCount; i++)
                {
                    channels[i] = areas.GetArea(i);
                }

                fixed (byte* srcptr = samples)
                {
                    for (int frame = 0; frame < frameCount; frame++)
                        for (int channel = 0; channel < areas.ChannelCount; channel++)
                        {
                            // Copy channel by channel, frame by frame. This is slow!
                            Unsafe.CopyBlockUnaligned((byte*)channels[channel].Pointer, srcptr + (frame * bytesPerFrame) + (channel * bytesPerSample), bytesPerSample);

                            channels[channel].Pointer += channels[channel].Step;
                        }
                }
            }

            _outputStream.EndWrite();

            ulong sampleCount = (ulong)(samples.Length / bytesPerSample / channelCount);

            ulong availaibleSampleCount = sampleCount;

            bool needUpdate = false;

            while (availaibleSampleCount > 0 && _queuedBuffers.TryPeek(out SoundIoAudioBuffer driverBuffer))
            {
                ulong sampleStillNeeded = driverBuffer.SampleCount - Interlocked.Read(ref driverBuffer.SamplePlayed);
                ulong playedAudioBufferSampleCount = Math.Min(sampleStillNeeded, availaibleSampleCount);

                Interlocked.Add(ref driverBuffer.SamplePlayed, playedAudioBufferSampleCount);
                availaibleSampleCount -= playedAudioBufferSampleCount;

                if (Interlocked.Read(ref driverBuffer.SamplePlayed) == driverBuffer.SampleCount)
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

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                PrepareToClose();
                Stop();

                _outputStream.Dispose();

                _driver.Unregister(this);
            }
        }

        public override void Dispose()
        {
            Dispose(true);
        }
    }
}
