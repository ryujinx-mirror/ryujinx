using Ryujinx.Graphics.Memory;
using System.Collections.Concurrent;
using System.Threading;

namespace Ryujinx.Graphics
{
    public class DmaPusher
    {
        private ConcurrentQueue<(NvGpuVmm, long)> IbBuffer;

        private long DmaPut;
        private long DmaGet;

        private struct DmaState
        {
            public int  Method;
            public int  SubChannel;
            public int  MethodCount;
            public bool NonIncrementing;
            public bool IncrementOnce;
            public int  LengthPending;
        }

        private DmaState State;

        private bool SliEnable;
        private bool SliActive;

        private bool IbEnable;
        private bool NonMain;

        private long DmaMGet;

        private NvGpuVmm Vmm;

        private NvGpu Gpu;

        private AutoResetEvent Event;

        public DmaPusher(NvGpu Gpu)
        {
            this.Gpu = Gpu;

            IbBuffer = new ConcurrentQueue<(NvGpuVmm, long)>();

            IbEnable = true;

            Event = new AutoResetEvent(false);
        }

        public void Push(NvGpuVmm Vmm, long Entry)
        {
            IbBuffer.Enqueue((Vmm, Entry));

            Event.Set();
        }

        public bool WaitForCommands()
        {
            return Event.WaitOne(8);
        }

        public void DispatchCalls()
        {
            while (Step());
        }

        private bool Step()
        {
            if (DmaGet != DmaPut)
            {
                int Word = Vmm.ReadInt32(DmaGet);

                DmaGet += 4;

                if (!NonMain)
                {
                    DmaMGet = DmaGet;
                }

                if (State.LengthPending != 0)
                {
                    State.LengthPending = 0;
                    State.MethodCount   = Word & 0xffffff;
                }
                else if (State.MethodCount != 0)
                {
                    if (!SliEnable || SliActive)
                    {
                        CallMethod(Word);
                    }

                    if (!State.NonIncrementing)
                    {
                        State.Method++;
                    }

                    if (State.IncrementOnce)
                    {
                        State.NonIncrementing = true;
                    }

                    State.MethodCount--;
                }
                else
                {
                    int SumissionMode = (Word >> 29) & 7;

                    switch (SumissionMode)
                    {
                        case 1:
                            //Incrementing.
                            SetNonImmediateState(Word);

                            State.NonIncrementing = false;
                            State.IncrementOnce   = false;

                            break;

                        case 3:
                            //Non-incrementing.
                            SetNonImmediateState(Word);

                            State.NonIncrementing = true;
                            State.IncrementOnce   = false;

                            break;

                        case 4:
                            //Immediate.
                            State.Method          = (Word >> 0)  & 0x1fff;
                            State.SubChannel      = (Word >> 13) & 7;
                            State.NonIncrementing = true;
                            State.IncrementOnce   = false;

                            CallMethod((Word >> 16) & 0x1fff);

                            break;

                        case 5:
                            //Increment-once.
                            SetNonImmediateState(Word);

                            State.NonIncrementing = false;
                            State.IncrementOnce   = true;

                            break;
                    }
                }
            }
            else if (IbEnable && IbBuffer.TryDequeue(out (NvGpuVmm Vmm, long Entry) Tuple))
            {
                this.Vmm = Tuple.Vmm;

                long Entry = Tuple.Entry;

                int Length = (int)(Entry >> 42) & 0x1fffff;

                DmaGet = Entry & 0xfffffffffc;
                DmaPut = DmaGet + Length * 4;

                NonMain = (Entry & (1L << 41)) != 0;

                Gpu.ResourceManager.ClearPbCache();
            }
            else
            {
                return false;
            }

            return true;
        }

        private void SetNonImmediateState(int Word)
        {
            State.Method      = (Word >> 0)  & 0x1fff;
            State.SubChannel  = (Word >> 13) & 7;
            State.MethodCount = (Word >> 16) & 0x1fff;
        }

        private void CallMethod(int Argument)
        {
            Gpu.Fifo.CallMethod(Vmm, new GpuMethodCall(
                State.Method,
                Argument,
                State.SubChannel,
                State.MethodCount));
        }
    }
}