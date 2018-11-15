using SoundIOSharp;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Ryujinx.Audio.SoundIo
{
    internal class SoundIoAudioTrack : IDisposable
    {
        /// <summary>
        /// The audio track ring buffer
        /// </summary>
        private SoundIoRingBuffer m_Buffer;

        /// <summary>
        /// A list of buffers currently pending writeback to the audio backend
        /// </summary>
        private ConcurrentQueue<SoundIoBuffer> m_ReservedBuffers;

        /// <summary>
        /// Occurs when a buffer has been released by the audio backend
        /// </summary>
        private event ReleaseCallback BufferReleased;

        /// <summary>
        /// The track ID of this <see cref="SoundIoAudioTrack"/>
        /// </summary>
        public int TrackID { get; private set; }

        /// <summary>
        /// The current playback state
        /// </summary>
        public PlaybackState State { get; private set; }

        /// <summary>
        /// The <see cref="SoundIO"/> audio context this track belongs to
        /// </summary>
        public SoundIO AudioContext { get; private set; }

        /// <summary>
        /// The <see cref="SoundIODevice"/> this track belongs to
        /// </summary>
        public SoundIODevice AudioDevice { get; private set; }

        /// <summary>
        /// The audio output stream of this track
        /// </summary>
        public SoundIOOutStream AudioStream { get; private set; }

        /// <summary>
        /// Released buffers the track is no longer holding
        /// </summary>
        public ConcurrentQueue<long> ReleasedBuffers { get; private set; }

        /// <summary>
        /// Constructs a new instance of a <see cref="SoundIoAudioTrack"/>
        /// </summary>
        /// <param name="trackId">The track ID</param>
        /// <param name="audioContext">The SoundIO audio context</param>
        /// <param name="audioDevice">The SoundIO audio device</param>
        public SoundIoAudioTrack(int trackId, SoundIO audioContext, SoundIODevice audioDevice)
        {
            TrackID         = trackId;
            AudioContext    = audioContext;
            AudioDevice     = audioDevice;
            State           = PlaybackState.Stopped;
            ReleasedBuffers = new ConcurrentQueue<long>();

            m_Buffer          = new SoundIoRingBuffer();
            m_ReservedBuffers = new ConcurrentQueue<SoundIoBuffer>();
        }

        /// <summary>
        /// Opens the audio track with the specified parameters
        /// </summary>
        /// <param name="sampleRate">The requested sample rate of the track</param>
        /// <param name="channelCount">The requested channel count of the track</param>
        /// <param name="callback">A <see cref="ReleaseCallback" /> that represents the delegate to invoke when a buffer has been released by the audio track</param>
        /// <param name="format">The requested sample format of the track</param>
        public void Open(
            int sampleRate,
            int channelCount,
            ReleaseCallback callback,
            SoundIOFormat format = SoundIOFormat.S16LE)
        {
            // Close any existing audio streams
            if (AudioStream != null)
            {
                Close();
            }

            if (!AudioDevice.SupportsSampleRate(sampleRate))
            {
                throw new InvalidOperationException($"This sound device does not support a sample rate of {sampleRate}Hz");
            }

            if (!AudioDevice.SupportsFormat(format))
            {
                throw new InvalidOperationException($"This sound device does not support SoundIOFormat.{Enum.GetName(typeof(SoundIOFormat), format)}");
            }

            AudioStream = AudioDevice.CreateOutStream();

            AudioStream.Name       = $"SwitchAudioTrack_{TrackID}";
            AudioStream.Layout     = SoundIOChannelLayout.GetDefault(channelCount);
            AudioStream.Format     = format;
            AudioStream.SampleRate = sampleRate;

            AudioStream.WriteCallback = WriteCallback;

            BufferReleased += callback;

            AudioStream.Open();
        }

        /// <summary>
        /// This callback occurs when the sound device is ready to buffer more frames
        /// </summary>
        /// <param name="minFrameCount">The minimum amount of frames expected by the audio backend</param>
        /// <param name="maxFrameCount">The maximum amount of frames that can be written to the audio backend</param>
        private unsafe void WriteCallback(int minFrameCount, int maxFrameCount)
        {
            int  bytesPerFrame  = AudioStream.BytesPerFrame;
            uint bytesPerSample = (uint)AudioStream.BytesPerSample;

            int  bufferedFrames  = m_Buffer.Length / bytesPerFrame;
            long bufferedSamples = m_Buffer.Length / bytesPerSample;

            int frameCount = Math.Min(bufferedFrames, maxFrameCount);

            if (frameCount == 0)
            {
                return;
            }

            SoundIOChannelAreas areas = AudioStream.BeginWrite(ref frameCount);
            int channelCount = areas.ChannelCount;

            byte[] samples = new byte[frameCount * bytesPerFrame];

            m_Buffer.Read(samples, 0, samples.Length);

            // This is a huge ugly block of code, but we save
            // a significant amount of time over the generic
            // loop that handles other channel counts.

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

            AudioStream.EndWrite();

            UpdateReleasedBuffers(samples.Length);
        }

        /// <summary>
        /// Releases any buffers that have been fully written to the output device
        /// </summary>
        /// <param name="bytesRead">The amount of bytes written in the last device write</param>
        private void UpdateReleasedBuffers(int bytesRead)
        {
            bool bufferReleased = false;

            while (bytesRead > 0)
            {
                if (m_ReservedBuffers.TryPeek(out SoundIoBuffer buffer))
                {
                    if (buffer.Length > bytesRead)
                    {
                        buffer.Length -= bytesRead;
                        bytesRead = 0;
                    }
                    else
                    {
                        bufferReleased = true;
                        bytesRead -= buffer.Length;

                        m_ReservedBuffers.TryDequeue(out buffer);
                        ReleasedBuffers.Enqueue(buffer.Tag);
                    }
                }
            }

            if (bufferReleased)
            {
                OnBufferReleased();
            }
        }

        /// <summary>
        /// Starts audio playback
        /// </summary>
        public void Start()
        {
            if (AudioStream == null)
            {
                return;
            }

            AudioStream.Start();
            AudioStream.Pause(false);
            AudioContext.FlushEvents();
            State = PlaybackState.Playing;
        }

        /// <summary>
        /// Stops audio playback
        /// </summary>
        public void Stop()
        {
            if (AudioStream == null)
            {
                return;
            }

            AudioStream.Pause(true);
            AudioContext.FlushEvents();
            State = PlaybackState.Stopped;
        }

        /// <summary>
        /// Appends an audio buffer to the tracks internal ring buffer
        /// </summary>
        /// <typeparam name="T">The audio sample type</typeparam>
        /// <param name="bufferTag">The unqiue tag of the buffer being appended</param>
        /// <param name="buffer">The buffer to append</param>
        public void AppendBuffer<T>(long bufferTag, T[] buffer)
        {
            if (AudioStream == null)
            {
                return;
            }

            // Calculate the size of the audio samples
            int size = Unsafe.SizeOf<T>();

            // Calculate the amount of bytes to copy from the buffer
            int bytesToCopy = size * buffer.Length;

            // Copy the memory to our ring buffer
            m_Buffer.Write(buffer, 0, bytesToCopy);

            // Keep track of "buffered" buffers
            m_ReservedBuffers.Enqueue(new SoundIoBuffer(bufferTag, bytesToCopy));
        }

        /// <summary>
        /// Returns a value indicating whether the specified buffer is currently reserved by the track
        /// </summary>
        /// <param name="bufferTag">The buffer tag to check</param>
        public bool ContainsBuffer(long bufferTag)
        {
            return m_ReservedBuffers.Any(x => x.Tag == bufferTag);
        }

        /// <summary>
        /// Closes the <see cref="SoundIoAudioTrack"/>
        /// </summary>
        public void Close()
        {
            if (AudioStream != null)
            {
                AudioStream.Pause(true);
                AudioStream.Dispose();
            }

            m_Buffer.Clear();
            OnBufferReleased();
            ReleasedBuffers.Clear();

            State          = PlaybackState.Stopped;
            AudioStream    = null;
            BufferReleased = null;
        }

        private void OnBufferReleased()
        {
            BufferReleased?.Invoke();
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="SoundIoAudioTrack" />
        /// </summary>
        public void Dispose()
        {
            Close();
        }

        ~SoundIoAudioTrack()
        {
            Dispose();
        }
    }
}
