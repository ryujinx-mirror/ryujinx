using Ryujinx.Graphics.Memory;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Ryujinx.Graphics
{
    public class NvGpuEngineP2mf : INvGpuEngine
    {
        public int[] Registers { get; private set; }

        private NvGpu Gpu;

        private Dictionary<int, NvGpuMethod> Methods;

        private ReadOnlyCollection<int> DataBuffer;

        public NvGpuEngineP2mf(NvGpu Gpu)
        {
            this.Gpu = Gpu;

            Registers = new int[0x80];

            Methods = new Dictionary<int, NvGpuMethod>();

            void AddMethod(int Meth, int Count, int Stride, NvGpuMethod Method)
            {
                while (Count-- > 0)
                {
                    Methods.Add(Meth, Method);

                    Meth += Stride;
                }
            }

            AddMethod(0x6c, 1, 1, Execute);
            AddMethod(0x6d, 1, 1, PushData);
        }

        public void CallMethod(NvGpuVmm Vmm, NvGpuPBEntry PBEntry)
        {
            if (Methods.TryGetValue(PBEntry.Method, out NvGpuMethod Method))
            {
                Method(Vmm, PBEntry);
            }
            else
            {
                WriteRegister(PBEntry);
            }
        }

        private void Execute(NvGpuVmm Vmm, NvGpuPBEntry PBEntry)
        {
            //TODO: Some registers and copy modes are still not implemented.
            int Control = PBEntry.Arguments[0];

            long DstAddress = MakeInt64From2xInt32(NvGpuEngineP2mfReg.DstAddress);

            int LineLengthIn = ReadRegister(NvGpuEngineP2mfReg.LineLengthIn);

            DataBuffer = null;

            Gpu.Fifo.Step();

            for (int Offset = 0; Offset < LineLengthIn; Offset += 4)
            {
                Vmm.WriteInt32(DstAddress + Offset, DataBuffer[Offset >> 2]);
            }
        }

        private void PushData(NvGpuVmm Vmm, NvGpuPBEntry PBEntry)
        {
            DataBuffer = PBEntry.Arguments;
        }

        private long MakeInt64From2xInt32(NvGpuEngineP2mfReg Reg)
        {
            return
                (long)Registers[(int)Reg + 0] << 32 |
                (uint)Registers[(int)Reg + 1];
        }

        private void WriteRegister(NvGpuPBEntry PBEntry)
        {
            int ArgsCount = PBEntry.Arguments.Count;

            if (ArgsCount > 0)
            {
                Registers[PBEntry.Method] = PBEntry.Arguments[ArgsCount - 1];
            }
        }

        private int ReadRegister(NvGpuEngineP2mfReg Reg)
        {
            return Registers[(int)Reg];
        }

        private void WriteRegister(NvGpuEngineP2mfReg Reg, int Value)
        {
            Registers[(int)Reg] = Value;
        }
    }
}