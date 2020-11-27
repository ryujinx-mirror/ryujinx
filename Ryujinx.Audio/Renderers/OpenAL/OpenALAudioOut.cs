using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading;

namespace Ryujinx.Audio
{
    /// <summary>
    /// An audio renderer that uses OpenAL as the audio backend
    /// </summary>
    public class OpenALAudioOut : IAalOutput, IDisposable
    {
        /// <summary>
        /// The maximum amount of tracks we can issue simultaneously
        /// </summary>
        private const int MaxTracks = 256;

        /// <summary>
        /// The <see cref="OpenTK.Audio"/> audio context
        /// </summary>
        private AudioContext _context;

        /// <summary>
        /// An object pool containing <see cref="OpenALAudioTrack"/> objects
        /// </summary>
        private ConcurrentDictionary<int, OpenALAudioTrack> _tracks;

        /// <summary>
        /// True if the thread need to keep polling
        /// </summary>
        private bool _keepPolling;

        /// <summary>
        /// The poller thread audio context
        /// </summary>
        private Thread _audioPollerThread;

        /// <summary>
        /// True if OpenAL is supported on the device
        /// </summary>
        public static bool IsSupported
        {
            get
            {
                try
                {
                    return AudioContext.AvailableDevices.Count > 0;
                }
                catch
                {
                    return false;
                }
            }
        }

        public OpenALAudioOut()
        {
            _context           = new AudioContext();
            _tracks            = new ConcurrentDictionary<int, OpenALAudioTrack>();
            _keepPolling       = true;
            _audioPollerThread = new Thread(AudioPollerWork)
            {
                Name = "Audio.PollerThread"
            };

            _audioPollerThread.Start();
        }

        private void AudioPollerWork()
        {
            do
            {
                foreach (OpenALAudioTrack track in _tracks.Values)
                {
                    lock (track)
                    {
                        track.CallReleaseCallbackIfNeeded();
                    }
                }

                // If it's not slept it will waste cycles.
                Thread.Sleep(10);
            }
            while (_keepPolling);

            foreach (OpenALAudioTrack track in _tracks.Values)
            {
                track.Dispose();
            }

            _tracks.Clear();
            _context.Dispose();
        }

        public bool SupportsChannelCount(int channels)
        {
            // NOTE: OpenAL doesn't give us a way to know if the 5.1 setup is supported by hardware or actually emulated.
            // TODO: find a way to determine hardware support.
            return channels == 1 || channels == 2;
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
            OpenALAudioTrack track = new OpenALAudioTrack(sampleRate, GetALFormat(hardwareChannels), hardwareChannels, virtualChannels, callback);

            for (int id = 0; id < MaxTracks; id++)
            {
                if (_tracks.TryAdd(id, track))
                {
                    return id;
                }
            }

            return -1;
        }

        private ALFormat GetALFormat(int channels)
        {
            switch (channels)
            {
                case 1: return ALFormat.Mono16;
                case 2: return ALFormat.Stereo16;
                case 6: return ALFormat.Multi51Chn16Ext;
            }

            throw new ArgumentOutOfRangeException(nameof(channels));
        }

        /// <summary>
        /// Stops playback and closes the track specified by <paramref name="trackId"/>
        /// </summary>
        /// <param name="trackId">The ID of the track to close</param>
        public void CloseTrack(int trackId)
        {
            if (_tracks.TryRemove(trackId, out OpenALAudioTrack track))
            {
                lock (track)
                {
                    track.Dispose();
                }
            }
        }

