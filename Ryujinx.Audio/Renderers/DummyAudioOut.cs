using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Ryujinx.Audio
{
    /// <summary>
    /// A Dummy audio renderer that does not output any audio
    /// </summary>
    public class DummyAudioOut : IAalOutput
    {
        private int   _lastTrackId = 1;
        private float _volume      = 1.0f;

        private ConcurrentQueue<int> _trackIds;
        private ConcurrentQueue<long> _buffers;
        private ConcurrentDictionary<int, ReleaseCallback> _releaseCallbacks;
        private ulong _playedSampleCount;

        public DummyAudioOut()
        {
            _buffers          = new ConcurrentQueue<long>();
            _trackIds         = new ConcurrentQueue<int>();
            _releaseCallbacks = new ConcurrentDictionary<int, ReleaseCallback>();
        }

        /// <summary>
        /// Dummy audio output is always available, Baka!
        /// </summary>
        public static bool IsSupported => true;

        public PlaybackState GetState(int trackId) => PlaybackState.Stopped;

        public bool SupportsChannelCount(int channels)
        {
            return true;
        }

        public int OpenHardwareTrack(int sampleRate, int hardwareChannels, int virtualChannels, ReleaseCallback callback)
        {
            if (!_trackIds.TryDequeue(out int trackId))
            {
                trackId = ++_lastTrackId;
            }

            _releaseCallbacks[trackId] = callback;

            return trackId;
        }

        public void CloseTrack(int trackId)
        {
            _trackIds.Enqueue(trackId);
            _releaseCallbacks.Remove(trackId, out _);
        }

        public bool ContainsBuffer(int trackID, long bufferTag) => false;

        public long[] GetReleasedBuffers(int trackId, int maxCount)
        {
            List<long> bufferTags = new List<long>();

            for (int i = 0; i < maxCount; i++)
            {
                if (!_buffers.TryDequeue(out long tag))
                {
                    break;
                }

                bufferTags.Add(tag);
            }

            return bufferTags.ToArray();
        }

        public void AppendBuffer<T>(int trackId, long bufferTag, T[] buffer) where T : struct
        {
            _buffers.Enqueue(bufferTag);

            _playedSampleCount += (ulong)buffer.Length;

            if (_releaseCallbacks.TryGetValue(trackId, out var callback))
            {
                callback?.Invoke();
            }
        }

        public void Start(int trackId) { }

        public void Stop(int trackId) { }

        public uint GetBufferCount(int trackId) => (uint)_buffers.Count;

        public ulong GetPlayedSampleCount(int trackId) => _playedSampleCount;

        public bool FlushBuffers(int trackId) => false;

        public float GetVolume(int trackId) => _volume;

        public void SetVolume(int trackId, float volume)
        {
            _volume = volume;
        }

        public void Dispose()
        {
            _buffers.Clear();
        }
    }
}