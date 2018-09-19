using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace ChocolArm64
{
    class ATranslatorCache
    {
        private const int MaxTotalSize          = 2 * 1024 * 256;
        private const int MaxTimeDelta          = 30000;
        private const int MinCallCountForUpdate = 1000;

        private class CacheBucket
        {
            public ATranslatedSub Subroutine { get; private set; }

            public LinkedListNode<long> Node { get; private set; }

            public int CallCount { get; set; }

            public int Size { get; private set; }

            public int Timestamp { get; private set; }

            public CacheBucket(ATranslatedSub Subroutine, LinkedListNode<long> Node, int Size)
            {
                this.Subroutine = Subroutine;
                this.Size       = Size;

                UpdateNode(Node);
            }

            public void UpdateNode(LinkedListNode<long> Node)
            {
                this.Node = Node;

                Timestamp = Environment.TickCount;
            }
        }

        private ConcurrentDictionary<long, CacheBucket> Cache;

        private LinkedList<long> SortedCache;

        private int TotalSize;

        public ATranslatorCache()
        {
            Cache = new ConcurrentDictionary<long, CacheBucket>();

            SortedCache = new LinkedList<long>();
        }

        public void AddOrUpdate(long Position, ATranslatedSub Subroutine, int Size)
        {
            ClearCacheIfNeeded();

            TotalSize += Size;

            lock (SortedCache)
            {
                LinkedListNode<long> Node = SortedCache.AddLast(Position);

                CacheBucket NewBucket = new CacheBucket(Subroutine, Node, Size);

                Cache.AddOrUpdate(Position, NewBucket, (Key, Bucket) =>
                {
                    TotalSize -= Bucket.Size;

                    SortedCache.Remove(Bucket.Node);

                    return NewBucket;
                });
            }
        }

        public bool HasSubroutine(long Position)
        {
            return Cache.ContainsKey(Position);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetSubroutine(long Position, out ATranslatedSub Subroutine)
        {
            if (Cache.TryGetValue(Position, out CacheBucket Bucket))
            {
                if (Bucket.CallCount++ > MinCallCountForUpdate)
                {
                    if (Monitor.TryEnter(SortedCache))
                    {
                        try
                        {
                            Bucket.CallCount = 0;

                            SortedCache.Remove(Bucket.Node);

                            Bucket.UpdateNode(SortedCache.AddLast(Position));
                        }
                        finally
                        {
                            Monitor.Exit(SortedCache);
                        }
                    }
                }

                Subroutine = Bucket.Subroutine;

                return true;
            }

            Subroutine = default(ATranslatedSub);

            return false;
        }

        private void ClearCacheIfNeeded()
        {
            int Timestamp = Environment.TickCount;

            while (TotalSize > MaxTotalSize)
            {
                lock (SortedCache)
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

                    if (Cache.TryRemove(Node.Value, out Bucket))
                    {
                        TotalSize -= Bucket.Size;

                        SortedCache.Remove(Bucket.Node);
                    }
                }
            }
        }

        private static int RingDelta(int Old, int New)
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