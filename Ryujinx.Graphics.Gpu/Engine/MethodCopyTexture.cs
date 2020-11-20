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
            var dstCopyTexture = state.Get<CopyTexture>(MethodOffset.CopyDstTexture);
            var srcCopyTexture = state.Get<CopyTexture>(MethodOffset.CopySrcTexture);

            var region = state.Get<CopyRegion>(MethodOffset.CopyRegion);

            var control = state.Get<CopyTextureControl>(MethodOffset.CopyTextureControl);

            int srcX1 = (int)(region.SrcXF >> 32);
            int srcY1 = (int)(region.SrcYF >> 32);

            int srcX2 = (int)((region.SrcXF + region.SrcWidthRF * region.DstWidth) >> 32);
            int srcY2 = (int)((region.SrcYF + region.SrcHeightRF * region.DstHeight) >> 32);

            int dstX1 = region.DstX;
            int dstY1 = region.DstY;

            int dstX2 = region.DstX + region.DstWidth;
            int dstY2 = region.DstY + region.DstHeight;

            // The source and destination textures should at least be as big as the region being requested.
            // The hints will only resize within alignment constraints, so out of bound copies won't resize in most cases.
            var srcHint = new Size(srcX2, srcY2, 1);
            var dstHint = new Size(dstX2, dstY2, 1);

            var srcCopyTextureFormat = srcCopyTexture.Format.Convert();

            Texture srcTexture = TextureManager.FindOrCreateTexture(srcCopyTexture, srcCopyTextureFormat, true, srcHint);

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

            Texture dstTexture = TextureManager.FindOrCreateTexture(dstCopyTexture, dstCopyTextureFormat, srcTexture.ScaleMode == TextureScaleMode.Scaled, dstHint);

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

            // For an out of bounds copy, we must ensure that the copy wraps to the next line,
            // so for a copy from a 64x64 texture, in the region [32, 96[, there are 32 pixels that are
            // outside the bounds of the texture. We fill the destination with the first 32 pixels
            // of the next line on the source texture.
            // This can be emulated with 2 copies (the first copy handles the region inside the bounds,
            // the second handles the region outside of the bounds).
            // We must also extend the source texture by one line to ensure we can wrap on the last line.
            // This is required by the (guest) OpenGL driver.
            if (srcX2 / srcTexture.Info.SamplesInX > srcTexture.Info.Width)
            {
                srcCopyTexture.Height++;

                srcTexture = TextureManager.FindOrCreateTexture(srcCopyTexture, srcCopyTextureFormat, srcTexture.ScaleMode == TextureScaleMode.Scaled, srcHint);
                scale = srcTexture.ScaleFactor;

                srcRegion = new Extents2D(
                    (int)Math.Ceiling(scale * ((srcX1 / srcTexture.Info.SamplesInX) - srcTexture.Info.Width)),
                    (int)Math.Ceiling(scale * ((srcY1 / srcTexture.Info.SamplesInY) + 1)),
                    (int)Math.Ceiling(scale * ((srcX2 / srcTexture.Info.SamplesInX) - srcTexture.Info.Width)),
                    (int)Math.Ceiling(scale * ((srcY2 / srcTexture.Info.SamplesInY) + 1)));

                srcTexture.HostTexture.CopyTo(dstTexture.HostTexture, srcRegion, dstRegion, linearFilter);
            }

            dstTexture.SignalModified();
        }
    }
}