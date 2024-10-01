using Ryujinx.Graphics.Device;
using Ryujinx.Graphics.Nvdec.Image;

namespace Ryujinx.Graphics.Nvdec
{
    readonly struct ResourceManager
    {
        public DeviceMemoryManager MemoryManager { get; }
        public SurfaceCache Cache { get; }

        public ResourceManager(DeviceMemoryManager mm, SurfaceCache cache)
        {
            MemoryManager = mm;
            Cache = cache;
        }
    }
}
