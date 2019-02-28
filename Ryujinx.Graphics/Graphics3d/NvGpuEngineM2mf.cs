using Ryujinx.Common.Logging;
using Ryujinx.Graphics.Memory;
using Ryujinx.Graphics.Texture;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Graphics3d
{
    class NvGpuEngineM2mf : INvGpuEngine
    {
        public int[] Registers { get; private set; }

        private NvGpu Gpu;

        private Dictionary<int, NvGpuMethod> Methods;

        public NvGpuEngineM2mf(NvGpu Gpu)
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

            bool SrcLinear = ((Control >> 7) & 1) != 0;
            bool DstLinear = ((Control >> 8) & 1) != 0;
            bool Copy2d    = ((Control >> 9) & 1) != 0;

            long SrcAddress = MakeInt64From2xInt32(NvGpuEngineM2mfReg.SrcAddress);
            long DstAddress = MakeInt64From2xInt32(NvGpuEngineM2mfReg.DstAddress);

            int SrcPitch = ReadRegister(NvGpuEngineM2mfReg.SrcPitch);
            int DstPitch = ReadRegister(NvGpuEngineM2mfReg.DstPitch);

            int XCount = ReadRegister(NvGpuEngineM2mfReg.XCount);
            int YCount = ReadRegister(NvGpuEngineM2mfReg.YCount);

            int Swizzle = ReadRegister(NvGpuEngineM2mfReg.Swizzle);

            int DstBlkDim = ReadRegister(NvGpuEngineM2mfReg.DstBlkDim);
            int DstSizeX  = ReadRegister(NvGpuEngineM2mfReg.DstSizeX);
            int DstSizeY  = ReadRegister(NvGpuEngineM2mfReg.DstSizeY);
            int DstSizeZ  = ReadRegister(NvGpuEngineM2mfReg.DstSizeZ);
            int DstPosXY  = ReadRegister(NvGpuEngineM2mfReg.DstPosXY);
            int DstPosZ   = ReadRegister(NvGpuEngineM2mfReg.DstPosZ);

            int SrcBlkDim = ReadRegister(NvGpuEngineM2mfReg.SrcBlkDim);
            int SrcSizeX  = ReadRegister(NvGpuEngineM2mfReg.SrcSizeX);
            int SrcSizeY  = ReadRegister(NvGpuEngineM2mfReg.SrcSizeY);
            int SrcSizeZ  = ReadRegister(NvGpuEngineM2mfReg.SrcSizeZ);
            int SrcPosXY  = ReadRegister(NvGpuEngineM2mfReg.SrcPosXY);
            int SrcPosZ   = ReadRegister(NvGpuEngineM2mfReg.SrcPosZ);

            int SrcCpp = ((Swizzle >> 20) & 7) + 1;
            int DstCpp = ((Swizzle >> 24) & 7) + 1;

            int DstPosX = (DstPosXY >>  0) & 0xffff;
            int DstPosY = (DstPosXY >> 16) & 0xffff;

            int SrcPosX = (SrcPosXY >>  0) & 0xffff;
            int SrcPosY = (SrcPosXY >> 16) & 0xffff;

            int SrcBlockHeight = 1 << ((SrcBlkDim >> 4) & 0xf);
            int DstBlockHeight = 1 << ((DstBlkDim >> 4) & 0xf);

            long SrcPA = Vmm.GetPhysicalAddress(SrcAddress);
            long DstPA = Vmm.GetPhysicalAddress(DstAddress);

            if (Copy2d)
            {
                if (SrcLinear)
                {
                    SrcPosX = SrcPosY = SrcPosZ = 0;
                }

                if (DstLinear)
                {
                    DstPosX = DstPosY = DstPosZ = 0;
                }

                if (SrcLinear && DstLinear)
                {
                    for (int Y = 0; Y < YCount; Y++)
                    {
                        int SrcOffset = (SrcPosY + Y) * SrcPitch + SrcPosX * SrcCpp;
                        int DstOffset = (DstPosY + Y) * DstPitch + DstPosX * DstCpp;

                        long Src = SrcPA + (uint)SrcOffset;
                        long Dst = DstPA + (uint)DstOffset;

                        Vmm.Memory.CopyBytes(Src, Dst, XCount * SrcCpp);
                    }
                }
                else
                {
                    ISwizzle SrcSwizzle;

                    if (SrcLinear)
                    {
                        SrcSwizzle = new LinearSwizzle(SrcPitch, SrcCpp, SrcSizeX, SrcSizeY);
                    }
                    else
                    {
                        SrcSwizzle = new BlockLinearSwizzle(
                            SrcSizeX,
                            SrcSizeY, 1,
                            SrcBlockHeight, 1,
                            SrcCpp);
                    }

                    ISwizzle DstSwizzle;

                    if (DstLinear)
                    {
                        DstSwizzle = new LinearSwizzle(DstPitch, DstCpp, SrcSizeX, SrcSizeY);
                    }
                    else
                    {
                        DstSwizzle = new BlockLinearSwizzle(
                            DstSizeX,
                            DstSizeY, 1,
                            DstBlockHeight, 1,
                            DstCpp);
                    }

                    for (int Y = 0; Y < YCount; Y++)
                    for (int X = 0; X < XCount; X++)
                    {
                        int SrcOffset = SrcSwizzle.GetSwizzleOffset(SrcPosX + X, SrcPosY + Y, 0);
                        int DstOffset = DstSwizzle.GetSwizzleOffset(DstPosX + X, DstPosY + Y, 0);

                        long Src = SrcPA + (uint)SrcOffset;
                        long Dst = DstPA + (uint)DstOffset;

                        Vmm.Memory.CopyBytes(Src, Dst, SrcCpp);
                    }
                }
            }
            else
            {
                Vmm.Memory.CopyBytes(SrcPA, DstPA, XCount);
            }
        }

        private long MakeInt64From2xInt32(NvGpuEngineM2mfReg Reg)
        {
            return
                (long)Registers[(int)Reg + 0] << 32 |
                (uint)Registers[(int)Reg + 1];
        }

        private void WriteRegister(GpuMethodCall MethCall)
        {
            Registers[MethCall.Method] = MethCall.Argument;
        }

        private int ReadRegister(NvGpuEngineM2mfReg Reg)
        {
            return Registers[(int)Reg];
        }

        private void WriteRegister(NvGpuEngineM2mfReg Reg, int Value)
        {
            Registers[(int)Reg] = Value;
        }
    }
}