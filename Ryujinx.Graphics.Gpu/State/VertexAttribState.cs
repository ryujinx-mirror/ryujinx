namespace Ryujinx.Graphics.Gpu.State
{
    /// <summary>
    /// Vertex buffer attribute state.
    /// </summary>
    struct VertexAttribState
    {
#pragma warning disable CS0649
        public uint Attribute;
#pragma warning restore CS0649

        /// <summary>
        /// Unpacks the index of the vertex buffer this attribute belongs to.
        /// </summary>
        /// <returns>Vertex buffer index</returns>
        public int UnpackBufferIndex()
        {
            return (int)(Attribute & 0x1f);
        }

        /// <summary>
        /// Unpacks the attribute constant flag.
        /// </summary>
        /// <returns>True if the attribute is constant, false otherwise</returns>
        public bool UnpackIsConstant()
        {
            return (Attribute & 0x40) != 0;
        }

        /// <summary>
        /// Unpacks the offset, in bytes, of the attribute on the vertex buffer.
        /// </summary>
        /// <returns>Attribute offset in bytes</returns>
        public int UnpackOffset()
        {
            return (int)((Attribute >> 7) & 0x3fff);
        }

        /// <summary>
        /// Unpacks the Maxwell attribute format integer.
        /// </summary>
        /// <returns>Attribute format integer</returns>
        public uint UnpackFormat()
        {
            return Attribute & 0x3fe00000;
        }
    }
}
