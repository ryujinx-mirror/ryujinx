using Ryujinx.Common;
using System;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Gal.OpenGL
{
    class OGLCachedResource<T>
    {
        public delegate void DeleteValue(T Value);

        private const int MinTimeDelta      = 5 * 60000;
        private const int MaxRemovalsPerRun = 10;

        private struct CacheBucket
        {
            public T Value { get; private set; }

            public LinkedListNode<long> Node { get; private set; }

            public long DataSize { get; private set; }

            public long Timestamp { get; private set; }

            public CacheBucket(T Value, long DataSize, LinkedListNode<long> Node)
            {
                this.Value    = Value;
                this.DataSize = DataSize;
                this.Node     = Node;

                Timestamp = PerformanceCounter.ElapsedMilliseconds;
            }
        }

        private Dictionary<long, CacheBucket> Cache;

        private LinkedList<long> SortedCache;

        private DeleteValue DeleteValueCallback;

        private Queue<T> DeletePending;

        private bool Locked;

        private long MaxSize;
        private long TotalSize;

        public OGLCachedResource(DeleteValue DeleteValueCallback, long MaxSize)
        {
            this.MaxSize = MaxSize;

            if (DeleteValueCallback == null)
            {
                throw new ArgumentNullException(nameof(DeleteValueCallback));
            }

            this.DeleteValueCallback = DeleteValueCallback;

            Cache = new Dictionary<long, CacheBucket>();

            SortedCache = new LinkedList<long>();

            DeletePending = new Queue<T>();
        }

        public void Lock()
        {
            Locked = true;
        }

        public void Unlock()
        {
            Locked = false;

            while (DeletePending.TryDequeue(out T Value))
            {
                DeleteValueCallback(Value);
            }

            ClearCacheIfNeeded();
        }

        public void AddOrUpdate(long Key, T Value, long Size)
        {
            if (!Locked)
            {
                ClearCacheIfNeeded();
            }

            LinkedListNode<long> Node = SortedCache.AddLast(Key);

            CacheBucket NewBucket = new CacheBucket(Value, Size, Node);

            if (Cache.TryGetValue(Key, out CacheBucket Bucket))
            {
                if (Locked)
                {
                    DeletePending.Enqueue(Bucket.Value);
                }
                else
                {
                    DeleteValueCallback(Bucket.Value);
                }

                SortedCache.Remove(Bucket.Node);

                TotalSize -= Bucket.DataSize;

                Cache[Key] = NewBucket;
            }
            else
            {
                Cache.Add(Key, NewBucket);
            }

            TotalSize += Size;
        }

        public bool TryGetValue(long Key, out T Value)
        {
            if (Cache.TryGetValue(Key, out CacheBucket Bucket))
            {
                Value = Bucket.Value;

                SortedCache.Remove(Bucket.Node);

                LinkedListNode<long> Node = SortedCache.AddLast(Key);

                Cache[Key] = new CacheBucket(Value, Bucket.DataSize, Node);

                return true;
            }

            Value = default(T);

            return false;
        }

        public bool TryGetSize(long Key, out long Size)
        {
            if (Cache.TryGetValue(Key, out CacheBucket Bucket))
            {
                Size = Bucket.DataSize;

                return true;
            }

            Size = 0;

            return false;
        }

        private void ClearCacheIfNeeded()
        {
            long Timestamp = PerformanceCounter.ElapsedMilliseconds;

            int Count = 0;

            while (Count++ < MaxRemovalsPerRun)
            {
                LinkedListNode<long> Node = SortedCache.First;

                if (Node == null)
                {
                    break;
                }

                CacheBucket Bucket = Cache[Node.Value];

                long TimeDelta = Timestamp - Bucket.Timestamp;

                if (TimeDelta <= MinTimeDelta && !UnderMemoryPressure())
                {
                    break;
                }

                SortedCache.Remove(Node);

                Cache.Remove(Node.Value);

                DeleteValueCallback(Bucket.Value);

                TotalSize -= Bucket.DataSize;
            }
        }

        private bool UnderMemoryPressure()
        {
            return TotalSize >= MaxSize;
        }
    }
}