namespace Ryujinx.Graphics.Gpu.Memory
{
    /// <summary>
    /// Memory range used for buffers.
    /// </summary>
    struct BufferBounds
    {
        /// <summary>
        /// Region virtual address.
        /// </summary>
        public ulong Address { get; }

        /// <summary>
        /// Region size in bytes.
        /// </summary>
        public ulong Size { get; }

        /// <summary>
        /// Creates a new buffer region.
        /// </summary>
        /// <param name="address">Region address</param>
        /// <param name="size">Region size</param>
        public BufferBounds(ulong address, ulong size)
        {
            Address = address;
            Size = size;
        }
    }
}