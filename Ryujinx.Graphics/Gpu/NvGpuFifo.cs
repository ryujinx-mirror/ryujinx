using ChocolArm64.Memory;
using System.Collections.Concurrent;

namespace Ryujinx.Graphics.Gpu
{
    public class NvGpuFifo
    {
        private const int MacrosCount    = 0x80;
        private const int MacroIndexMask = MacrosCount - 1;

        private NsGpu Gpu;

        private ConcurrentQueue<(AMemory, NsGpuPBEntry)> BufferQueue;

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

            public void Execute(AMemory Memory, int Param)
            {
                Interpreter?.Execute(Memory, Position, Param);
            }
        }

        private long CurrMacroPosition;
        private int  CurrMacroBindIndex;

        private CachedMacro[] Macros;

        public NvGpuFifo(NsGpu Gpu)
        {
            this.Gpu = Gpu;

            BufferQueue = new ConcurrentQueue<(AMemory, NsGpuPBEntry)>();

            SubChannels = new NvGpuEngine[8];

            Macros = new CachedMacro[MacrosCount];
        }

        public void PushBuffer(AMemory Memory, NsGpuPBEntry[] Buffer)
        {
            foreach (NsGpuPBEntry PBEntry in Buffer)
            {
                BufferQueue.Enqueue((Memory, PBEntry));
            }
        }

        public void DispatchCalls()
        {
            while (Step());
        }

        public bool Step()
        {
            if (BufferQueue.TryDequeue(out (AMemory Memory, NsGpuPBEntry PBEntry) Tuple))
            {
                CallMethod(Tuple.Memory, Tuple.PBEntry);

                return true;
            }

            return false;
        }

        private void CallMethod(AMemory Memory, NsGpuPBEntry PBEntry)
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
                        long Position = Gpu.GetCpuAddr(CurrMacroPosition);

                        foreach (int Arg in PBEntry.Arguments)
                        {
                            Memory.WriteInt32(Position, Arg);

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

                        Position = Gpu.GetCpuAddr(Position);

                        Macros[CurrMacroBindIndex] = new CachedMacro(this, Gpu.Engine3d, Position);

                        break;
                    }
                }
            }
            else
            {
                switch (SubChannels[PBEntry.SubChannel])
                {
                    case NvGpuEngine._3d: Call3dMethod(Memory, PBEntry); break;
                }
            }
        }

        private void Call3dMethod(AMemory Memory, NsGpuPBEntry PBEntry)
        {
            if (PBEntry.Method < 0xe00)
            {
                Gpu.Engine3d.CallMethod(Memory, PBEntry);
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
                    Macros[MacroIndex].Execute(Memory, PBEntry.Arguments[0]);
                }
            }
        }
    }
}