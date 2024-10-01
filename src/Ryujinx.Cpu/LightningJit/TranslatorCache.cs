using ARMeilleure.Translation;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Ryujinx.Cpu.LightningJit
{
    internal class TranslatorCache<T>
    {
        private readonly IntervalTree<ulong, T> _tree;
        private readonly ReaderWriterLockSlim _treeLock;

        public int Count => _tree.Count;

        public TranslatorCache()
        {
            _tree = new IntervalTree<ulong, T>();
            _treeLock = new ReaderWriterLockSlim();
        }

        public bool TryAdd(ulong address, ulong size, T value)
        {
            return AddOrUpdate(address, size, value, null);
        }

        public bool AddOrUpdate(ulong address, ulong size, T value, Func<ulong, T, T> updateFactoryCallback)
        {
            _treeLock.EnterWriteLock();
            bool result = _tree.AddOrUpdate(address, address + size, value, updateFactoryCallback);
            _treeLock.ExitWriteLock();

            return result;
        }

        public T GetOrAdd(ulong address, ulong size, T value)
        {
            _treeLock.EnterWriteLock();
            value = _tree.GetOrAdd(address, address + size, value);
            _treeLock.ExitWriteLock();

            return value;
        }

        public bool Remove(ulong address)
        {
            _treeLock.EnterWriteLock();
            bool removed = _tree.Remove(address) != 0;
            _treeLock.ExitWriteLock();

            return removed;
        }

        public void Clear()
        {
            _treeLock.EnterWriteLock();
            _tree.Clear();
            _treeLock.ExitWriteLock();
        }

        public bool ContainsKey(ulong address)
        {
            _treeLock.EnterReadLock();
            bool result = _tree.ContainsKey(address);
            _treeLock.ExitReadLock();

            return result;
        }

        public bool TryGetValue(ulong address, out T value)
        {
            _treeLock.EnterReadLock();
            bool result = _tree.TryGet(address, out value);
            _treeLock.ExitReadLock();

            return result;
        }

        public int GetOverlaps(ulong address, ulong size, ref ulong[] overlaps)
        {
            _treeLock.EnterReadLock();
            int count = _tree.Get(address, address + size, ref overlaps);
            _treeLock.ExitReadLock();

            return count;
        }

        public List<T> AsList()
        {
            _treeLock.EnterReadLock();
            List<T> list = _tree.AsList();
            _treeLock.ExitReadLock();

            return list;
        }
    }
}
