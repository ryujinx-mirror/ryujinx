using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.Image;
using Ryujinx.Graphics.Gpu.State;
using Ryujinx.Graphics.Texture;
using System;

namespace Ryujinx.Graphics.Gpu.Engine
{
    using Texture = Image.Texture;

    partial class Methods
    {
        /// <summary>
        /// Performs a texture to texture copy.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        /// <param name="argument">Method call argument</param>
        private void CopyTexture(GpuState state, int argument)
        {
            var memoryManager = state.Channel.MemoryManager;

            var dstCopyTexture = state.Get<CopyTexture>(MethodOffset.CopyDstTexture);
            var srcCopyTexture = state.Get<CopyTexture>(MethodOffset.CopySrcTexture);

            var region = state.Get<CopyRegion>(MethodOffset.CopyRegion);

            var control = state.Get<CopyTextureControl>(MethodOffset.CopyTextureControl);

            bool originCorner = control.UnpackOriginCorner();

            long srcX = region.SrcXF;
            long srcY = region.SrcYF;

            if (originCorner)
            {
                // If the origin is corner, it is assumed that the guest API
                // is manually centering the origin by adding a offset to the
                // source region X/Y coordinates.
                // Here we attempt to remove such offset to ensure we have the correct region.
                // The offset is calculated as FactorXY / 2.0, where FactorXY = SrcXY / DstXY,
                // so we do the same here by dividing the fixed point value by 2, while
                // throwing away the fractional part to avoid rounding errors.
                srcX -= (region.SrcWidthRF >> 33) << 32;
                srcY -= (region.SrcHeightRF >> 33) << 32;
            }

            int srcX1 = (int)(srcX >> 32);
            int srcY1 = (int)(srcY >> 32);

            int srcX2 = srcX1 + (int)((region.SrcWidthRF * region.DstWidth + uint.MaxValue) >> 32);
            int srcY2 = srcY1 + (int)((region.SrcHeightRF * region.DstHeight + uint.MaxValue) >> 32);

            int dstX1 = region.DstX;
            int dstY1 = region.DstY;

            int dstX2 = region.DstX + region.DstWidth;
            int dstY2 = region.DstY + region.DstHeight;

            // The source and destination textures should at least be as big as the region being requested.
            // The hints will only resize within alignment constraints, so out of bound copies won't resize in most cases.
            var srcHint = new Size(srcX2, srcY2, 1);
            var dstHint = new Size(dstX2, dstY2, 1);

            var srcCopyTextureFormat = srcCopyTexture.Format.Convert();

            int srcWidthAligned = srcCopyTexture.Stride / srcCopyTextureFormat.BytesPerPixel;

            ulong offset = 0;

            // For an out of bounds copy, we must ensure that the copy wraps to the next line,
            // so for a copy from a 64x64 texture, in the region [32, 96[, there are 32 pixels that are
            // outside the bounds of the texture. We fill the destination with the first 32 pixels
            // of the next line on the source texture.
            // This can be done by simply adding an offset to the texture address, so that the initial
            // gap is skipped and the copy is inside bounds again.
            // This is required by the proprietary guest OpenGL driver.
            if (srcCopyTexture.LinearLayout && srcCopyTexture.Width == srcX2 && srcX2 > srcWidthAligned && srcX1 > 0)
            {
                offset = (ulong)(srcX1 * srcCopyTextureFormat.BytesPerPixel);
                srcCopyTexture.Width -= srcX1;
                srcX2 -= srcX1;
                srcX1 = 0;
            }

            Texture srcTexture = memoryManager.Physical.TextureCache.FindOrCreateTexture(
                memoryManager,
                srcCopyTexture,
                offset,
                srcCopyTextureFormat,
                true,
                srcHint);

            if (srcTexture == null)
            {
                return;
            }

            // When the source texture that was found has a depth format,
            // we must enforce the target texture also has a depth format,
            // as copies between depth and color formats are not allowed.
            FormatInfo dstCopyTextureFormat;

            if (srcTexture.Format.IsDepthOrStencil())
            {
                dstCopyTextureFormat = srcTexture.Info.FormatInfo;
            }
            else
            {
                dstCopyTextureFormat = dstCopyTexture.Format.Convert();
            }

            Texture dstTexture = memoryManager.Physical.TextureCache.FindOrCreateTexture(
                memoryManager,
                dstCopyTexture,
                0,
                dstCopyTextureFormat,
                srcTexture.ScaleMode == TextureScaleMode.Scaled,
                dstHint);

            if (dstTexture == null)
            {
                return;
            }

            float scale = srcTexture.ScaleFactor;
            float dstScale = dstTexture.ScaleFactor;

            Extents2D srcRegion = new Extents2D(
                (int)Math.Ceiling(scale * (srcX1 / srcTexture.Info.SamplesInX)),
                (int)Math.Ceiling(scale * (srcY1 / srcTexture.Info.SamplesInY)),
                (int)Math.Ceiling(scale * (srcX2 / srcTexture.Info.SamplesInX)),
                (int)Math.Ceiling(scale * (srcY2 / srcTexture.Info.SamplesInY)));

            Extents2D dstRegion = new Extents2D(
                (int)Math.Ceiling(dstScale * (dstX1 / dstTexture.Info.SamplesInX)),
                (int)Math.Ceiling(dstScale * (dstY1 / dstTexture.Info.SamplesInY)),
                (int)Math.Ceiling(dstScale * (dstX2 / dstTexture.Info.SamplesInX)),
                (int)Math.Ceiling(dstScale * (dstY2 / dstTexture.Info.SamplesInY)));

            bool linearFilter = control.UnpackLinearFilter();

            srcTexture.HostTexture.CopyTo(dstTexture.HostTexture, srcRegion, dstRegion, linearFilter);

            dstTexture.SignalModified();
        }
    }
}