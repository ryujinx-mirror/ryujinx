using Ryujinx.Audio.SoundIo;
using SoundIOSharp;
using System;
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
        /// The volume of audio renderer
        /// </summary>
        private float _volume = 1.0f;

        /// <summary>
        /// True if the volume of audio renderer have changed
        /// </summary>
        private bool _volumeChanged;

        /// <summary>
        /// The <see cref="SoundIO"/> audio context
        /// </summary>
        private SoundIO _audioContext;

        /// <summary>
        /// The <see cref="SoundIODevice"/> audio device
        /// </summary>
        private SoundIODevice _audioDevice;

        /// <summary>
        /// An object pool containing <see cref="SoundIoAudioTrack"/> objects
        /// </summary>
        private SoundIoAudioTrackPool _trackPool;

        /// <summary>
        /// True if SoundIO is supported on the device
        /// </summary>
        public static bool IsSupported
        {
            get
            {
                return IsSupportedInternal();
            }
        }

        /// <summary>
        /// Constructs a new instance of a <see cref="SoundIoAudioOut"/>
        /// </summary>
        public SoundIoAudioOut()
        {
            _audioContext = new SoundIO();

            _audioContext.Connect();
            _audioContext.FlushEvents();

            _audioDevice = FindNonRawDefaultAudioDevice(_audioContext, true);
            _trackPool   = new SoundIoAudioTrackPool(_audioContext, _audioDevice, MaximumTracks);
        }

        public bool SupportsChannelCount(int channels)
        {
            return _audioDevice.SupportsChannelCount(channels);
        }

        /// <summary>
        /// Creates a new audio track with the specified parameters
        /// </summary>
        /// <param name="sampleRate">The requested sample rate</param>
        /// <param name="hardwareChannels">The requested hardware channels</param>
        /// <param name="virtualChannels">The requested virtual channels</param>
        /// <param name="callback">A <see cref="ReleaseCallback" /> that represents the delegate to invoke when a buffer has been released by the audio track</param>
        /// <returns>The created track's Track ID</returns>
        public int OpenHardwareTrack(int sampleRate, int hardwareChannels, int virtualChannels, ReleaseCallback callback)
        {
            if (!_trackPool.TryGet(out SoundIoAudioTrack track))
            {
                return -1;
            }

            // Open the output. We currently only support 16-bit signed LE
            track.Open(sampleRate, hardwareChannels, virtualChannels, callback, SoundIOFormat.S16LE);

            return track.TrackID;
        }

        /// <summary>
        /// Stops playback and closes the track specified by <paramref name="trackId"/>
        /// </summary>
        /// <param name="trackId">The ID of the track to close</param>
        public void CloseTrack(int trackId)
        {
            if (_trackPool.TryGet(trackId, out SoundIoAudioTrack track))
            {
                // Close and dispose of the track
                track.Close();

                // Recycle the track back into the pool
                _trackPool.Put(track);
            }
        }

        /// <summary>
        /// Returns a value indicating whether the specified buffer is currently reserved by the specified track
        /// </summary>
        /// <param name="trackId">The track to check</param>
        /// <param name="bufferTag">The buffer tag to check</param>
        public bool ContainsBuffer(int trackId, long bufferTag)
        {
            if (_trackPool.TryGet(trackId, out SoundIoAudioTrack track))
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
            if (_trackPool.TryGet(trackId, out SoundIoAudioTrack track))
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
        /// Appends an audio buffer to the specified track
        /// </summary>
        /// <typeparam name="T">The sample type of the buffer</typeparam>
        /// <param name="trackId">The track to append the buffer to</param>
        /// <param name="bufferTag">The internal tag of the buffer</param>
        /// <param name="buffer">The buffer to append to the track</param>
        public void AppendBuffer<T>(int trackId, long bufferTag, T[] buffer) where T : struct
        {
            if (_trackPool.TryGet(trackId, out SoundIoAudioTrack track))
            {
                if (_volumeChanged)
                {
                    track.AudioStream.SetVolume(_volume);

                    _volumeChanged = false;
                }
                    
                track.AppendBuffer(bufferTag, buffer);
            }
        }

        /// <summary>
        /// Starts playback
        /// </summary>
        /// <param name="trackId">The ID of the track to start playback on</param>
        public void Start(int trackId)
        {
            if (_trackPool.TryGet(trackId, out SoundIoAudioTrack track))
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
            if (_trackPool.TryGet(trackId, out SoundIoAudioTrack track))
            {
                track.Stop();
            }
        }

        /// <summary>
        /// Get playback volume
        /// </summary>
        public float GetVolume() => _volume;

        /// <summary>
        /// Set playback volume
        /// </summary>
        /// <param name="volume">The volume of the playback</param>
        public void SetVolume(float volume)
        {
            if (!_volumeChanged)
            {
                _volume        = volume;
                _volumeChanged = true;
            }
        }

        /// <summary>
        /// Gets the current playback state of the specified track
        /// </summary>
        /// <param name="trackId">The track to retrieve the playback state for</param>
        public PlaybackState GetState(int trackId)
        {
            if (_trackPool.TryGet(trackId, out SoundIoAudioTrack track))
            {
                return track.State;
            }

            return PlaybackState.Stopped;
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="SoundIoAudioOut" />
        /// </summary>
        public void Dispose()
        {
            _trackPool.Dispose();
            _audioContext.Disconnect();
            _audioContext.Dispose();
        }

        /// <summary>
        /// Searches for a shared version of the default audio device
        /// </summary>
        /// <param name="audioContext">The <see cref="SoundIO"/> audio context</param>
        /// <param name="fallback">Whether to fallback to the raw default audio device if a non-raw device cannot be found</param>
        private static SoundIODevice FindNonRawDefaultAudioDevice(SoundIO audioContext, bool fallback = false)
        {
            SoundIODevice defaultAudioDevice = audioContext.GetOutputDevice(audioContext.DefaultOutputDeviceIndex);

            if (!defaultAudioDevice.IsRaw)
            {
                return defaultAudioDevice;
            }

            for (int i = 0; i < audioContext.BackendCount; i++)
            {
                SoundIODevice audioDevice = audioContext.GetOutputDevice(i);

                if (audioDevice.Id == defaultAudioDevice.Id && !audioDevice.IsRaw)
                {
                    return audioDevice;
                }
            }

            return fallback ? defaultAudioDevice : null;
        }

        /// <summary>
        /// Determines if SoundIO can connect to a supported backend
        /// </summary>
        /// <returns></returns>
        private static bool IsSupportedInternal()
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

                if (backendDisconnected)
                {
                    return false;
                }

                if (context.OutputDeviceCount == 0)
                {
                    return false;
                }

                device = FindNonRawDefaultAudioDevice(context);

                if (device == null || backendDisconnected)
                {
                    return false;
                }

                stream = device.CreateOutStream();

                if (stream == null || backendDisconnected)
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
                if (stream != null)
                {
                    stream.Dispose();
                }

                if (context != null)
                {
                    context.Dispose();
                }
            }
        }
    }
}