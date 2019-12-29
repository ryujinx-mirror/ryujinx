using System.Collections;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Gpu.Image
{
    /// <summary>
    /// A texture cache that automatically removes older textures that are not used for some time.
    /// The cache works with a rotated list with a fixed size. When new textures are added, the
    /// old ones at the bottom of the list are deleted.
    /// </summary>
    class AutoDeleteCache : IEnumerable<Texture>
    {
        private const int MaxCapacity = 2048;

        private readonly LinkedList<Texture> _textures;

        /// <summary>
        /// Creates a new instance of the automatic deletion cache.
        /// </summary>
        public AutoDeleteCache()
        {
            _textures = new LinkedList<Texture>();
        }

        /// <summary>
        /// Adds a new texture to the cache, even if the texture added is already on the cache.
        /// Using this method is only recommended if you know that the texture is not yet on the cache,
        /// otherwise it would store the same texture more than once.
        /// </summary>
        /// <param name="texture">The texture to be added to the cache</param>
        public void Add(Texture texture)
        {
            texture.IncrementReferenceCount();

            texture.CacheNode = _textures.AddLast(texture);

            if (_textures.Count > MaxCapacity)
            {
                Texture oldestTexture = _textures.First.Value;

                _textures.RemoveFirst();

                oldestTexture.DecrementReferenceCount();

                oldestTexture.CacheNode = null;
            }
        }

        /// <summary>
        /// Adds a new texture to the cache, or just moves it to the top of the list if the
        /// texture is already on the cache. Moving the texture to the top of the list prevents
        /// it from being deleted, as the textures on the bottom of the list are deleted when new ones are added.
        /// </summary>
        /// <param name="texture">The texture to be added, or moved to the top</param>
        public void Lift(Texture texture)
        {
            if (texture.CacheNode != null)
            {
                if (texture.CacheNode != _textures.Last)
                {
                    _textures.Remove(texture.CacheNode);

                    texture.CacheNode = _textures.AddLast(texture);
                }
            }
            else
            {
                Add(texture);
            }
        }

        public IEnumerator<Texture> GetEnumerator()
        {
            return _textures.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _textures.GetEnumerator();
        }
    }
}