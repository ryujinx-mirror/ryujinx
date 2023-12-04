using Ryujinx.Graphics.GAL;
using Ryujinx.Memory.Range;
using System;

namespace Ryujinx.Graphics.Gpu.Memory
{
    /// <summary>
    /// Buffer, used to store vertex and index data, uniform and storage buffers, and others.
    /// </summary>
    class MultiRangeBuffer : IMultiRangeItem, IDisposable
    {
        private readonly GpuContext _context;

        /// <summary>
        /// Host buffer handle.
        /// </summary>
        public BufferHandle Handle { get; }

        /// <summary>
        /// Range of memory where the data is located.
        /// </summary>
        public MultiRange Range { get; }

        /// <summary>
        /// Creates a new instance of the buffer.
        /// </summary>
        /// <param name="context">GPU context that the buffer belongs to</param>
        /// <param name="range">Range of memory where the data is mapped</param>
        /// <param name="storages">Backing memory for the buffers</param>
        public MultiRangeBuffer(GpuContext context, MultiRange range, ReadOnlySpan<BufferRange> storages)
        {
            _context = context;
            Range = range;
            Handle = context.Renderer.CreateBufferSparse(storages);
        }

        /// <summary>
        /// Gets a sub-range from the buffer.
        /// </summary>
        /// <remarks>
        /// This can be used to bind and use sub-ranges of the buffer on the host API.
        /// </remarks>
        /// <param name="range">Range of memory where the data is mapped</param>
        /// <returns>The buffer sub-range</returns>
        public BufferRange GetRange(MultiRange range)
        {
            int offset = Range.FindOffset(range);

            return new BufferRange(Handle, offset, (int)range.GetSize());
        }

        /// <summary>
        /// Disposes the host buffer.
        /// </summary>
        public void Dispose()
        {
            _context.Renderer.DeleteBuffer(Handle);
        }
    }
}
