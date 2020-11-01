using System.Collections.Generic;

namespace Ryujinx.Graphics.Gpu.Image
{
    /// <summary>
    /// Texture pool cache.
    /// This can keep multiple texture pools, and return the current one as needed.
    /// It is useful for applications that uses multiple texture pools.
    /// </summary>
    class TexturePoolCache
    {
        private const int MaxCapacity = 4;

        private GpuContext _context;

        private LinkedList<TexturePool> _pools;

        /// <summary>
        /// Constructs a new instance of the texture pool.
        /// </summary>
        /// <param name="context">GPU context that the texture pool belongs to</param>
        public TexturePoolCache(GpuContext context)
        {
            _context = context;

            _pools = new LinkedList<TexturePool>();
        }

        /// <summary>
        /// Finds a cache texture pool, or creates a new one if not found.
        /// </summary>
        /// <param name="address">Start address of the texture pool</param>
        /// <param name="maximumId">Maximum ID of the texture pool</param>
        /// <returns>The found or newly created texture pool</returns>
        public TexturePool FindOrCreate(ulong address, int maximumId)
        {
            TexturePool pool;

            // First we try to find the pool.
            for (LinkedListNode<TexturePool> node = _pools.First; node != null; node = node.Next)
            {
                pool = node.Value;

                if (pool.Address == address)
                {
                    if (pool.CacheNode != _pools.Last)
                    {
                        _pools.Remove(pool.CacheNode);

                        pool.CacheNode = _pools.AddLast(pool);
                    }

                    return pool;
                }
            }

            // If not found, create a new one.
            pool = new TexturePool(_context, address, maximumId);

            pool.CacheNode = _pools.AddLast(pool);

            if (_pools.Count > MaxCapacity)
            {
                TexturePool oldestPool = _pools.First.Value;

                _pools.RemoveFirst();

                oldestPool.Dispose();

                oldestPool.CacheNode = null;
            }

            return pool;
        }
    }
}