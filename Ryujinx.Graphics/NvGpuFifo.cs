using Ryujinx.Graphics.Memory;
using System.Collections.Concurrent;
using System.Threading;

namespace Ryujinx.Graphics
{
    public class NvGpuFifo
    {
        private const int MacrosCount    = 0x80;
        private const int MacroIndexMask = MacrosCount - 1;

        //Note: The size of the macro memory is unknown, we just make
        //a guess here and use 256kb as the size. Increase if needed.
        private const int MmeWords = 256 * 256;

        private NvGpu Gpu;

        private ConcurrentQueue<(NvGpuVmm, NvGpuPBEntry[])> BufferQueue;

        private NvGpuEngine[] SubChannels;

        public AutoResetEvent Event { get; private set; }

        private struct CachedMacro
        {
            public int Position { get; private set; }

            private MacroInterpreter Interpreter;

            public CachedMacro(NvGpuFifo PFifo, INvGpuEngine Engine, int Position)
            {
                this.Position = Position;

                Interpreter = new MacroInterpreter(PFifo, Engine);
            }

            public void PushParam(int Param)
            {
                Interpreter?.Fifo.Enqueue(Param);
            }

            public void Execute(NvGpuVmm Vmm, int[] Mme, int Param)
            {
                Interpreter?.Execute(Vmm, Mme, Position, Param);
            }
        }

        private int CurrMacroPosition;
        private int CurrMacroBindIndex;

        private CachedMacro[] Macros;

        private int[] Mme;

        public NvGpuFifo(NvGpu Gpu)
        {
            this.Gpu = Gpu;

            BufferQueue = new ConcurrentQueue<(NvGpuVmm, NvGpuPBEntry[])>();

            SubChannels = new NvGpuEngine[8];

            Macros = new CachedMacro[MacrosCount];

            Mme = new int[MmeWords];

            Event = new AutoResetEvent(false);
        }

        public void PushBuffer(NvGpuVmm Vmm, NvGpuPBEntry[] Buffer)
        {
            BufferQueue.Enqueue((Vmm, Buffer));

            Event.Set();
        }

        public void DispatchCalls()
        {
            while (Step());
        }

        private (NvGpuVmm Vmm, NvGpuPBEntry[] Pb) Curr;

        private int CurrPbEntryIndex;

        public bool Step()
        {
            while (Curr.Pb == null || Curr.Pb.Length <= CurrPbEntryIndex)
            {
                if (!BufferQueue.TryDequeue(out Curr))
                {
                    return false;
                }

                Gpu.Engine3d.ResetCache();

                CurrPbEntryIndex = 0;
            }

            CallMethod(Curr.Vmm, Curr.Pb[CurrPbEntryIndex++]);

            return true;
        }

        private void CallMethod(NvGpuVmm Vmm, NvGpuPBEntry PBEntry)
        {
            if (PBEntry.Method < 0x80)
            {
                switch ((NvGpuFifoMeth)PBEntry.Method)
                {
                    case NvGpuFifoMeth.BindChannel:
                    {
                        NvGpuEngine Engine = (NvGpuEngine)PBEntry.Arguments[0];

                        SubChannels[PBEntry.SubChannel] = Engine;

                        break;
                    }

                    case NvGpuFifoMeth.SetMacroUploadAddress:
                    {
                        CurrMacroPosition = PBEntry.Arguments[0];

                        break;
                    }

                    case NvGpuFifoMeth.SendMacroCodeData:
                    {
                        foreach (int Arg in PBEntry.Arguments)
                        {
                            Mme[CurrMacroPosition++] = Arg;
                        }
                        break;
                    }

                    case NvGpuFifoMeth.SetMacroBindingIndex:
                    {
                        CurrMacroBindIndex = PBEntry.Arguments[0];

                        break;
                    }

                    case NvGpuFifoMeth.BindMacro:
                    {
                        int Position = PBEntry.Arguments[0];

                        Macros[CurrMacroBindIndex] = new CachedMacro(this, Gpu.Engine3d, Position);

                        break;
                    }
                }
            }
            else
            {
                switch (SubChannels[PBEntry.SubChannel])
                {
                    case NvGpuEngine._2d: Call2dMethod (Vmm, PBEntry); break;
                    case NvGpuEngine._3d: Call3dMethod (Vmm, PBEntry); break;
                    case NvGpuEngine.Dma: CallDmaMethod(Vmm, PBEntry); break;
                }
            }
        }

        private void Call2dMethod(NvGpuVmm Vmm, NvGpuPBEntry PBEntry)
        {
            Gpu.Engine2d.CallMethod(Vmm, PBEntry);
        }

        private void Call3dMethod(NvGpuVmm Vmm, NvGpuPBEntry PBEntry)
        {
            if (PBEntry.Method < 0xe00)
            {
                Gpu.Engine3d.CallMethod(Vmm, PBEntry);
            }
            else
            {
                int MacroIndex = (PBEntry.Method >> 1) & MacroIndexMask;

                if ((PBEntry.Method & 1) != 0)
                {
                    foreach (int Arg in PBEntry.Arguments)
                    {
                        Macros[MacroIndex].PushParam(Arg);
                    }
                }
                else
                {
                    Macros[MacroIndex].Execute(Vmm, Mme, PBEntry.Arguments[0]);
                }
            }
        }

        private void CallDmaMethod(NvGpuVmm Vmm, NvGpuPBEntry PBEntry)
        {
            Gpu.EngineDma.CallMethod(Vmm, PBEntry);
        }
    }
}