using Ryujinx.Graphics.Gal;
using Ryujinx.Graphics.Memory;
using Ryujinx.Graphics.Texture;
using System;

namespace Ryujinx.Graphics
{
    public class NvGpuEngine2d : INvGpuEngine
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

        public NvGpuEngine2d(NvGpu Gpu)
        {
            this.Gpu = Gpu;

            Registers = new int[0x238];
        }

        public void CallMethod(NvGpuVmm Vmm, NvGpuPBEntry PBEntry)
        {
            WriteRegister(PBEntry);

            if ((NvGpuEngine2dReg)PBEntry.Method == NvGpuEngine2dReg.BlitSrcYInt)
            {
                TextureCopy(Vmm);
            }
        }

        private void TextureCopy(NvGpuVmm Vmm)
        {
            CopyOperation Operation = (CopyOperation)ReadRegister(NvGpuEngine2dReg.CopyOperation);

            int  SrcFormat = ReadRegister(NvGpuEngine2dReg.SrcFormat);
            bool SrcLinear = ReadRegister(NvGpuEngine2dReg.SrcLinear) != 0;
            int  SrcWidth  = ReadRegister(NvGpuEngine2dReg.SrcWidth);
            int  SrcHeight = ReadRegister(NvGpuEngine2dReg.SrcHeight);
            int  SrcPitch  = ReadRegister(NvGpuEngine2dReg.SrcPitch);
            int  SrcBlkDim = ReadRegister(NvGpuEngine2dReg.SrcBlockDimensions);

            int  DstFormat = ReadRegister(NvGpuEngine2dReg.DstFormat);
            bool DstLinear = ReadRegister(NvGpuEngine2dReg.DstLinear) != 0;
            int  DstWidth  = ReadRegister(NvGpuEngine2dReg.DstWidth);
            int  DstHeight = ReadRegister(NvGpuEngine2dReg.DstHeight);
            int  DstPitch  = ReadRegister(NvGpuEngine2dReg.DstPitch);
            int  DstBlkDim = ReadRegister(NvGpuEngine2dReg.DstBlockDimensions);

            GalImageFormat SrcImgFormat = ImageUtils.ConvertSurface((GalSurfaceFormat)SrcFormat);
            GalImageFormat DstImgFormat = ImageUtils.ConvertSurface((GalSurfaceFormat)DstFormat);

            GalMemoryLayout SrcLayout = GetLayout(SrcLinear);
            GalMemoryLayout DstLayout = GetLayout(DstLinear);

            int SrcBlockHeight = 1 << ((SrcBlkDim >> 4) & 0xf);
            int DstBlockHeight = 1 << ((DstBlkDim >> 4) & 0xf);

            long SrcAddress = MakeInt64From2xInt32(NvGpuEngine2dReg.SrcAddress);
            long DstAddress = MakeInt64From2xInt32(NvGpuEngine2dReg.DstAddress);

            long SrcKey = Vmm.GetPhysicalAddress(SrcAddress);
            long DstKey = Vmm.GetPhysicalAddress(DstAddress);

            GalImage SrcTexture = new GalImage(
                SrcWidth,
                SrcHeight, 1,
                SrcBlockHeight,
                SrcLayout,
                SrcImgFormat);

            GalImage DstTexture = new GalImage(
                DstWidth,
                DstHeight, 1,
                DstBlockHeight,
                DstLayout,
                DstImgFormat);

            Gpu.ResourceManager.SendTexture(Vmm, SrcKey, SrcTexture);
            Gpu.ResourceManager.SendTexture(Vmm, DstKey, DstTexture);

            int Width  = Math.Min(SrcWidth,  DstWidth);
            int Height = Math.Min(SrcHeight, DstHeight);

            Gpu.Renderer.RenderTarget.Copy(
                SrcKey,
                DstKey,
                0,
                0,
                Width,
                Height,
                0,
                0,
                Width,
                Height);
        }

        private static GalMemoryLayout GetLayout(bool Linear)
        {
            return Linear
                ? GalMemoryLayout.Pitch
                : GalMemoryLayout.BlockLinear;
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
    }
}