namespace Ryujinx.Graphics.Gpu.State
{
    /// <summary>
    /// Vertex buffer state.
    /// </summary>
    struct VertexBufferState
    {
#pragma warning disable CS0649
        public uint  Control;
        public GpuVa Address;
        public int   Divisor;
#pragma warning restore CS0649

        /// <summary>
        /// Vertex buffer stride, defined as the number of bytes occupied by each vertex in memory.
        /// </summary>
        /// <returns>Vertex buffer stride</returns>
        public int UnpackStride()
        {
            return (int)(Control & 0xfff);
        }

        /// <summary>
        /// Vertex buffer enable.
        /// </summary>
        /// <returns>True if the vertex buffer is enabled, false otherwise</returns>
        public bool UnpackEnable()
        {
            return (Control & (1 << 12)) != 0;
        }
    }
}
