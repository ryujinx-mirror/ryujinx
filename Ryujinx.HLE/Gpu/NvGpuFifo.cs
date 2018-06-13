using System.Collections.Concurrent;

namespace Ryujinx.HLE.Gpu
{
    class NvGpuFifo
    {
        private const int MacrosCount    = 0x80;
        private const int MacroIndexMask = MacrosCount - 1;

        private NvGpu Gpu;

        private ConcurrentQueue<(NvGpuVmm, NvGpuPBEntry)> BufferQueue;

        private NvGpuEngine[] SubChannels;

        private struct CachedMacro
        {
            public long Position { get; private set; }

            private MacroInterpreter Interpreter;

            public CachedMacro(NvGpuFifo PFifo, INvGpuEngine Engine, long Position)
            {
                this.Position = Position;

                Interpreter = new MacroInterpreter(PFifo, Engine);
            }

            public void PushParam(int Param)
            {
                Interpreter?.Fifo.Enqueue(Param);
            }

            public void Execute(NvGpuVmm Vmm, int Param)
            {
                Interpreter?.Execute(Vmm, Position, Param);
            }
        }

        private long CurrMacroPosition;
        private int  CurrMacroBindIndex;

        private CachedMacro[] Macros;

        public NvGpuFifo(NvGpu Gpu)
        {
            this.Gpu = Gpu;

            BufferQueue = new ConcurrentQueue<(NvGpuVmm, NvGpuPBEntry)>();

            SubChannels = new NvGpuEngine[8];

            Macros = new CachedMacro[MacrosCount];
        }

        public void PushBuffer(NvGpuVmm Vmm, NvGpuPBEntry[] Buffer)
        {
            foreach (NvGpuPBEntry PBEntry in Buffer)
            {
                BufferQueue.Enqueue((Vmm, PBEntry));
            }
        }

        public void DispatchCalls()
        {
            while (Step());
        }

        public bool Step()
        {
            if (BufferQueue.TryDequeue(out (NvGpuVmm Vmm, NvGpuPBEntry PBEntry) Tuple))
            {
                CallMethod(Tuple.Vmm, Tuple.PBEntry);

                return true;
            }

            return false;
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
                        CurrMacroPosition = (long)((ulong)PBEntry.Arguments[0] << 2);

                        break;
                    }

                    case NvGpuFifoMeth.SendMacroCodeData:
                    {
                        long Position = CurrMacroPosition;

                        foreach (int Arg in PBEntry.Arguments)
                        {
                            Vmm.WriteInt32(Position, Arg);

                            CurrMacroPosition += 4;

                            Position += 4;
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
                        long Position = (long)((ulong)PBEntry.Arguments[0] << 2);

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
                    Macros[MacroIndex].Execute(Vmm, PBEntry.Arguments[0]);
                }
            }
        }

        private void CallDmaMethod(NvGpuVmm Vmm, NvGpuPBEntry PBEntry)
        {
            Gpu.EngineDma.CallMethod(Vmm, PBEntry);
        }
    }
}