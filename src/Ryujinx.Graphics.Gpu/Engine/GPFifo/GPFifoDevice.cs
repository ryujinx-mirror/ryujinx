using Ryujinx.Graphics.Gpu.Memory;
using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace Ryujinx.Graphics.Gpu.Engine.GPFifo
{
    /// <summary>
    /// Represents a GPU General Purpose FIFO device.
    /// </summary>
    public sealed class GPFifoDevice : IDisposable
    {
        /// <summary>
        /// Indicates if the command buffer has pre-fetch enabled.
        /// </summary>
        private enum CommandBufferType
        {
            Prefetch,
            NoPrefetch,
        }

        /// <summary>
        /// Command buffer data.
        /// </summary>
        private struct CommandBuffer
        {
            /// <summary>
            /// Processor used to process the command buffer. Contains channel state.
            /// </summary>
            public GPFifoProcessor Processor;

            /// <summary>
            /// The type of the command buffer.
            /// </summary>
            public CommandBufferType Type;

            /// <summary>
            /// Fetched data.
            /// </summary>
            public int[] Words;

            /// <summary>
            /// The GPFIFO entry address (used in <see cref="CommandBufferType.NoPrefetch"/> mode).
            /// </summary>
            public ulong EntryAddress;

            /// <summary>
            /// The count of entries inside this GPFIFO entry.
            /// </summary>
            public uint EntryCount;

            /// <summary>
            /// Get the entries for the command buffer from memory.
            /// </summary>
            /// <param name="memoryManager">The memory manager used to fetch the data</param>
            /// <param name="flush">If true, flushes potential GPU written data before reading the command buffer</param>
            /// <returns>The fetched data</returns>
            private readonly ReadOnlySpan<int> GetWords(MemoryManager memoryManager, bool flush)
            {
                return MemoryMarshal.Cast<byte, int>(memoryManager.GetSpan(EntryAddress, (int)EntryCount * 4, flush));
            }

            /// <summary>
            /// Prefetch the command buffer.
            /// </summary>
            /// <param name="memoryManager">The memory manager used to fetch the data</param>
            public void Prefetch(MemoryManager memoryManager)
            {
                Words = GetWords(memoryManager, true).ToArray();
            }

            /// <summary>
            /// Fetch the command buffer.
            /// </summary>
            /// <param name="memoryManager">The memory manager used to fetch the data</param>
            /// <param name="flush">If true, flushes potential GPU written data before reading the command buffer</param>
            /// <returns>The command buffer words</returns>
            public readonly ReadOnlySpan<int> Fetch(MemoryManager memoryManager, bool flush)
            {
                return Words ?? GetWords(memoryManager, flush);
            }
        }

        private readonly ConcurrentQueue<CommandBuffer> _commandBufferQueue;

        private GPFifoProcessor _prevChannelProcessor;

        private readonly bool _ibEnable;
        private readonly GpuContext _context;
        private readonly AutoResetEvent _event;

        private bool _interrupt;
        private int _flushSkips;

        /// <summary>
        /// Creates a new instance of the GPU General Purpose FIFO device.
        /// </summary>
        /// <param name="context">GPU context that the GPFIFO belongs to</param>
        internal GPFifoDevice(GpuContext context)
        {
            _commandBufferQueue = new ConcurrentQueue<CommandBuffer>();
            _ibEnable = true;
            _context = context;
            _event = new AutoResetEvent(false);
        }

        /// <summary>
        /// Signal the FIFO that there are new entries to process.
        /// </summary>
        public void SignalNewEntries()
        {
            _event.Set();
        }

        /// <summary>
        /// Push a GPFIFO entry in the form of a prefetched command buffer.
        /// It is intended to be used by nvservices to handle special cases.
        /// </summary>
        /// <param name="processor">Processor used to process <paramref name="commandBuffer"/></param>
        /// <param name="commandBuffer">The command buffer containing the prefetched commands</param>
        internal void PushHostCommandBuffer(GPFifoProcessor processor, int[] commandBuffer)
        {
            _commandBufferQueue.Enqueue(new CommandBuffer
            {
                Processor = processor,
                Type = CommandBufferType.Prefetch,
                Words = commandBuffer,
                EntryAddress = ulong.MaxValue,
                EntryCount = (uint)commandBuffer.Length,
            });
        }

        /// <summary>
        /// Create a CommandBuffer from a GPFIFO entry.
        /// </summary>
        /// <param name="processor">Processor used to process the command buffer pointed to by <paramref name="entry"/></param>
        /// <param name="entry">The GPFIFO entry</param>
        /// <returns>A new CommandBuffer based on the GPFIFO entry</returns>
        private static CommandBuffer CreateCommandBuffer(GPFifoProcessor processor, GPEntry entry)
        {
            CommandBufferType type = CommandBufferType.Prefetch;

            if (entry.Entry1Sync == Entry1Sync.Wait)
            {
                type = CommandBufferType.NoPrefetch;
            }

            ulong startAddress = ((ulong)entry.Entry0Get << 2) | ((ulong)entry.Entry1GetHi << 32);

            return new CommandBuffer
            {
                Processor = processor,
                Type = type,
                Words = null,
                EntryAddress = startAddress,
                EntryCount = (uint)entry.Entry1Length,
            };
        }

        /// <summary>
        /// Pushes GPFIFO entries.
        /// </summary>
        /// <param name="processor">Processor used to process the command buffers pointed to by <paramref name="entries"/></param>
        /// <param name="entries">GPFIFO entries</param>
        internal void PushEntries(GPFifoProcessor processor, ReadOnlySpan<ulong> entries)
        {
            bool beforeBarrier = true;

            for (int index = 0; index < entries.Length; index++)
            {
                ulong entry = entries[index];

                CommandBuffer commandBuffer = CreateCommandBuffer(processor, Unsafe.As<ulong, GPEntry>(ref entry));

                if (beforeBarrier && commandBuffer.Type == CommandBufferType.Prefetch)
                {
                    commandBuffer.Prefetch(processor.MemoryManager);
                }

                if (commandBuffer.Type == CommandBufferType.NoPrefetch)
                {
                    beforeBarrier = false;
                }

                _commandBufferQueue.Enqueue(commandBuffer);
            }
        }

        /// <summary>
        /// Waits until commands are pushed to the FIFO.
        /// </summary>
        /// <returns>True if commands were received, false if wait timed out</returns>
        public bool WaitForCommands()
        {
            return !_commandBufferQueue.IsEmpty || (_event.WaitOne(8) && !_commandBufferQueue.IsEmpty);
        }

        /// <summary>
        /// Processes commands pushed to the FIFO.
        /// </summary>
        public void DispatchCalls()
        {
            // Use this opportunity to also dispose any pending channels that were closed.
            _context.RunDeferredActions();

            // Process command buffers.
            while (_ibEnable && !_interrupt && _commandBufferQueue.TryDequeue(out CommandBuffer entry))
            {
                bool flushCommandBuffer = true;

                if (_flushSkips != 0)
                {
                    _flushSkips--;
                    flushCommandBuffer = false;
                }

                ReadOnlySpan<int> words = entry.Fetch(entry.Processor.MemoryManager, flushCommandBuffer);

                // If we are changing the current channel,
                // we need to force all the host state to be updated.
                if (_prevChannelProcessor != entry.Processor)
                {
                    _prevChannelProcessor = entry.Processor;
                    entry.Processor.ForceAllDirty();
                }

                entry.Processor.Process(entry.EntryAddress, words);
            }

            _interrupt = false;
        }

        /// <summary>
        /// Sets the number of flushes that should be skipped for subsequent command buffers.
        /// </summary>
        /// <remarks>
        /// This can improve performance when command buffer data only needs to be consumed by the GPU.
        /// </remarks>
        /// <param name="count">The amount of flushes that should be skipped</param>
        internal void SetFlushSkips(int count)
        {
            _flushSkips = count;
        }

        /// <summary>
        /// Interrupts command processing. This will break out of the DispatchCalls loop.
        /// </summary>
        public void Interrupt()
        {
            _interrupt = true;
            _event.Set();
        }

        /// <summary>
        /// Disposes of resources used for GPFifo command processing.
        /// </summary>
        public void Dispose() => _event.Dispose();
    }
}
