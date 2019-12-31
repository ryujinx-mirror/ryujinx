namespace Ryujinx.Graphics.Gpu.State
{
    /// <summary>
    /// Texture or sampler pool state.
    /// </summary>
    struct PoolState
    {
        public GpuVa Address;
        public int   MaximumId;
    }
}