using Ryujinx.Graphics.Shader;
using Ryujinx.Memory.Range;

namespace Ryujinx.Graphics.Gpu.Memory
{
    /// <summary>
    /// Memory range used for buffers.
    /// </summary>
    readonly struct BufferBounds
    {
        /// <summary>
        /// Physical memory ranges where the buffer is mapped.
        /// </summary>
        public MultiRange Range { get; }

        /// <summary>
        /// Buffer usage flags.
        /// </summary>
        public BufferUsageFlags Flags { get; }

        /// <summary>
        /// Indicates that the backing memory for the buffer does not exist.
        /// </summary>
        public bool IsUnmapped => Range.IsUnmapped;

        /// <summary>
        /// Creates a new buffer region.
        /// </summary>
        /// <param name="range">Physical memory ranges where the buffer is mapped</param>
        /// <param name="flags">Buffer usage flags</param>
        public BufferBounds(MultiRange range, BufferUsageFlags flags = BufferUsageFlags.None)
        {
            Range = range;
            Flags = flags;
        }
    }
}
