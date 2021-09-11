using Ryujinx.Graphics.Device;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.Engine.Types;
using Ryujinx.Graphics.Gpu.Image;
using Ryujinx.Graphics.Texture;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Ryujinx.Graphics.Gpu.Engine.Twod
{
    /// <summary>
    /// Represents a 2D engine class.
    /// </summary>
    class TwodClass : IDeviceState
    {
        private readonly GpuChannel _channel;
        private readonly DeviceState<TwodClassState> _state;

        /// <summary>
        /// Creates a new instance of the 2D engine class.
        /// </summary>
        /// <param name="channel">The channel that will make use of the engine</param>
        public TwodClass(GpuChannel channel)
        {
            _channel = channel;
            _state = new DeviceState<TwodClassState>(new Dictionary<string, RwCallback>
            {
                { nameof(TwodClassState.PixelsFromMemorySrcY0Int), new RwCallback(PixelsFromMemorySrcY0Int, null) }
            });
        }

        /// <summary>
        /// Reads data from the class registers.
        /// </summary>
        /// <param name="offset">Register byte offset</param>
        /// <returns>Data at the specified offset</returns>
        public int Read(int offset) => _state.Read(offset);

        /// <summary>
        /// Writes data to the class registers.
        /// </summary>
        /// <param name="offset">Register byte offset</param>
        /// <param name="data">Data to be written</param>
        public void Write(int offset, int data) => _state.Write(offset, data);

        /// <summary>
        /// Performs the blit operation, triggered by the register write.
        /// </summary>
        /// <param name="argument">Method call argument</param>
        private void PixelsFromMemorySrcY0Int(int argument)
        {
            var memoryManager = _channel.MemoryManager;

            var dstCopyTexture = Unsafe.As<uint, TwodTexture>(ref _state.State.SetDstFormat);
            var srcCopyTexture = Unsafe.As<uint, TwodTexture>(ref _state.State.SetSrcFormat);

            long srcX = ((long)_state.State.SetPixelsFromMemorySrcX0Int << 32) | (long)(ulong)_state.State.SetPixelsFromMemorySrcX0Frac;
            long srcY = ((long)_state.State.PixelsFromMemorySrcY0Int << 32) | (long)(ulong)_state.State.SetPixelsFromMemorySrcY0Frac;

            long duDx = ((long)_state.State.SetPixelsFromMemoryDuDxInt << 32) | (long)(ulong)_state.State.SetPixelsFromMemoryDuDxFrac;
            long dvDy = ((long)_state.State.SetPixelsFromMemoryDvDyInt << 32) | (long)(ulong)_state.State.SetPixelsFromMemoryDvDyFrac;

            bool originCorner = _state.State.SetPixelsFromMemorySampleModeOrigin == SetPixelsFromMemorySampleModeOrigin.Corner;

            if (originCorner)
            {
                // If the origin is corner, it is assumed that the guest API
                // is manually centering the origin by adding a offset to the
                // source region X/Y coordinates.
                // Here we attempt to remove such offset to ensure we have the correct region.
                // The offset is calculated as FactorXY / 2.0, where FactorXY = SrcXY / DstXY,
                // so we do the same here by dividing the fixed point value by 2, while
                // throwing away the fractional part to avoid rounding errors.
                srcX -= (duDx >> 33) << 32;
                srcY -= (dvDy >> 33) << 32;
            }

            int srcX1 = (int)(srcX >> 32);
            int srcY1 = (int)(srcY >> 32);

            int srcX2 = srcX1 + (int)((duDx * _state.State.SetPixelsFromMemoryDstWidth + uint.MaxValue) >> 32);
            int srcY2 = srcY1 + (int)((dvDy * _state.State.SetPixelsFromMemoryDstHeight + uint.MaxValue) >> 32);

            int dstX1 = (int)_state.State.SetPixelsFromMemoryDstX0;
            int dstY1 = (int)_state.State.SetPixelsFromMemoryDstY0;

            int dstX2 = dstX1 + (int)_state.State.SetPixelsFromMemoryDstWidth;
            int dstY2 = dstY1 + (int)_state.State.SetPixelsFromMemoryDstHeight;

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

            var srcTexture = memoryManager.Physical.TextureCache.FindOrCreateTexture(
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

            memoryManager.Physical.TextureCache.Lift(srcTexture);

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

            var dstTexture = memoryManager.Physical.TextureCache.FindOrCreateTexture(
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

            bool linearFilter = _state.State.SetPixelsFromMemorySampleModeFilter == SetPixelsFromMemorySampleModeFilter.Bilinear;

            srcTexture.HostTexture.CopyTo(dstTexture.HostTexture, srcRegion, dstRegion, linearFilter);

            dstTexture.SignalModified();
        }
    }
}
