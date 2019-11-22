using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.State;

namespace Ryujinx.Graphics.Gpu.Engine
{
    partial class Methods
    {
        private void CopyTexture(GpuState state, int argument)
        {
            var dstCopyTexture = state.Get<CopyTexture>(MethodOffset.CopyDstTexture);
            var srcCopyTexture = state.Get<CopyTexture>(MethodOffset.CopySrcTexture);

            Image.Texture srcTexture = _textureManager.FindOrCreateTexture(srcCopyTexture);

            if (srcTexture == null)
            {
                return;
            }

            // When the source texture that was found has a depth format,
            // we must enforce the target texture also has a depth format,
            // as copies between depth and color formats are not allowed.
            if (srcTexture.Format == Format.D32Float)
            {
                dstCopyTexture.Format = RtFormat.D32Float;
            }

            Image.Texture dstTexture = _textureManager.FindOrCreateTexture(dstCopyTexture);

            if (dstTexture == null)
            {
                return;
            }

            var control = state.Get<CopyTextureControl>(MethodOffset.CopyTextureControl);

            var region = state.Get<CopyRegion>(MethodOffset.CopyRegion);

            int srcX1 = (int)(region.SrcXF >> 32);
            int srcY1 = (int)(region.SrcYF >> 32);

            int srcX2 = (int)((region.SrcXF + region.SrcWidthRF  * region.DstWidth)  >> 32);
            int srcY2 = (int)((region.SrcYF + region.SrcHeightRF * region.DstHeight) >> 32);

            int dstX1 = region.DstX;
            int dstY1 = region.DstY;

            int dstX2 = region.DstX + region.DstWidth;
            int dstY2 = region.DstY + region.DstHeight;

            Extents2D srcRegion = new Extents2D(
                srcX1 / srcTexture.Info.SamplesInX,
                srcY1 / srcTexture.Info.SamplesInY,
                srcX2 / srcTexture.Info.SamplesInX,
                srcY2 / srcTexture.Info.SamplesInY);

            Extents2D dstRegion = new Extents2D(
                dstX1 / dstTexture.Info.SamplesInX,
                dstY1 / dstTexture.Info.SamplesInY,
                dstX2 / dstTexture.Info.SamplesInX,
                dstY2 / dstTexture.Info.SamplesInY);

            bool linearFilter = control.UnpackLinearFilter();

            srcTexture.HostTexture.CopyTo(dstTexture.HostTexture, srcRegion, dstRegion, linearFilter);

            dstTexture.Modified = true;
        }
    }
}