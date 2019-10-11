using OpenTK.Audio.OpenAL;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Ryujinx.Audio
{
    internal class OpenALAudioTrack : IDisposable
    {
        public int           SourceId   { get; private set; }
        public int           SampleRate { get; private set; }
        public ALFormat      Format     { get; private set; }
        public PlaybackState State      { get; set; }

        private ReleaseCallback _callback;

        private ConcurrentDictionary<long, int> _buffers;

        private Queue<long> _queuedTagsQueue;
        private Queue<long> _releasedTagsQueue;

        private bool _disposed;

        public OpenALAudioTrack(int sampleRate, ALFormat format, ReleaseCallback callback)
        {
            SampleRate = sampleRate;
            Format     = format;
            State      = PlaybackState.Stopped;
            SourceId   = AL.GenSource();

            _callback = callback;

            _buffers = new ConcurrentDictionary<long, int>();

            _queuedTagsQueue   = new Queue<long>();
            _releasedTagsQueue = new Queue<long>();
        }

        public bool ContainsBuffer(long tag)
        {
            foreach (long queuedTag in _queuedTagsQueue)
            {
                if (queuedTag == tag)
                {
                    return true;
                }
            }

            return false;
        }

        public long[] GetReleasedBuffers(int count)
        {
            AL.GetSource(SourceId, ALGetSourcei.BuffersProcessed, out int releasedCount);

            releasedCount += _releasedTagsQueue.Count;

            if (count > releasedCount)
            {
                count = releasedCount;
            }

            List<long> tags = new List<long>();

            while (count-- > 0 && _releasedTagsQueue.TryDequeue(out long tag))
            {
                tags.Add(tag);
            }

            while (count-- > 0 && _queuedTagsQueue.TryDequeue(out long tag))
            {
                AL.SourceUnqueueBuffers(SourceId, 1);

                tags.Add(tag);
            }

            return tags.ToArray();
        }

        public int AppendBuffer(long tag)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }

            int id = AL.GenBuffer();

            _buffers.AddOrUpdate(tag, id, (key, oldId) =>
            {
                AL.DeleteBuffer(oldId);

                return id;
            });

            _queuedTagsQueue.Enqueue(tag);

            return id;
        }

        public void CallReleaseCallbackIfNeeded()
        {
            AL.GetSource(SourceId, ALGetSourcei.BuffersProcessed, out int releasedCount);

            if (releasedCount > 0)
            {
                // If we signal, then we also need to have released buffers available
                // to return when GetReleasedBuffers is called.
                // If playback needs to be re-started due to all buffers being processed,
                // then OpenAL zeros the counts (ReleasedCount), so we keep it on the queue.
                while (releasedCount-- > 0 && _queuedTagsQueue.TryDequeue(out long tag))
                {
                    AL.SourceUnqueueBuffers(SourceId, 1);

                    _releasedTagsQueue.Enqueue(tag);
                }

                _callback();
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                _disposed = true;

                AL.DeleteSource(SourceId);

                foreach (int id in _buffers.Values)
                {
                    AL.DeleteBuffer(id);
                }
            }
        }
    }
}