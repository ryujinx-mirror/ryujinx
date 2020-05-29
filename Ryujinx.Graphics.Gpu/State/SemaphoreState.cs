namespace Ryujinx.Graphics.Gpu.State
{
    /// <summary>
    /// GPU semaphore state.
    /// </summary>
    struct SemaphoreState
    {
#pragma warning disable CS0649
        public GpuVa Address;
        public int   Payload;
        public uint  Control;
#pragma warning restore CS0649
    }
}
