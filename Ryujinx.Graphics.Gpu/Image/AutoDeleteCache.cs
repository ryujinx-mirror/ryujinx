using System.Collections;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Gpu.Image
{
    class AutoDeleteCache : IEnumerable<Texture>
    {
        private const int MaxCapacity = 2048;

        private LinkedList<Texture> _textures;

        public AutoDeleteCache()
        {
            _textures = new LinkedList<Texture>();
        }

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