using System.Collections.Generic;

namespace Ryujinx.Graphics.Gpu.Image
{
    class TexturePoolCache
    {
        private const int MaxCapacity = 4;

        private GpuContext     _context;
        private TextureManager _textureManager;

        private LinkedList<TexturePool> _pools;

        public TexturePoolCache(GpuContext context, TextureManager textureManager)
        {
            _context        = context;
            _textureManager = textureManager;

            _pools = new LinkedList<TexturePool>();
        }

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
            pool = new TexturePool(_context, _textureManager, address, maximumId);

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

        public void InvalidateRange(ulong address, ulong size)
        {
            for (LinkedListNode<TexturePool> node = _pools.First; node != null; node = node.Next)
            {
                TexturePool pool = node.Value;

                pool.InvalidateRange(address, size);
            }
        }
    }
}