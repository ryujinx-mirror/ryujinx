using System;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Gal.OpenGL
{
    class OGLCachedResource<T>
    {
        public delegate void DeleteValue(T Value);

        private const int MaxTimeDelta      = 5 * 60000;
        private const int MaxRemovalsPerRun = 10;

        private struct CacheBucket
        {
            public T Value { get; private set; }

            public LinkedListNode<long> Node { get; private set; }

            public long DataSize { get; private set; }

            public int Timestamp { get; private set; }

            public CacheBucket(T Value, long DataSize, LinkedListNode<long> Node)
            {
                this.Value     = Value;
                this.DataSize  = DataSize;
                this.Node      = Node;

                Timestamp = Environment.TickCount;
            }
        }

        private Dictionary<long, CacheBucket> Cache;

        private LinkedList<long> SortedCache;

        private DeleteValue DeleteValueCallback;

        public OGLCachedResource(DeleteValue DeleteValueCallback)
        {
            if (DeleteValueCallback == null)
            {
                throw new ArgumentNullException(nameof(DeleteValueCallback));
            }

            this.DeleteValueCallback = DeleteValueCallback;

            Cache = new Dictionary<long, CacheBucket>();

            SortedCache = new LinkedList<long>();
        }

        public void AddOrUpdate(long Key, T Value, long Size)
        {
            ClearCacheIfNeeded();

            LinkedListNode<long> Node = SortedCache.AddLast(Key);

            CacheBucket NewBucket = new CacheBucket(Value, Size, Node);

            if (Cache.TryGetValue(Key, out CacheBucket Bucket))
            {
                DeleteValueCallback(Bucket.Value);

                SortedCache.Remove(Bucket.Node);

                Cache[Key] = NewBucket;
            }
            else
            {
                Cache.Add(Key, NewBucket);
            }
        }

        public bool TryGetValue(long Key, out T Value)
        {
            if (Cache.TryGetValue(Key, out CacheBucket Bucket))
            {
                Value = Bucket.Value;

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
            int Timestamp = Environment.TickCount;

            int Count = 0;

            while (Count++ < MaxRemovalsPerRun)
            {
                LinkedListNode<long> Node = SortedCache.First;

                if (Node == null)
                {
                    break;
                }

                CacheBucket Bucket = Cache[Node.Value];

                int TimeDelta = RingDelta(Bucket.Timestamp, Timestamp);

                if ((uint)TimeDelta <= (uint)MaxTimeDelta)
                {
                    break;
                }

                SortedCache.Remove(Node);

                Cache.Remove(Node.Value);

                DeleteValueCallback(Bucket.Value);
            }
        }

        private int RingDelta(int Old, int New)
        {
            if ((uint)New < (uint)Old)
            {
                return New + (~Old + 1);
            }
            else
            {
                return New - Old;
            }
        }
    }
}