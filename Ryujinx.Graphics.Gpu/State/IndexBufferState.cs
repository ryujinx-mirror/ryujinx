using Ryujinx.Graphics.GAL;

namespace Ryujinx.Graphics.Gpu.State
{
    struct IndexBufferState
    {
        public GpuVa     Address;
        public GpuVa     EndAddress;
        public IndexType Type;
        public int       First;
        public int       Count;
    }
}
