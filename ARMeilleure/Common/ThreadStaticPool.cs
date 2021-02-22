using ARMeilleure.Translation.PTC;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ARMeilleure.Common
{
    class ThreadStaticPool<T> where T : class, new()
    {
        [ThreadStatic]
        private static ThreadStaticPool<T> _instance;

        public static ThreadStaticPool<T> Instance
        {
            get
            {
                if (_instance == null)
                {
                    PreparePool(); // So that we can still use a pool when blindly initializing one.
                }

                return _instance;
            }
        }

        private static readonly ConcurrentDictionary<int, Stack<ThreadStaticPool<T>>> _pools = new();

        private static Stack<ThreadStaticPool<T>> GetPools(int groupId)
        {
            return _pools.GetOrAdd(groupId, (groupId) => new());
        }

        public static void PreparePool(
            int groupId = 0,
            ChunkSizeLimit chunkSizeLimit = ChunkSizeLimit.Large,
            PoolSizeIncrement poolSizeIncrement = PoolSizeIncrement.Default)
        {
            if (Ptc.State == PtcState.Disabled)
            {
                PreparePoolDefault(groupId, (int)chunkSizeLimit, (int)poolSizeIncrement);
            }
            else
            {
                PreparePoolSlim((int)chunkSizeLimit, (int)poolSizeIncrement);
            }
        }

        private static void PreparePoolDefault(int groupId, int chunkSizeLimit, int poolSizeIncrement)
        {
            // Prepare the pool for this thread, ideally using an existing one from the specified group.

            if (_instance == null)
            {
                var pools = GetPools(groupId);
                lock (pools)
                {
                    _instance = (pools.Count != 0) ? pools.Pop() : new(chunkSizeLimit, poolSizeIncrement);
                }
            }
        }

        private static void PreparePoolSlim(int chunkSizeLimit, int poolSizeIncrement)
        {
            // Prepare the pool for this thread.

            if (_instance == null)
            {
                _instance = new(chunkSizeLimit, poolSizeIncrement);
            }
        }

        public static void ResetPool(int groupId = 0)
        {
            if (Ptc.State == PtcState.Disabled)
            {
                ResetPoolDefault(groupId);
            }
            else
            {
                ResetPoolSlim();
            }
        }

        private static void ResetPoolDefault(int groupId)
        {
            // Reset, limit if necessary, and return the pool for this thread to the specified group.

            if (_instance != null)
            {
                var pools = GetPools(groupId);
                lock (pools)
                {
                    _instance.Clear();
                    _instance.ChunkSizeLimiter();
                    pools.Push(_instance);

                    _instance = null;
                }
            }
        }

        private static void ResetPoolSlim()
        {
            // Reset, limit if necessary, the pool for this thread.

            if (_instance != null)
            {
                _instance.Clear();
                _instance.ChunkSizeLimiter();
            }
        }

        public static void DisposePools()
        {
            if (Ptc.State == PtcState.Disabled)
            {
                DisposePoolsDefault();
            }
            else
            {
                DisposePoolSlim();
            }
        }

        private static void DisposePoolsDefault()
        {
            // Resets any static references to the pools used by threads for each group, allowing them to be garbage collected.

            foreach (var pools in _pools.Values)
            {
                foreach (var instance in pools)
                {
                    instance.Dispose();
                }

                pools.Clear();
            }

            _pools.Clear();
        }

        private static void DisposePoolSlim()
        {
            // Dispose the pool for this thread.

            if (_instance != null)
            {
                _instance.Dispose();

                _instance = null;
            }
        }

        private List<T[]> _pool;
        private int _chunkIndex = -1;
        private int _poolIndex = -1;
        private int _chunkSizeLimit;
        private int _poolSizeIncrement;

        private ThreadStaticPool(int chunkSizeLimit, int poolSizeIncrement)
        {
            _chunkSizeLimit = chunkSizeLimit;
            _poolSizeIncrement = poolSizeIncrement;

            _pool = new(chunkSizeLimit * 2);

            AddChunkIfNeeded();
        }

        public T Allocate()
        {
            if (++_poolIndex >= _poolSizeIncrement)
            {
                AddChunkIfNeeded();

                _poolIndex = 0;
            }

            return _pool[_chunkIndex][_poolIndex];
        }

        private void AddChunkIfNeeded()
        {
            if (++_chunkIndex >= _pool.Count)
            {
                T[] pool = new T[_poolSizeIncrement];

                for (int i = 0; i < _poolSizeIncrement; i++)
                {
                    pool[i] = new T();
                }

                _pool.Add(pool);
            }
        }

        public void Clear()
        {
            _chunkIndex = 0;
            _poolIndex = -1;
        }

        private void ChunkSizeLimiter()
        {
            if (_pool.Count >= _chunkSizeLimit)
            {
                int newChunkSize = _chunkSizeLimit / 2;

                _pool.RemoveRange(newChunkSize, _pool.Count - newChunkSize);
                _pool.Capacity = _chunkSizeLimit * 2;
            }
        }

        private void Dispose()
        {
            _pool = null;
        }
    }
}
