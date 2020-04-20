using Ryujinx.Graphics.GAL;

namespace Ryujinx.Graphics.Gpu.State
{
    /// <summary>
    /// GPU index buffer state.
    /// This is used on indexed draws.
    /// </summary>
    struct IndexBufferState
    {
#pragma warning disable CS0649
        public GpuVa     Address;
        public GpuVa     EndAddress;
        public IndexType Type;
        public int       First;
        public int       Count;
#pragma warning restore CS0649
    }
}
