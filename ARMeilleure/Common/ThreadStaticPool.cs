using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace ARMeilleure.Common
{
    class ThreadStaticPool<T> where T : class, new()
    {
        private const int PoolSizeIncrement = 200;

        [ThreadStatic]
        private static ThreadStaticPool<T> _instance;

        public static ThreadStaticPool<T> Instance
        {
            get
            {
                if (_instance == null)
                {
                    PreparePool(0); // So that we can still use a pool when blindly initializing one.
                }

                return _instance;
            }
        }

        private static ConcurrentDictionary<int, Stack<ThreadStaticPool<T>>> _pools = new ConcurrentDictionary<int, Stack<ThreadStaticPool<T>>>();

        private static Stack<ThreadStaticPool<T>> GetPools(int groupId)
        {
            return _pools.GetOrAdd(groupId, x => new Stack<ThreadStaticPool<T>>());
        }

        public static void PreparePool(int groupId)
        {
            // Prepare the pool for this thread, ideally using an existing one from the specified group.

            if (_instance == null)
            {
                var pools = GetPools(groupId);
                lock (pools)
                {
                    _instance = (pools.Count != 0) ? pools.Pop() : new ThreadStaticPool<T>(PoolSizeIncrement * 2);
                }
            }
        }

        public static void ReturnPool(int groupId)
        {
            // Reset and return the pool for this thread to the specified group.

            var pools = GetPools(groupId);
            lock (pools)
            {
                _instance.Clear();
                pools.Push(_instance);

                _instance = null;
            }
        }

        public static void ResetPools()
        {
            // Resets any static references to the pools used by threads for each group, allowing them to be garbage collected.

            foreach (var pools in _pools.Values)
            {
                pools.Clear();
            }

            _pools.Clear();
        }

        private T[] _pool;
        private int _poolUsed = -1;
        private int _poolSize;

        public ThreadStaticPool(int initialSize)
        {
            _pool = new T[initialSize];

            for (int i = 0; i < initialSize; i++)
            {
                _pool[i] = new T();
            }

            _poolSize = initialSize;
        }

        public T Allocate()
        {
            int index = Interlocked.Increment(ref _poolUsed);

            if (index >= _poolSize)
            {
                IncreaseSize();
            }

            return _pool[index];
        }

        private void IncreaseSize()
        {
            _poolSize += PoolSizeIncrement;

            T[] newArray = new T[_poolSize];
            Array.Copy(_pool, 0, newArray, 0, _pool.Length);

            for (int i = _pool.Length; i < _poolSize; i++)
            {
                newArray[i] = new T();
            }

            Interlocked.Exchange(ref _pool, newArray);
        }

        public void Clear()
        {
            _poolUsed = -1;
        }
    }
}
