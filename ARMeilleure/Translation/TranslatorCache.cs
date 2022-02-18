using System;
using System.Collections.Generic;
using System.Threading;

namespace ARMeilleure.Translation
{
    internal class TranslatorCache<T>
    {
        private readonly IntervalTree<ulong, T> _tree;
        private readonly ReaderWriterLock _treeLock;

        public int Count => _tree.Count;

        public TranslatorCache()
        {
            _tree = new IntervalTree<ulong, T>();
            _treeLock = new ReaderWriterLock();
        }

        public bool TryAdd(ulong address, ulong size, T value)
        {
            return AddOrUpdate(address, size, value, null);
        }

        public bool AddOrUpdate(ulong address, ulong size, T value, Func<ulong, T, T> updateFactoryCallback)
        {
            _treeLock.AcquireWriterLock(Timeout.Infinite);
            bool result = _tree.AddOrUpdate(address, address + size, value, updateFactoryCallback);
            _treeLock.ReleaseWriterLock();

            return result;
        }

        public T GetOrAdd(ulong address, ulong size, T value)
        {
            _treeLock.AcquireWriterLock(Timeout.Infinite);
            value = _tree.GetOrAdd(address, address + size, value);
            _treeLock.ReleaseWriterLock();

            return value;
        }

        public bool Remove(ulong address)
        {
            _treeLock.AcquireWriterLock(Timeout.Infinite);
            bool removed = _tree.Remove(address) != 0;
            _treeLock.ReleaseWriterLock();

            return removed;
        }

        public void Clear()
        {
            _treeLock.AcquireWriterLock(Timeout.Infinite);
            _tree.Clear();
            _treeLock.ReleaseWriterLock();
        }

        public bool ContainsKey(ulong address)
        {
            _treeLock.AcquireReaderLock(Timeout.Infinite);
            bool result = _tree.ContainsKey(address);
            _treeLock.ReleaseReaderLock();

            return result;
        }

        public bool TryGetValue(ulong address, out T value)
        {
            _treeLock.AcquireReaderLock(Timeout.Infinite);
            bool result = _tree.TryGet(address, out value);
            _treeLock.ReleaseReaderLock();

            return result;
        }

        public int GetOverlaps(ulong address, ulong size, ref ulong[] overlaps)
        {
            _treeLock.AcquireReaderLock(Timeout.Infinite);
            int count = _tree.Get(address, address + size, ref overlaps);
            _treeLock.ReleaseReaderLock();

            return count;
        }

        public List<T> AsList()
        {
            _treeLock.AcquireReaderLock(Timeout.Infinite);
            List<T> list = _tree.AsList();
            _treeLock.ReleaseReaderLock();

            return list;
        }
    }
}
