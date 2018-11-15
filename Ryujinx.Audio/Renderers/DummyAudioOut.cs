using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Ryujinx.Audio
{
    /// <summary>
    /// A Dummy audio renderer that does not output any audio
    /// </summary>
    public class DummyAudioOut : IAalOutput
    {
        private ConcurrentQueue<long> m_Buffers;

        public DummyAudioOut()
        {
            m_Buffers = new ConcurrentQueue<long>();
        }

        /// <summary>
        /// Dummy audio output is always available, Baka!
        /// </summary>
        public static bool IsSupported => true;

        public PlaybackState GetState(int trackId) => PlaybackState.Stopped;

        public int OpenTrack(int sampleRate, int channels, ReleaseCallback callback) => 1;

        public void CloseTrack(int trackId) { }

        public void Start(int trackId) { }

        public void Stop(int trackId) { }

        public void AppendBuffer<T>(int trackID, long bufferTag, T[] buffer)
            where T : struct
        {
            m_Buffers.Enqueue(bufferTag);
        }

        public long[] GetReleasedBuffers(int trackId, int maxCount)
        {
            List<long> bufferTags = new List<long>();

            for (int i = 0; i < maxCount; i++)
            {
                if (!m_Buffers.TryDequeue(out long tag))
                {
                    break;
                }

                bufferTags.Add(tag);
            }

            return bufferTags.ToArray();
        }

        public bool ContainsBuffer(int trackID, long bufferTag) => false;

        public void Dispose()
        {
            m_Buffers.Clear();
        }
    }
}
