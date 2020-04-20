namespace Ryujinx.Graphics.Gpu.State
{
    /// <summary>
    /// Texture or sampler pool state.
    /// </summary>
    struct PoolState
    {
#pragma warning disable CS0649
        public GpuVa Address;
        public int   MaximumId;
#pragma warning restore CS0649
    }
}