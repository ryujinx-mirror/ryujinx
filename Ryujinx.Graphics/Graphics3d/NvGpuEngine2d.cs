using Ryujinx.Graphics.Gal;
using Ryujinx.Graphics.Memory;
using Ryujinx.Graphics.Texture;

namespace Ryujinx.Graphics.Graphics3d
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

        private NvGpu _gpu;

        public NvGpuEngine2d(NvGpu gpu)
        {
            _gpu = gpu;

            Registers = new int[0x238];
        }

        public void CallMethod(NvGpuVmm vmm, GpuMethodCall methCall)
        {
            WriteRegister(methCall);

            if ((NvGpuEngine2dReg)methCall.Method == NvGpuEngine2dReg.BlitSrcYInt)
            {
                TextureCopy(vmm);
            }
        }

        private void TextureCopy(NvGpuVmm vmm)
        {
            CopyOperation operation = (CopyOperation)ReadRegister(NvGpuEngine2dReg.CopyOperation);

            int  dstFormat = ReadRegister(NvGpuEngine2dReg.DstFormat);
            bool dstLinear = ReadRegister(NvGpuEngine2dReg.DstLinear) != 0;
            int  dstWidth  = ReadRegister(NvGpuEngine2dReg.DstWidth);
            int  dstHeight = ReadRegister(NvGpuEngine2dReg.DstHeight);
            int  dstDepth  = ReadRegister(NvGpuEngine2dReg.DstDepth);
            int  dstLayer  = ReadRegister(NvGpuEngine2dReg.DstLayer);
            int  dstPitch  = ReadRegister(NvGpuEngine2dReg.DstPitch);
            int  dstBlkDim = ReadRegister(NvGpuEngine2dReg.DstBlockDimensions);

            int  srcFormat = ReadRegister(NvGpuEngine2dReg.SrcFormat);
            bool srcLinear = ReadRegister(NvGpuEngine2dReg.SrcLinear) != 0;
            int  srcWidth  = ReadRegister(NvGpuEngine2dReg.SrcWidth);
            int  srcHeight = ReadRegister(NvGpuEngine2dReg.SrcHeight);
            int  srcDepth  = ReadRegister(NvGpuEngine2dReg.SrcDepth);
            int  srcLayer  = ReadRegister(NvGpuEngine2dReg.SrcLayer);
            int  srcPitch  = ReadRegister(NvGpuEngine2dReg.SrcPitch);
            int  srcBlkDim = ReadRegister(NvGpuEngine2dReg.SrcBlockDimensions);

            int dstBlitX = ReadRegister(NvGpuEngine2dReg.BlitDstX);
            int dstBlitY = ReadRegister(NvGpuEngine2dReg.BlitDstY);
            int dstBlitW = ReadRegister(NvGpuEngine2dReg.BlitDstW);
            int dstBlitH = ReadRegister(NvGpuEngine2dReg.BlitDstH);

            long blitDuDx = ReadRegisterFixed1_31_32(NvGpuEngine2dReg.BlitDuDxFract);
            long blitDvDy = ReadRegisterFixed1_31_32(NvGpuEngine2dReg.BlitDvDyFract);

            long srcBlitX = ReadRegisterFixed1_31_32(NvGpuEngine2dReg.BlitSrcXFract);
            long srcBlitY = ReadRegisterFixed1_31_32(NvGpuEngine2dReg.BlitSrcYFract);

            GalImageFormat srcImgFormat = ImageUtils.ConvertSurface((GalSurfaceFormat)srcFormat);
            GalImageFormat dstImgFormat = ImageUtils.ConvertSurface((GalSurfaceFormat)dstFormat);

            GalMemoryLayout srcLayout = GetLayout(srcLinear);
            GalMemoryLayout dstLayout = GetLayout(dstLinear);

            int srcBlockHeight = 1 << ((srcBlkDim >> 4) & 0xf);
            int dstBlockHeight = 1 << ((dstBlkDim >> 4) & 0xf);

            long srcAddress = MakeInt64From2xInt32(NvGpuEngine2dReg.SrcAddress);
            long dstAddress = MakeInt64From2xInt32(NvGpuEngine2dReg.DstAddress);

            long srcKey = vmm.GetPhysicalAddress(srcAddress);
            long dstKey = vmm.GetPhysicalAddress(dstAddress);

            bool isSrcLayered = false;
            bool isDstLayered = false;

            GalTextureTarget srcTarget = GalTextureTarget.TwoD;

            if (srcDepth != 0)
            {
                srcTarget = GalTextureTarget.TwoDArray;
                srcDepth++;
                isSrcLayered = true;
            }
            else
            {
                srcDepth = 1;
            }

            GalTextureTarget dstTarget = GalTextureTarget.TwoD;

            if (dstDepth != 0)
            {
                dstTarget = GalTextureTarget.TwoDArray;
                dstDepth++;
                isDstLayered = true;
            }
            else
            {
                dstDepth = 1;
            }

            GalImage srcTexture = new GalImage(
                srcWidth,
                srcHeight,
                1, srcDepth, 1,
                srcBlockHeight, 1,
                srcLayout,
                srcImgFormat,
                srcTarget);

            GalImage dstTexture = new GalImage(
                dstWidth,
                dstHeight,
                1, dstDepth, 1,
                dstBlockHeight, 1,
                dstLayout,
                dstImgFormat,
                dstTarget);

            srcTexture.Pitch = srcPitch;
            dstTexture.Pitch = dstPitch;

            long GetLayerOffset(GalImage image, int layer)
            {
                int targetMipLevel = image.MaxMipmapLevel <= 1 ? 1 : image.MaxMipmapLevel - 1;
                return ImageUtils.GetLayerOffset(image, targetMipLevel) * layer;
            }

            int srcLayerIndex = -1;

            if (isSrcLayered && _gpu.ResourceManager.TryGetTextureLayer(srcKey, out srcLayerIndex) && srcLayerIndex != 0)
            {
                srcKey = srcKey - GetLayerOffset(srcTexture, srcLayerIndex);
            }

            int dstLayerIndex = -1;

            if (isDstLayered && _gpu.ResourceManager.TryGetTextureLayer(dstKey, out dstLayerIndex) && dstLayerIndex != 0)
            {
                dstKey = dstKey - GetLayerOffset(dstTexture, dstLayerIndex);
            }

            _gpu.ResourceManager.SendTexture(vmm, srcKey, srcTexture);
            _gpu.ResourceManager.SendTexture(vmm, dstKey, dstTexture);

            if (isSrcLayered && srcLayerIndex == -1)
            {
                for (int layer = 0; layer < srcTexture.LayerCount; layer++)
                {
                    _gpu.ResourceManager.SetTextureArrayLayer(srcKey + GetLayerOffset(srcTexture, layer), layer);
                }

                srcLayerIndex = 0;
            }

            if (isDstLayered && dstLayerIndex == -1)
            {
                for (int layer = 0; layer < dstTexture.LayerCount; layer++)
                {
                    _gpu.ResourceManager.SetTextureArrayLayer(dstKey + GetLayerOffset(dstTexture, layer), layer);
                }

                dstLayerIndex = 0;
            }

            int srcBlitX1 = (int)(srcBlitX >> 32);
            int srcBlitY1 = (int)(srcBlitY >> 32);

            int srcBlitX2 = (int)(srcBlitX + dstBlitW * blitDuDx >> 32);
            int srcBlitY2 = (int)(srcBlitY + dstBlitH * blitDvDy >> 32);

            _gpu.Renderer.RenderTarget.Copy(
                srcTexture,
                dstTexture,
                srcKey,
                dstKey,
                srcLayerIndex,
                dstLayerIndex,
                srcBlitX1,
                srcBlitY1,
                srcBlitX2,
                srcBlitY2,
                dstBlitX,
                dstBlitY,
                dstBlitX + dstBlitW,
                dstBlitY + dstBlitH);

            //Do a guest side copy aswell. This is necessary when
            //the texture is modified by the guest, however it doesn't
            //work when resources that the gpu can write to are copied,
            //like framebuffers.

            // FIXME: SUPPORT MULTILAYER CORRECTLY HERE (this will cause weird stuffs on the first layer)
            ImageUtils.CopyTexture(
                vmm,
                srcTexture,
                dstTexture,
                srcAddress,
                dstAddress,
                srcBlitX1,
                srcBlitY1,
                dstBlitX,
                dstBlitY,
                dstBlitW,
                dstBlitH);

            vmm.IsRegionModified(dstKey, ImageUtils.GetSize(dstTexture), NvGpuBufferType.Texture);
        }

        private static GalMemoryLayout GetLayout(bool linear)
        {
            return linear
                ? GalMemoryLayout.Pitch
                : GalMemoryLayout.BlockLinear;
        }

        private long MakeInt64From2xInt32(NvGpuEngine2dReg reg)
        {
            return
                (long)Registers[(int)reg + 0] << 32 |
                (uint)Registers[(int)reg + 1];
        }

        private void WriteRegister(GpuMethodCall methCall)
        {
            Registers[methCall.Method] = methCall.Argument;
        }

        private long ReadRegisterFixed1_31_32(NvGpuEngine2dReg reg)
        {
            long low  = (uint)ReadRegister(reg + 0);
            long high = (uint)ReadRegister(reg + 1);

            return low | (high << 32);
        }

        private int ReadRegister(NvGpuEngine2dReg reg)
        {
            return Registers[(int)reg];
        }
    }
}