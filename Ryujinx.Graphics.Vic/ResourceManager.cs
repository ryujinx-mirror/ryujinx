using Ryujinx.Graphics.Gpu.Memory;
using Ryujinx.Graphics.Vic.Image;

namespace Ryujinx.Graphics.Vic
{
    struct ResourceManager
    {
        public MemoryManager Gmm { get; }
        public BufferPool<Pixel> SurfacePool { get; }
        public BufferPool<byte> BufferPool { get; }

        public ResourceManager(MemoryManager gmm, BufferPool<Pixel> surfacePool, BufferPool<byte> bufferPool)
        {
            Gmm = gmm;
            SurfacePool = surfacePool;
            BufferPool = bufferPool;
        }
    }
}
