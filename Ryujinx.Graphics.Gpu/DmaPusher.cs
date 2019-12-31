using System.Collections.Concurrent;
using System.Threading;

namespace Ryujinx.Graphics.Gpu
{
    /// <summary>
    /// GPU DMA pusher, used to push commands to the GPU.
    /// </summary>
    public class DmaPusher
    {
        private ConcurrentQueue<ulong> _ibBuffer;

        private ulong _dmaPut;
        private ulong _dmaGet;

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
        private bool _nonMain;

        private ulong _dmaMGet;

        private GpuContext _context;

        private AutoResetEvent _event;

        /// <summary>
        /// Creates a new instance of the GPU DMA pusher.
        /// </summary>
        /// <param name="context">GPU context that the pusher belongs to</param>
        internal DmaPusher(GpuContext context)
        {
            _context = context;

            _ibBuffer = new ConcurrentQueue<ulong>();

            _ibEnable = true;

            _event = new AutoResetEvent(false);
        }

        /// <summary>
        /// Pushes a GPFIFO entry.
        /// </summary>
        /// <param name="entry">GPFIFO entry</param>
        public void Push(ulong entry)
        {
            _ibBuffer.Enqueue(entry);

            _event.Set();
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
        /// <returns></returns>
        private bool Step()
        {
            if (_dmaGet != _dmaPut)
            {
                int word = _context.MemoryAccessor.ReadInt32(_dmaGet);

                _dmaGet += 4;

                if (!_nonMain)
                {
                    _dmaMGet = _dmaGet;
                }

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
            else if (_ibEnable && _ibBuffer.TryDequeue(out ulong entry))
            {
                ulong length = (entry >> 42) & 0x1fffff;

                _dmaGet = entry & 0xfffffffffc;
                _dmaPut = _dmaGet + length * 4;

                _nonMain = (entry & (1UL << 41)) != 0;
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