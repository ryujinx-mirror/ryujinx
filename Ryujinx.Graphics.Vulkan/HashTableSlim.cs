using System;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Vulkan
{
    interface IRefEquatable<T>
    {
        bool Equals(ref T other);
    }

    class HashTableSlim<K, V> where K : IRefEquatable<K>
    {
        private const int TotalBuckets = 16; // Must be power of 2
        private const int TotalBucketsMask = TotalBuckets - 1;

        private struct Entry
        {
            public K Key;
            public V Value;
        }

        private readonly Entry[][] _hashTable = new Entry[TotalBuckets][];

        public IEnumerable<K> Keys
        {
            get
            {
                foreach (Entry[] bucket in _hashTable)
                {
                    if (bucket != null)
                    {
                        foreach (Entry entry in bucket)
                        {
                            yield return entry.Key;
                        }
                    }
                }
            }
        }

        public IEnumerable<V> Values
        {
            get
            {
                foreach (Entry[] bucket in _hashTable)
                {
                    if (bucket != null)
                    {
                        foreach (Entry entry in bucket)
                        {
                            yield return entry.Value;
                        }
                    }
                }
            }
        }

        public void Add(ref K key, V value)
        {
            var entry = new Entry()
            {
                Key = key,
                Value = value
            };

            int hashCode = key.GetHashCode();
            int bucketIndex = hashCode & TotalBucketsMask;

            var bucket = _hashTable[bucketIndex];
            if (bucket != null)
            {
                int index = bucket.Length;

                Array.Resize(ref _hashTable[bucketIndex], index + 1);

                _hashTable[bucketIndex][index] = entry;
            }
            else
            {
                _hashTable[bucketIndex] = new Entry[]
                {
                    entry
                };
            }
        }

        public bool TryGetValue(ref K key, out V value)
        {
            int hashCode = key.GetHashCode();

            var bucket = _hashTable[hashCode & TotalBucketsMask];
            if (bucket != null)
            {

                for (int i = 0; i < bucket.Length; i++)
                {
                    ref var entry = ref bucket[i];

                    if (entry.Key.Equals(ref key))
                    {
                        value = entry.Value;
                        return true;
                    }
                }
            }

            value = default;
            return false;
        }
    }
}
