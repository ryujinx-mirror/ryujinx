using OpenTK.Graphics.OpenGL;
using System;

namespace Ryujinx.Graphics.Gal.OpenGL
{
    class OglConstBuffer : IGalConstBuffer
    {
        private const long MaxConstBufferCacheSize = 64 * 1024 * 1024;

        private OglCachedResource<OglStreamBuffer> _cache;

        public OglConstBuffer()
        {
            _cache = new OglCachedResource<OglStreamBuffer>(DeleteBuffer, MaxConstBufferCacheSize);
        }

        public void LockCache()
        {
            _cache.Lock();
        }

        public void UnlockCache()
        {
            _cache.Unlock();
        }

        public void Create(long key, long size)
        {
            OglStreamBuffer buffer = new OglStreamBuffer(BufferTarget.UniformBuffer, size);

            _cache.AddOrUpdate(key, buffer, size);
        }

        public bool IsCached(long key, long size)
        {
            return _cache.TryGetSize(key, out long cachedSize) && cachedSize == size;
        }

        public void SetData(long key, long size, IntPtr hostAddress)
        {
            if (_cache.TryGetValue(key, out OglStreamBuffer buffer))
            {
                buffer.SetData(size, hostAddress);
            }
        }

        public void SetData(long key, byte[] data)
        {
            if (_cache.TryGetValue(key, out OglStreamBuffer buffer))
            {
                buffer.SetData(data);
            }
        }

        public bool TryGetUbo(long key, out int uboHandle)
        {
            if (_cache.TryGetValue(key, out OglStreamBuffer buffer))
            {
                uboHandle = buffer.Handle;

                return true;
            }

            uboHandle = 0;

            return false;
        }

        private static void DeleteBuffer(OglStreamBuffer buffer)
        {
            buffer.Dispose();
        }
    }
}