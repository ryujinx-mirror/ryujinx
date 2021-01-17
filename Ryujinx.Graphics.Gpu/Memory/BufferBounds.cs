using Ryujinx.Graphics.Shader;

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
        /// Buffer usage flags.
        /// </summary>
        public BufferUsageFlags Flags { get; }

        /// <summary>
        /// Creates a new buffer region.
        /// </summary>
        /// <param name="address">Region address</param>
        /// <param name="size">Region size</param>
        /// <param name="flags">Buffer usage flags</param>
        public BufferBounds(ulong address, ulong size, BufferUsageFlags flags = BufferUsageFlags.None)
        {
            Address = address;
            Size = size;
            Flags = flags;
        }
    }
}