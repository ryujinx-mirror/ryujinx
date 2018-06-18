using System.Collections.Generic;

namespace Ryujinx.HLE.Gpu
{
    class NvGpuEngineDma : INvGpuEngine
    {
        public int[] Registers { get; private set; }

        private NvGpu Gpu;

        private Dictionary<int, NvGpuMethod> Methods;

        public NvGpuEngineDma(NvGpu Gpu)
        {
            this.Gpu = Gpu;

            Registers = new int[0x1d6];

            Methods = new Dictionary<int, NvGpuMethod>();

            void AddMethod(int Meth, int Count, int Stride, NvGpuMethod Method)
            {
                while (Count-- > 0)
                {
                    Methods.Add(Meth, Method);

                    Meth += Stride;
                }
            }

            AddMethod(0xc0, 1, 1, Execute);
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
            int Control = PBEntry.Arguments[0];

            bool SrcLinear = ((Control >> 7) & 1) != 0;
            bool DstLinear = ((Control >> 8) & 1) != 0;

            long SrcAddress = MakeInt64From2xInt32(NvGpuEngineDmaReg.SrcAddress);
            long DstAddress = MakeInt64From2xInt32(NvGpuEngineDmaReg.DstAddress);

            int SrcPitch = ReadRegister(NvGpuEngineDmaReg.SrcPitch);
            int DstPitch = ReadRegister(NvGpuEngineDmaReg.DstPitch);

            int DstBlkDim = ReadRegister(NvGpuEngineDmaReg.DstBlkDim);
            int DstSizeX  = ReadRegister(NvGpuEngineDmaReg.DstSizeX);
            int DstSizeY  = ReadRegister(NvGpuEngineDmaReg.DstSizeY);
            int DstSizeZ  = ReadRegister(NvGpuEngineDmaReg.DstSizeZ);
            int DstPosXY  = ReadRegister(NvGpuEngineDmaReg.DstPosXY);
            int DstPosZ   = ReadRegister(NvGpuEngineDmaReg.DstPosZ);

            int SrcBlkDim = ReadRegister(NvGpuEngineDmaReg.SrcBlkDim);
            int SrcSizeX  = ReadRegister(NvGpuEngineDmaReg.SrcSizeX);
            int SrcSizeY  = ReadRegister(NvGpuEngineDmaReg.SrcSizeY);
            int SrcSizeZ  = ReadRegister(NvGpuEngineDmaReg.SrcSizeZ);
            int SrcPosXY  = ReadRegister(NvGpuEngineDmaReg.SrcPosXY);
            int SrcPosZ   = ReadRegister(NvGpuEngineDmaReg.SrcPosZ);

            int DstPosX = (DstPosXY >>  0) & 0xffff;
            int DstPosY = (DstPosXY >> 16) & 0xffff;

            int SrcPosX = (SrcPosXY >>  0) & 0xffff;
            int SrcPosY = (SrcPosXY >> 16) & 0xffff;

            int SrcBlockHeight = 1 << ((SrcBlkDim >> 4) & 0xf);
            int DstBlockHeight = 1 << ((DstBlkDim >> 4) & 0xf);

            ISwizzle SrcSwizzle;

            if (SrcLinear)
            {
                SrcSwizzle = new LinearSwizzle(SrcPitch, 1);
            }
            else
            {
                SrcSwizzle = new BlockLinearSwizzle(SrcSizeX, 1, SrcBlockHeight);
            }

            ISwizzle DstSwizzle;

            if (DstLinear)
            {
                DstSwizzle = new LinearSwizzle(DstPitch, 1);
            }
            else
            {
                DstSwizzle = new BlockLinearSwizzle(DstSizeX, 1, DstBlockHeight);
            }

            for (int Y = 0; Y < DstSizeY; Y++)
            for (int X = 0; X < DstSizeX; X++)
            {
                long SrcOffset = SrcAddress + (uint)SrcSwizzle.GetSwizzleOffset(X, Y);
                long DstOffset = DstAddress + (uint)DstSwizzle.GetSwizzleOffset(X, Y);

                Vmm.WriteByte(DstOffset, Vmm.ReadByte(SrcOffset));
            }
        }

        private long MakeInt64From2xInt32(NvGpuEngineDmaReg Reg)
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

        private int ReadRegister(NvGpuEngineDmaReg Reg)
        {
            return Registers[(int)Reg];
        }

        private void WriteRegister(NvGpuEngineDmaReg Reg, int Value)
        {
            Registers[(int)Reg] = Value;
        }
    }
}