        /// <summary>
        /// Returns a value indicating whether the specified buffer is currently reserved by the specified track
        /// </summary>
        /// <param name="trackId">The track to check</param>
        /// <param name="bufferTag">The buffer tag to check</param>
        public bool ContainsBuffer(int trackId, long bufferTag)
        {
            if (_tracks.TryGetValue(trackId, out OpenALAudioTrack track))
            {
                lock (track)
                {
                    return track.ContainsBuffer(bufferTag);
                }
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
            if (_tracks.TryGetValue(trackId, out OpenALAudioTrack track))
            {
                lock (track)
                {
                    return track.GetReleasedBuffers(maxCount);
                }
            }

            return null;
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
            if (_tracks.TryGetValue(trackId, out OpenALAudioTrack track))
            {
                lock (track)
                {
                    int bufferId = track.AppendBuffer(bufferTag);

                    // Do we need to downmix?
                    if (track.HardwareChannels != track.VirtualChannels)
                    {
                        short[] downmixedBuffer;

                        ReadOnlySpan<short> bufferPCM16 = MemoryMarshal.Cast<T, short>(buffer);

                        if (track.VirtualChannels == 6)
                        {
                            downmixedBuffer = Downmixing.DownMixSurroundToStereo(bufferPCM16);

                            if (track.HardwareChannels == 1)
                            {
                                downmixedBuffer = Downmixing.DownMixStereoToMono(downmixedBuffer);
                            }
                        }
                        else if (track.VirtualChannels == 2)
                        {
                            downmixedBuffer = Downmixing.DownMixStereoToMono(bufferPCM16);
                        }
                        else
                        {
                            throw new NotImplementedException($"Downmixing from {track.VirtualChannels} to {track.HardwareChannels} not implemented!");
                        }

                        AL.BufferData(bufferId, track.Format, downmixedBuffer, downmixedBuffer.Length * sizeof(ushort), track.SampleRate);
                    }
                    else
                    {
                        AL.BufferData(bufferId, track.Format, buffer, buffer.Length * sizeof(ushort), track.SampleRate);
                    }

                    AL.SourceQueueBuffer(track.SourceId, bufferId);

                    StartPlaybackIfNeeded(track);

                    track.PlayedSampleCount += (ulong)buffer.Length;
                }
            }
        }

        /// <summary>
        /// Starts playback
        /// </summary>
        /// <param name="trackId">The ID of the track to start playback on</param>
        public void Start(int trackId)
        {
            if (_tracks.TryGetValue(trackId, out OpenALAudioTrack track))
            {
                lock (track)
                {
                    track.State = PlaybackState.Playing;

                    StartPlaybackIfNeeded(track);
                }
            }
        }

        private void StartPlaybackIfNeeded(OpenALAudioTrack track)
        {
            AL.GetSource(track.SourceId, ALGetSourcei.SourceState, out int stateInt);

            ALSourceState State = (ALSourceState)stateInt;

            if (State != ALSourceState.Playing && track.State == PlaybackState.Playing)
            {
                AL.SourcePlay(track.SourceId);
            }
        }

        /// <summary>
        /// Stops playback
        /// </summary>
        /// <param name="trackId">The ID of the track to stop playback on</param>
        public void Stop(int trackId)
        {
            if (_tracks.TryGetValue(trackId, out OpenALAudioTrack track))
            {
                lock (track)
                {
                    track.State = PlaybackState.Stopped;

                    AL.SourceStop(track.SourceId);
                }
            }
        }

        /// <summary>
        /// Get track buffer count
        /// </summary>
        /// <param name="trackId">The ID of the track to get buffer count</param>
        public uint GetBufferCount(int trackId)
        {
            if (_tracks.TryGetValue(trackId, out OpenALAudioTrack track))
            {
                lock (track)
                {
                    return track.BufferCount;
                }
            }

            return 0;
        }

        /// <summary>
        /// Get track played sample count
        /// </summary>
        /// <param name="trackId">The ID of the track to get played sample count</param>
        public ulong GetPlayedSampleCount(int trackId)
        {
            if (_tracks.TryGetValue(trackId, out OpenALAudioTrack track))
            {
                lock (track)
                {
                    return track.PlayedSampleCount;
                }
            }

            return 0;
        }

        /// <summary>
        /// Flush all track buffers
        /// </summary>
        /// <param name="trackId">The ID of the track to flush</param>
        public bool FlushBuffers(int trackId)
        {
            if (_tracks.TryGetValue(trackId, out OpenALAudioTrack track))
            {
                lock (track)
                {
                    track.FlushBuffers();
                }
            }

            return false;
        }

        /// <summary>
        /// Set track volume
        /// </summary>
        /// <param name="trackId">The ID of the track to set volume</param>
        /// <param name="volume">The volume of the track</param>
        public void SetVolume(int trackId, float volume)
        {
            if (_tracks.TryGetValue(trackId, out OpenALAudioTrack track))
            {
                lock (track)
                {
                    track.SetVolume(volume);
                }
            }
        }

        /// <summary>
        /// Get track volume
        /// </summary>
        /// <param name="trackId">The ID of the track to get volume</param>
        public float GetVolume(int trackId)
        {
            if (_tracks.TryGetValue(trackId, out OpenALAudioTrack track))
            {
                lock (track)
                {
                    return track.GetVolume();
                }
            }

            return 1.0f;
        }

        /// <summary>
        /// Gets the current playback state of the specified track
        /// </summary>
        /// <param name="trackId">The track to retrieve the playback state for</param>
        public PlaybackState GetState(int trackId)
        {
            if (_tracks.TryGetValue(trackId, out OpenALAudioTrack track))
            {
                return track.State;
            }

            return PlaybackState.Stopped;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _keepPolling = false;
            }
        }
    }
}