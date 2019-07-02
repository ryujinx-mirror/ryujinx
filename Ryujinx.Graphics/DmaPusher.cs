using Ryujinx.Graphics.Memory;
using System.Collections.Concurrent;
using System.Threading;

namespace Ryujinx.Graphics
{
    public class DmaPusher
    {
        private ConcurrentQueue<(NvGpuVmm, long)> _ibBuffer;

        private long _dmaPut;
        private long _dmaGet;

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

        private long _dmaMGet;

        private NvGpuVmm _vmm;

        private NvGpu _gpu;

        private AutoResetEvent _event;

        public DmaPusher(NvGpu gpu)
        {
            _gpu = gpu;

            _ibBuffer = new ConcurrentQueue<(NvGpuVmm, long)>();

            _ibEnable = true;

            _event = new AutoResetEvent(false);
        }

        public void Push(NvGpuVmm vmm, long entry)
        {
            _ibBuffer.Enqueue((vmm, entry));

            _event.Set();
        }

        public bool WaitForCommands()
        {
            return _event.WaitOne(8);
        }

        public void DispatchCalls()
        {
            while (Step());
        }

        private bool Step()
        {
            if (_dmaGet != _dmaPut)
            {
                int word = _vmm.ReadInt32(_dmaGet);

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
            else if (_ibEnable && _ibBuffer.TryDequeue(out (NvGpuVmm Vmm, long Entry) tuple))
            {
                _vmm = tuple.Vmm;

                long entry = tuple.Entry;

                int length = (int)(entry >> 42) & 0x1fffff;

                _dmaGet = entry & 0xfffffffffc;
                _dmaPut = _dmaGet + length * 4;

                _nonMain = (entry & (1L << 41)) != 0;

                _gpu.ResourceManager.ClearPbCache();
            }
            else
            {
                return false;
            }

            return true;
        }

        private void SetNonImmediateState(int word)
        {
            _state.Method      = (word >> 0)  & 0x1fff;
            _state.SubChannel  = (word >> 13) & 7;
            _state.MethodCount = (word >> 16) & 0x1fff;
        }

        private void CallMethod(int argument)
        {
            _gpu.Fifo.CallMethod(_vmm, new GpuMethodCall(
                _state.Method,
                argument,
                _state.SubChannel,
                _state.MethodCount));
        }
    }
}