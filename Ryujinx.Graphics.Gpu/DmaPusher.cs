using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading;

namespace Ryujinx.Graphics.Gpu
{
    /// <summary>
    /// GPU DMA pusher, used to push commands to the GPU.
    /// </summary>
    public class DmaPusher
    {
        private ConcurrentQueue<CommandBuffer> _commandBufferQueue;

        private enum CommandBufferType
        {
            Prefetch,
            NoPrefetch,
        }

        private struct CommandBuffer
        {
            /// <summary>
            /// The type of the command buffer.
            /// </summary>
            public CommandBufferType Type;

            /// <summary>
            /// Fetched data.
            /// </summary>
            public int[] Words;

            /// <summary>
            /// The GPFIFO entry address. (used in NoPrefetch mode)
            /// </summary>
            public ulong EntryAddress;

            /// <summary>
            /// The count of entries inside this GPFIFO entry.
            /// </summary>
            public uint EntryCount;

            /// <summary>
            /// Fetch the command buffer.
            /// </summary>
            public void Fetch(GpuContext context)
            {
                if (Words == null)
                {
                    Words = MemoryMarshal.Cast<byte, int>(context.MemoryAccessor.GetSpan(EntryAddress, EntryCount * 4)).ToArray();
                }
            }

            /// <summary>
            /// Read inside the command buffer.
            /// </summary>
            /// <param name="context">The GPU context</param>
            /// <param name="index">The index inside the command buffer</param>
            /// <returns>The value read</returns>
            public int ReadAt(GpuContext context, int index)
            {
                return Words[index];
            }
        }

        private CommandBuffer _currentCommandBuffer;
        private int           _wordsPosition;

        /// <summary>
        /// Internal GPFIFO state.
        /// </summary>
        private struct DmaState
        {
            public int  Method;
            public int  SubChannel;
            public int  MethodCount;
            public bool NonIncrementing;
            public bool IncrementOnce;
            public int  LengthPending;
        }

        private DmaState _state;

        private bool _sliEnable;
        private bool _sliActive;

        private bool _ibEnable;

        private GpuContext _context;

        private AutoResetEvent _event;

        /// <summary>
        /// Creates a new instance of the GPU DMA pusher.
        /// </summary>
        /// <param name="context">GPU context that the pusher belongs to</param>
        internal DmaPusher(GpuContext context)
        {
            _context = context;

            _ibEnable = true;

            _commandBufferQueue = new ConcurrentQueue<CommandBuffer>();

            _event = new AutoResetEvent(false);
        }

        /// <summary>
        /// Signal the pusher that there are new entries to process.
        /// </summary>
        public void SignalNewEntries()
        {
            _event.Set();
        }

        /// <summary>
        /// Push a GPFIFO entry in the form of a prefetched command buffer.
        /// It is intended to be used by nvservices to handle special cases.
        /// </summary>
        /// <param name="commandBuffer">The command buffer containing the prefetched commands</param>
        public void PushHostCommandBuffer(int[] commandBuffer)
        {
            _commandBufferQueue.Enqueue(new CommandBuffer
            {
                Type         = CommandBufferType.Prefetch,
                Words        = commandBuffer,
                EntryAddress = ulong.MaxValue,
                EntryCount   = (uint)commandBuffer.Length
            });
        }

        /// <summary>
        /// Create a CommandBuffer from a GPFIFO entry.
        /// </summary>
        /// <param name="entry">The GPFIFO entry</param>
        /// <returns>A new CommandBuffer based on the GPFIFO entry</returns>
        private CommandBuffer CreateCommandBuffer(ulong entry)
        {
            ulong length       = (entry >> 42) & 0x1fffff;
            ulong startAddress = entry & 0xfffffffffc;

            bool noPrefetch = (entry & (1UL << 63)) != 0;

            CommandBufferType type = CommandBufferType.Prefetch;

            if (noPrefetch)
            {
                type = CommandBufferType.NoPrefetch;
            }

            return new CommandBuffer
            {
                Type         = type,
                Words        = null,
                EntryAddress = startAddress,
                EntryCount   = (uint)length
            };
        }

        /// <summary>
        /// Pushes GPFIFO entries.
        /// </summary>
        /// <param name="entries">GPFIFO entries</param>
        public void PushEntries(ReadOnlySpan<ulong> entries)
        {
            bool beforeBarrier = true;

            foreach (ulong entry in entries)
            {
                CommandBuffer commandBuffer = CreateCommandBuffer(entry);

                if (beforeBarrier && commandBuffer.Type == CommandBufferType.Prefetch)
                {
                    commandBuffer.Fetch(_context);
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
            return _event.WaitOne(8);
        }

        /// <summary>
        /// Processes commands pushed to the FIFO.
        /// </summary>
        public void DispatchCalls()
        {
            while (Step());
        }

        /// <summary>
        /// Processes a single command on the FIFO.
        /// </summary>
        /// <returns>True if the FIFO still has commands to be processed, false otherwise</returns>
        private bool Step()
        {
            if (_wordsPosition != _currentCommandBuffer.EntryCount)
            {
                int word = _currentCommandBuffer.ReadAt(_context, _wordsPosition++);

                if (_state.LengthPending != 0)
                {
                    _state.LengthPending = 0;
                    _state.MethodCount   = word & 0xffffff;
                }
                else if (_state.MethodCount != 0)
                {
                    if (!_sliEnable || _sliActive)
                    {
                        CallMethod(word);
                    }

                    if (!_state.NonIncrementing)
                    {
                        _state.Method++;
                    }

                    if (_state.IncrementOnce)
                    {
                        _state.NonIncrementing = true;
                    }

                    _state.MethodCount--;
                }
                else
                {
                    int submissionMode = (word >> 29) & 7;

                    switch (submissionMode)
                    {
                        case 1:
                            // Incrementing.
                            SetNonImmediateState(word);

                            _state.NonIncrementing = false;
                            _state.IncrementOnce   = false;

                            break;

                        case 3:
                            // Non-incrementing.
                            SetNonImmediateState(word);

                            _state.NonIncrementing = true;
                            _state.IncrementOnce   = false;

                            break;

                        case 4:
                            // Immediate.
                            _state.Method          = (word >> 0)  & 0x1fff;
                            _state.SubChannel      = (word >> 13) & 7;
                            _state.NonIncrementing = true;
                            _state.IncrementOnce   = false;

                            CallMethod((word >> 16) & 0x1fff);

                            break;

                        case 5:
                            // Increment-once.
                            SetNonImmediateState(word);

                            _state.NonIncrementing = false;
                            _state.IncrementOnce   = true;

                            break;
                    }
                }
            }
            else if (_ibEnable && _commandBufferQueue.TryDequeue(out CommandBuffer entry))
            {
                _currentCommandBuffer = entry;
                _wordsPosition        = 0;

                _currentCommandBuffer.Fetch(_context);
            }
            else
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Sets current non-immediate method call state.
        /// </summary>
        /// <param name="word">Compressed method word</param>
        private void SetNonImmediateState(int word)
        {
            _state.Method      = (word >> 0)  & 0x1fff;
            _state.SubChannel  = (word >> 13) & 7;
            _state.MethodCount = (word >> 16) & 0x1fff;
        }

        /// <summary>
        /// Forwards the method call to GPU engines.
        /// </summary>
        /// <param name="argument">Call argument</param>
        private void CallMethod(int argument)
        {
            _context.Fifo.CallMethod(new MethodParams(
                _state.Method,
                argument,
                _state.SubChannel,
                _state.MethodCount));
        }
    }
}