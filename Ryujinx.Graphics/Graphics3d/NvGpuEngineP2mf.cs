using Ryujinx.Common.Logging;
using Ryujinx.Graphics.Memory;
using Ryujinx.Graphics.Texture;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Graphics3d
{
    class NvGpuEngineP2mf : INvGpuEngine
    {
        public int[] Registers { get; private set; }

        private NvGpu Gpu;

        private Dictionary<int, NvGpuMethod> Methods;

        private int CopyStartX;
        private int CopyStartY;

        private int CopyWidth;
        private int CopyHeight;
        private int CopyGobBlockHeight;

        private long CopyAddress;

        private int CopyOffset;
        private int CopySize;

        private bool CopyLinear;

        private byte[] Buffer;

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

        public void CallMethod(NvGpuVmm Vmm, GpuMethodCall MethCall)
        {
            if (Methods.TryGetValue(MethCall.Method, out NvGpuMethod Method))
            {
                Method(Vmm, MethCall);
            }
            else
            {
                WriteRegister(MethCall);
            }
        }

        private void Execute(NvGpuVmm Vmm, GpuMethodCall MethCall)
        {
            //TODO: Some registers and copy modes are still not implemented.
            int Control = MethCall.Argument;

            long DstAddress = MakeInt64From2xInt32(NvGpuEngineP2mfReg.DstAddress);

            int DstPitch  = ReadRegister(NvGpuEngineP2mfReg.DstPitch);
            int DstBlkDim = ReadRegister(NvGpuEngineP2mfReg.DstBlockDim);

            int DstX = ReadRegister(NvGpuEngineP2mfReg.DstX);
            int DstY = ReadRegister(NvGpuEngineP2mfReg.DstY);

            int DstWidth  = ReadRegister(NvGpuEngineP2mfReg.DstWidth);
            int DstHeight = ReadRegister(NvGpuEngineP2mfReg.DstHeight);

            int LineLengthIn = ReadRegister(NvGpuEngineP2mfReg.LineLengthIn);
            int LineCount    = ReadRegister(NvGpuEngineP2mfReg.LineCount);

            CopyLinear = (Control & 1) != 0;

            CopyGobBlockHeight = 1 << ((DstBlkDim >> 4) & 0xf);

            CopyStartX = DstX;
            CopyStartY = DstY;

            CopyWidth  = DstWidth;
            CopyHeight = DstHeight;

            CopyAddress = DstAddress;

            CopyOffset = 0;
            CopySize   = LineLengthIn * LineCount;

            Buffer = new byte[CopySize];
        }

        private void PushData(NvGpuVmm Vmm, GpuMethodCall MethCall)
        {
            if (Buffer == null)
            {
                return;
            }

            for (int Shift = 0; Shift < 32 && CopyOffset < CopySize; Shift += 8, CopyOffset++)
            {
                Buffer[CopyOffset] = (byte)(MethCall.Argument >> Shift);
            }

            if (MethCall.IsLastCall)
            {
                if (CopyLinear)
                {
                    Vmm.WriteBytes(CopyAddress, Buffer);
                }
                else
                {
                    BlockLinearSwizzle Swizzle = new BlockLinearSwizzle(
                        CopyWidth,
                        CopyHeight, 1,
                        CopyGobBlockHeight, 1, 1);

                    int SrcOffset = 0;

                    for (int Y = CopyStartY; Y < CopyHeight && SrcOffset < CopySize; Y++)
                    for (int X = CopyStartX; X < CopyWidth  && SrcOffset < CopySize; X++)
                    {
                        int DstOffset = Swizzle.GetSwizzleOffset(X, Y, 0);

                        Vmm.WriteByte(CopyAddress + DstOffset, Buffer[SrcOffset++]);
                    }
                }

                Buffer = null;
            }
        }

        private long MakeInt64From2xInt32(NvGpuEngineP2mfReg Reg)
        {
            return
                (long)Registers[(int)Reg + 0] << 32 |
                (uint)Registers[(int)Reg + 1];
        }

        private void WriteRegister(GpuMethodCall MethCall)
        {
            Registers[MethCall.Method] = MethCall.Argument;
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