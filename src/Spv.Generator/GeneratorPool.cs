using System.Collections.Generic;

namespace Spv.Generator
{
    public class GeneratorPool<T> where T : class, new()
    {
        private readonly List<T[]> _pool;
        private int _chunkIndex = -1;
        private int _poolIndex = -1;
        private readonly int _poolSizeIncrement;

        public GeneratorPool() : this(1000, 200) { }

        public GeneratorPool(int chunkSizeLimit, int poolSizeIncrement)
        {
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
    }
}
