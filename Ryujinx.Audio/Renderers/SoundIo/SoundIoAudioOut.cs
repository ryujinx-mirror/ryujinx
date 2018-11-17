using Ryujinx.Audio.SoundIo;
using SoundIOSharp;
using System.Collections.Generic;

namespace Ryujinx.Audio
{
    /// <summary>
    /// An audio renderer that uses libsoundio as the audio backend
    /// </summary>
    public class SoundIoAudioOut : IAalOutput
    {
        /// <summary>
        /// The maximum amount of tracks we can issue simultaneously
        /// </summary>
        private const int MaximumTracks = 256;

        /// <summary>
        /// The <see cref="SoundIO"/> audio context
        /// </summary>
        private SoundIO m_AudioContext;

        /// <summary>
        /// The <see cref="SoundIODevice"/> audio device
        /// </summary>
        private SoundIODevice m_AudioDevice;

        /// <summary>
        /// An object pool containing <see cref="SoundIoAudioTrack"/> objects
        /// </summary>
        private SoundIoAudioTrackPool m_TrackPool;

        /// <summary>
        /// True if SoundIO is supported on the device.
        /// </summary>
        public static bool IsSupported
        {
            get
            {
                SoundIO          context = null;
                SoundIODevice    device  = null;
                SoundIOOutStream stream  = null;

                bool backendDisconnected = false;

                try
                {
                    context = new SoundIO();

                    context.OnBackendDisconnect = (i) => {
                        backendDisconnected = true;
                    };

                    context.Connect();
                    context.FlushEvents();

                    if(backendDisconnected)
                    {
                        return false;
                    }

                    device = context.GetOutputDevice(context.DefaultOutputDeviceIndex);

                    if(device == null || backendDisconnected)
                    {
                        return false;
                    }

                    stream = device.CreateOutStream();

                    if(stream == null || backendDisconnected)
                    {
                        return false;
                    }

                    return true;
                }
                catch
                {
                    return false;
                }
                finally
                {
                    if(stream != null)
                    {
                        stream.Dispose();
                    }

                    if(context != null)
                    {
                        context.Dispose();
                    }
                }
            }
        }

        /// <summary>
        /// Constructs a new instance of a <see cref="SoundIoAudioOut"/>
        /// </summary>
        public SoundIoAudioOut()
        {
            m_AudioContext = new SoundIO();

            m_AudioContext.Connect();
            m_AudioContext.FlushEvents();

            m_AudioDevice = m_AudioContext.GetOutputDevice(m_AudioContext.DefaultOutputDeviceIndex);
            m_TrackPool = new SoundIoAudioTrackPool(m_AudioContext, m_AudioDevice, MaximumTracks);
        }

        /// <summary>
        /// Gets the current playback state of the specified track
        /// </summary>
        /// <param name="trackId">The track to retrieve the playback state for</param>
        public PlaybackState GetState(int trackId)
        {
            if (m_TrackPool.TryGet(trackId, out SoundIoAudioTrack track))
            {
                return track.State;
            }

            return PlaybackState.Stopped;
        }

        /// <summary>
        /// Creates a new audio track with the specified parameters
        /// </summary>
        /// <param name="sampleRate">The requested sample rate</param>
        /// <param name="channels">The requested channels</param>
        /// <param name="callback">A <see cref="ReleaseCallback" /> that represents the delegate to invoke when a buffer has been released by the audio track</param>
        /// <returns>The created track's Track ID</returns>
        public int OpenTrack(int sampleRate, int channels, ReleaseCallback callback)
        {
            if (!m_TrackPool.TryGet(out SoundIoAudioTrack track))
            {
                return -1;
            }

            // Open the output. We currently only support 16-bit signed LE
            track.Open(sampleRate, channels, callback, SoundIOFormat.S16LE);

            return track.TrackID;
        }

        /// <summary>
        /// Stops playback and closes the track specified by <paramref name="trackId"/>
        /// </summary>
        /// <param name="trackId">The ID of the track to close</param>
        public void CloseTrack(int trackId)
        {
            if (m_TrackPool.TryGet(trackId, out SoundIoAudioTrack track))
            {
                // Close and dispose of the track
                track.Close();

                // Recycle the track back into the pool
                m_TrackPool.Put(track);
            }
        }

        /// <summary>
        /// Starts playback
        /// </summary>
        /// <param name="trackId">The ID of the track to start playback on</param>
        public void Start(int trackId)
        {
            if (m_TrackPool.TryGet(trackId, out SoundIoAudioTrack track))
            {
                track.Start();
            }
        }

        /// <summary>
        /// Stops playback
        /// </summary>
        /// <param name="trackId">The ID of the track to stop playback on</param>
        public void Stop(int trackId)
        {
            if (m_TrackPool.TryGet(trackId, out SoundIoAudioTrack track))
            {
                track.Stop();
            }
        }

        /// <summary>
        /// Appends an audio buffer to the specified track
        /// </summary>
        /// <typeparam name="T">The sample type of the buffer</typeparam>
        /// <param name="trackId">The track to append the buffer to</param>
        /// <param name="bufferTag">The internal tag of the buffer</param>
        /// <param name="buffer">The buffer to append to the track</param>
        public void AppendBuffer<T>(int trackId, long bufferTag, T[] buffer)
            where T : struct
        {
            if(m_TrackPool.TryGet(trackId, out SoundIoAudioTrack track))
            {
                track.AppendBuffer(bufferTag, buffer);
            }
        }

        /// <summary>
        /// Returns a value indicating whether the specified buffer is currently reserved by the specified track
        /// </summary>
        /// <param name="trackId">The track to check</param>
        /// <param name="bufferTag">The buffer tag to check</param>
        public bool ContainsBuffer(int trackId, long bufferTag)
        {
            if (m_TrackPool.TryGet(trackId, out SoundIoAudioTrack track))
            {
                return track.ContainsBuffer(bufferTag);
            }

            return false;
        }

        /// <summary>
        /// Gets a list of buffer tags the specified track is no longer reserving
        /// </summary>
        /// <param name="trackId">The track to retrieve buffer tags from</param>
        /// <param name="maxCount">The maximum amount of buffer tags to retrieve</param>
        /// <returns>Buffers released by the specified track</returns>
        public long[] GetReleasedBuffers(int trackId, int maxCount)
        {
            if (m_TrackPool.TryGet(trackId, out SoundIoAudioTrack track))
            {
                List<long> bufferTags = new List<long>();

                while(maxCount-- > 0 && track.ReleasedBuffers.TryDequeue(out long tag))
                {
                    bufferTags.Add(tag);
                }

                return bufferTags.ToArray();
            }

            return new long[0];
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="SoundIoAudioOut" />
        /// </summary>
        public void Dispose()
        {
            m_TrackPool.Dispose();
            m_AudioContext.Disconnect();
            m_AudioContext.Dispose();
        }
    }
}