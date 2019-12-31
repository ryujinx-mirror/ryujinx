namespace Ryujinx.Graphics.Gpu.State
{
    /// <summary>
    /// Uniform buffer state for the uniform buffer currently being modified.
    /// </summary>
    struct UniformBufferState
    {
        public int   Size;
        public GpuVa Address;
        public int   Offset;
    }
}
