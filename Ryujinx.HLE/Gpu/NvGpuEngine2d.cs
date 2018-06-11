using Ryujinx.Graphics.Gal;
using System.Collections.Generic;

namespace Ryujinx.HLE.Gpu
{
    class NvGpuEngine2d : INvGpuEngine
    {
        private enum CopyOperation
        {
            SrcCopyAnd,
            RopAnd,
            Blend,
            SrcCopy,
            Rop,
            SrcCopyPremult,
            BlendPremult
        }

        public int[] Registers { get; private set; }

        private NvGpu Gpu;

        private Dictionary<int, NvGpuMethod> Methods;

        public NvGpuEngine2d(NvGpu Gpu)
        {
            this.Gpu = Gpu;

            Registers = new int[0xe00];

            Methods = new Dictionary<int, NvGpuMethod>();

            void AddMethod(int Meth, int Count, int Stride, NvGpuMethod Method)
            {
                while (Count-- > 0)
                {
                    Methods.Add(Meth, Method);

                    Meth += Stride;
                }
            }

            AddMethod(0xb5, 1, 1, TextureCopy);
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

        private void TextureCopy(NvGpuVmm Vmm, NvGpuPBEntry PBEntry)
        {
            CopyOperation Operation = (CopyOperation)ReadRegister(NvGpuEngine2dReg.CopyOperation);

            bool SrcLinear = ReadRegister(NvGpuEngine2dReg.SrcLinear) != 0;
            int  SrcWidth  = ReadRegister(NvGpuEngine2dReg.SrcWidth);
            int  SrcHeight = ReadRegister(NvGpuEngine2dReg.SrcHeight);

            bool DstLinear = ReadRegister(NvGpuEngine2dReg.DstLinear) != 0;
            int  DstWidth  = ReadRegister(NvGpuEngine2dReg.DstWidth);
            int  DstHeight = ReadRegister(NvGpuEngine2dReg.DstHeight);
            int  DstPitch  = ReadRegister(NvGpuEngine2dReg.DstPitch);
            int  DstBlkDim = ReadRegister(NvGpuEngine2dReg.DstBlockDimensions);

            TextureSwizzle DstSwizzle = DstLinear
                ? TextureSwizzle.Pitch
                : TextureSwizzle.BlockLinear;

            int DstBlockHeight = 1 << ((DstBlkDim >> 4) & 0xf);

            long Tag = Vmm.GetPhysicalAddress(MakeInt64From2xInt32(NvGpuEngine2dReg.SrcAddress));

            long SrcAddress = MakeInt64From2xInt32(NvGpuEngine2dReg.SrcAddress);
            long DstAddress = MakeInt64From2xInt32(NvGpuEngine2dReg.DstAddress);

            bool IsFbTexture = Gpu.Engine3d.IsFrameBufferPosition(Tag);

            if (IsFbTexture && DstLinear)
            {
                DstSwizzle = TextureSwizzle.BlockLinear;
            }

            Texture DstTexture = new Texture(
                DstAddress,
                DstWidth,
                DstHeight,
                DstBlockHeight,
                DstBlockHeight,
                DstSwizzle,
                GalTextureFormat.A8B8G8R8);

            if (IsFbTexture)
            {
                //TODO: Change this when the correct frame buffer resolution is used.
                //Currently, the frame buffer size is hardcoded to 1280x720.
                SrcWidth  = 1280;
                SrcHeight = 720;

                Gpu.Renderer.GetFrameBufferData(Tag, (byte[] Buffer) =>
                {
                    CopyTexture(
                        Vmm,
                        DstTexture,
                        Buffer,
                        SrcWidth,
                        SrcHeight);
                });
            }
            else
            {
                long Size = SrcWidth * SrcHeight * 4;

                byte[] Buffer = Vmm.ReadBytes(SrcAddress, Size);

                CopyTexture(
                    Vmm,
                    DstTexture,
                    Buffer,
                    SrcWidth,
                    SrcHeight);
            }
        }

        private void CopyTexture(
            NvGpuVmm Vmm,
            Texture  Texture,
            byte[]   Buffer,
            int      Width,
            int      Height)
        {
            TextureWriter.Write(Vmm, Texture, Buffer, Width, Height);
        }

        private long MakeInt64From2xInt32(NvGpuEngine2dReg Reg)
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

        private int ReadRegister(NvGpuEngine2dReg Reg)
        {
            return Registers[(int)Reg];
        }

        private void WriteRegister(NvGpuEngine2dReg Reg, int Value)
        {
            Registers[(int)Reg] = Value;
        }
    }
}