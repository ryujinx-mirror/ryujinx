using Ryujinx.Graphics.Gpu.Memory;
using Ryujinx.Graphics.Nvdec.Image;

namespace Ryujinx.Graphics.Nvdec
{
    readonly struct ResourceManager
    {
        public MemoryManager Gmm { get; }
        public SurfaceCache Cache { get; }

        public ResourceManager(MemoryManager gmm, SurfaceCache cache)
        {
            Gmm = gmm;
            Cache = cache;
        }
    }
}
