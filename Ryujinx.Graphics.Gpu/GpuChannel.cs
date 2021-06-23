using Ryujinx.Graphics.Gpu.Engine.GPFifo;
using Ryujinx.Graphics.Gpu.Image;
using Ryujinx.Graphics.Gpu.Memory;
using System;

namespace Ryujinx.Graphics.Gpu
{
    /// <summary>
    /// Represents a GPU channel.
    /// </summary>
    public class GpuChannel : IDisposable
    {
        private readonly GpuContext _context;
        private readonly GPFifoDevice _device;
        private readonly GPFifoProcessor _processor;

        /// <summary>
        /// Channel buffer bindings manager.
        /// </summary>
        internal BufferManager BufferManager { get; }

        /// <summary>
        /// Channel texture bindings manager.
        /// </summary>
        internal TextureManager TextureManager { get; }

        /// <summary>
        /// Creates a new instance of a GPU channel.
        /// </summary>
        /// <param name="context">GPU context that the channel belongs to</param>
        internal GpuChannel(GpuContext context)
        {
            _context = context;
            _device = context.GPFifo;
            _processor = new GPFifoProcessor(context, this);
            BufferManager = new BufferManager(context);
            TextureManager = new TextureManager(context, this);
        }

        /// <summary>
        /// Push a GPFIFO entry in the form of a prefetched command buffer.
        /// It is intended to be used by nvservices to handle special cases.
        /// </summary>
        /// <param name="commandBuffer">The command buffer containing the prefetched commands</param>
        public void PushHostCommandBuffer(int[] commandBuffer)
        {
            _device.PushHostCommandBuffer(_processor, commandBuffer);
        }

        /// <summary>
        /// Pushes GPFIFO entries.
        /// </summary>
        /// <param name="entries">GPFIFO entries</param>
        public void PushEntries(ReadOnlySpan<ulong> entries)
        {
            _device.PushEntries(_processor, entries);
        }

        /// <summary>
        /// Disposes the GPU channel.
        /// It's an error to use the GPU channel after disposal.
        /// </summary>
        public void Dispose()
        {
            _context.DisposedChannels.Enqueue(this);
        }

        /// <summary>
        /// Performs disposal of the host GPU resources used by this channel, that are not shared.
        /// This must only be called from the render thread.
        /// </summary>
        internal void Destroy()
        {
            BufferManager.Dispose();
            TextureManager.Dispose();
        }
    }
}
