using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace ChocolArm64
{
    class TranslatorCache
    {
        //Maximum size of the cache, the unit used is completely arbitrary.
        private const int MaxTotalSize = 0x800000;

        //Minimum time required in milliseconds for a method to be eligible for deletion.
        private const int MinTimeDelta = 2 * 60000;

        //Minimum number of calls required to update the timestamp.
        private const int MinCallCountForUpdate = 250;

        private class CacheBucket
        {
            public TranslatedSub Subroutine { get; private set; }

            public LinkedListNode<long> Node { get; private set; }

            public int CallCount { get; set; }

            public int Size { get; private set; }

            public long Timestamp { get; private set; }

            public CacheBucket(TranslatedSub subroutine, LinkedListNode<long> node, int size)
            {
                Subroutine = subroutine;
                Size       = size;

                UpdateNode(node);
            }

            public void UpdateNode(LinkedListNode<long> node)
            {
                Node = node;

                Timestamp = GetTimestamp();
            }
        }

        private ConcurrentDictionary<long, CacheBucket> _cache;

        private LinkedList<long> _sortedCache;

        private int _totalSize;

        public TranslatorCache()
        {
            _cache = new ConcurrentDictionary<long, CacheBucket>();

            _sortedCache = new LinkedList<long>();
        }

        public void AddOrUpdate(long position, TranslatedSub subroutine, int size)
        {
            ClearCacheIfNeeded();

            lock (_sortedCache)
            {
                _totalSize += size;

                LinkedListNode<long> node = _sortedCache.AddLast(position);

                CacheBucket newBucket = new CacheBucket(subroutine, node, size);

                _cache.AddOrUpdate(position, newBucket, (key, bucket) =>
                {
                    _totalSize -= bucket.Size;

                    _sortedCache.Remove(bucket.Node);

                    return newBucket;
                });
            }
        }

        public bool HasSubroutine(long position)
        {
            return _cache.ContainsKey(position);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetSubroutine(long position, out TranslatedSub subroutine)
        {
            if (_cache.TryGetValue(position, out CacheBucket bucket))
            {
                if (bucket.CallCount++ > MinCallCountForUpdate)
                {
                    if (Monitor.TryEnter(_sortedCache))
                    {
                        try
                        {
                            //The bucket value on the dictionary may have changed between the
                            //time we get the value from the dictionary, and we acquire the
                            //lock. So we need to ensure we are working with the latest value,
                            //we can do that by getting the value again, inside the lock.
                            if (_cache.TryGetValue(position, out CacheBucket latestBucket))
                            {
                                latestBucket.CallCount = 0;

                                _sortedCache.Remove(latestBucket.Node);

                                latestBucket.UpdateNode(_sortedCache.AddLast(position));
                            }
                        }
                        finally
                        {
                            Monitor.Exit(_sortedCache);
                        }
                    }
                }

                subroutine = bucket.Subroutine;

                return true;
            }

            subroutine = default(TranslatedSub);

            return false;
        }

        private void ClearCacheIfNeeded()
        {
            long timestamp = GetTimestamp();

            while (_totalSize > MaxTotalSize)
            {
                lock (_sortedCache)
                {
                    LinkedListNode<long> node = _sortedCache.First;

                    if (node == null)
                    {
                        break;
                    }

                    CacheBucket bucket = _cache[node.Value];

                    long timeDelta = timestamp - bucket.Timestamp;

                    if (timeDelta <= MinTimeDelta)
                    {
                        break;
                    }

                    if (_cache.TryRemove(node.Value, out bucket))
                    {
                        _totalSize -= bucket.Size;

                        _sortedCache.Remove(bucket.Node);
                    }
                }
            }
        }

        private static long GetTimestamp()
        {
            long timestamp = Stopwatch.GetTimestamp();

            return timestamp / (Stopwatch.Frequency / 1000);
        }
    }
}