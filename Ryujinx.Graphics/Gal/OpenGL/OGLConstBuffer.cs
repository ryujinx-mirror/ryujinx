using OpenTK.Graphics.OpenGL;
using System;

namespace Ryujinx.Graphics.Gal.OpenGL
{
    class OGLConstBuffer : IGalConstBuffer
    {
        private const long MaxConstBufferCacheSize = 64 * 1024 * 1024;

        private OGLCachedResource<OGLStreamBuffer> Cache;

        public OGLConstBuffer()
        {
            Cache = new OGLCachedResource<OGLStreamBuffer>(DeleteBuffer, MaxConstBufferCacheSize);
        }

        public void LockCache()
        {
            Cache.Lock();
        }

        public void UnlockCache()
        {
            Cache.Unlock();
        }

        public void Create(long Key, long Size)
        {
            OGLStreamBuffer Buffer = new OGLStreamBuffer(BufferTarget.UniformBuffer, Size);

            Cache.AddOrUpdate(Key, Buffer, Size);
        }

        public bool IsCached(long Key, long Size)
        {
            return Cache.TryGetSize(Key, out long CachedSize) && CachedSize == Size;
        }

        public void SetData(long Key, long Size, IntPtr HostAddress)
        {
            if (Cache.TryGetValue(Key, out OGLStreamBuffer Buffer))
            {
                Buffer.SetData(Size, HostAddress);
            }
        }

        public bool TryGetUbo(long Key, out int UboHandle)
        {
            if (Cache.TryGetValue(Key, out OGLStreamBuffer Buffer))
            {
                UboHandle = Buffer.Handle;

                return true;
            }

            UboHandle = 0;

            return false;
        }

        private static void DeleteBuffer(OGLStreamBuffer Buffer)
        {
            Buffer.Dispose();
        }
    }
}