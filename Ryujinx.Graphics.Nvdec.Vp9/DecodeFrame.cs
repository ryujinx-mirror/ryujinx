using Ryujinx.Common.Memory;
using Ryujinx.Graphics.Nvdec.Vp9.Common;
using Ryujinx.Graphics.Nvdec.Vp9.Dsp;
using Ryujinx.Graphics.Nvdec.Vp9.Types;
using Ryujinx.Graphics.Video;
using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Mv = Ryujinx.Graphics.Nvdec.Vp9.Types.Mv;

namespace Ryujinx.Graphics.Nvdec.Vp9
{
    static class DecodeFrame
    {
        private static bool ReadIsValid(ArrayPtr<byte> start, int len)
        {
            return len != 0 && len <= start.Length;
        }

        private static void InverseTransformBlockInter(ref MacroBlockD xd, int plane, TxSize txSize, Span<byte> dst, int stride, int eob)
        {
            ref MacroBlockDPlane pd = ref xd.Plane[plane];
            ArrayPtr<int> dqcoeff = pd.DqCoeff;
            Debug.Assert(eob > 0);
            if (xd.CurBuf.HighBd)
            {
                Span<ushort> dst16 = MemoryMarshal.Cast<byte, ushort>(dst);
                if (xd.Lossless)
                {
                    Idct.HighbdIwht4x4Add(dqcoeff.ToSpan(), dst16, stride, eob, xd.Bd);
                }
                else
                {
                    switch (txSize)
                    {
                        case TxSize.Tx4x4:
                            Idct.HighbdIdct4x4Add(dqcoeff.ToSpan(), dst16, stride, eob, xd.Bd);
                            break;
                        case TxSize.Tx8x8:
                            Idct.HighbdIdct8x8Add(dqcoeff.ToSpan(), dst16, stride, eob, xd.Bd);
                            break;
                        case TxSize.Tx16x16:
                            Idct.HighbdIdct16x16Add(dqcoeff.ToSpan(), dst16, stride, eob, xd.Bd);
                            break;
                        case TxSize.Tx32x32:
                            Idct.HighbdIdct32x32Add(dqcoeff.ToSpan(), dst16, stride, eob, xd.Bd);
                            break;
                        default: Debug.Assert(false, "Invalid transform size"); break;
                    }
                }
            }
            else
            {
                if (xd.Lossless)
                {
                    Idct.Iwht4x4Add(dqcoeff.ToSpan(), dst, stride, eob);
                }
                else
                {
                    switch (txSize)
                    {
                        case TxSize.Tx4x4: Idct.Idct4x4Add(dqcoeff.ToSpan(), dst, stride, eob); break;
                        case TxSize.Tx8x8: Idct.Idct8x8Add(dqcoeff.ToSpan(), dst, stride, eob); break;
                        case TxSize.Tx16x16: Idct.Idct16x16Add(dqcoeff.ToSpan(), dst, stride, eob); break;
                        case TxSize.Tx32x32: Idct.Idct32x32Add(dqcoeff.ToSpan(), dst, stride, eob); break;
                        default: Debug.Assert(false, "Invalid transform size"); return;
                    }
                }
            }

            if (eob == 1)
            {
                dqcoeff.ToSpan()[0] = 0;
            }
            else
            {
                if (txSize <= TxSize.Tx16x16 && eob <= 10)
                {
                    dqcoeff.ToSpan().Slice(0, 4 * (4 << (int)txSize)).Fill(0);
                }
                else if (txSize == TxSize.Tx32x32 && eob <= 34)
                {
                    dqcoeff.ToSpan().Slice(0, 256).Fill(0);
                }
                else
                {
                    dqcoeff.ToSpan().Slice(0, 16 << ((int)txSize << 1)).Fill(0);
                }
            }
        }

        private static void InverseTransformBlockIntra(
            ref MacroBlockD xd,
            int plane,
            TxType txType,
            TxSize txSize,
            Span<byte> dst,
            int stride,
            int eob)
        {
            ref MacroBlockDPlane pd = ref xd.Plane[plane];
            ArrayPtr<int> dqcoeff = pd.DqCoeff;
            Debug.Assert(eob > 0);
            if (xd.CurBuf.HighBd)
            {
                Span<ushort> dst16 = MemoryMarshal.Cast<byte, ushort>(dst);
                if (xd.Lossless)
                {
                    Idct.HighbdIwht4x4Add(dqcoeff.ToSpan(), dst16, stride, eob, xd.Bd);
                }
                else
                {
                    switch (txSize)
                    {
                        case TxSize.Tx4x4:
                            Idct.HighbdIht4x4Add(txType, dqcoeff.ToSpan(), dst16, stride, eob, xd.Bd);
                            break;
                        case TxSize.Tx8x8:
                            Idct.HighbdIht8x8Add(txType, dqcoeff.ToSpan(), dst16, stride, eob, xd.Bd);
                            break;
                        case TxSize.Tx16x16:
                            Idct.HighbdIht16x16Add(txType, dqcoeff.ToSpan(), dst16, stride, eob, xd.Bd);
                            break;
                        case TxSize.Tx32x32:
                            Idct.HighbdIdct32x32Add(dqcoeff.ToSpan(), dst16, stride, eob, xd.Bd);
                            break;
                        default: Debug.Assert(false, "Invalid transform size"); break;
                    }
                }
            }
            else
            {
                if (xd.Lossless)
                {
                    Idct.Iwht4x4Add(dqcoeff.ToSpan(), dst, stride, eob);
                }
                else
                {
                    switch (txSize)
                    {
                        case TxSize.Tx4x4: Idct.Iht4x4Add(txType, dqcoeff.ToSpan(), dst, stride, eob); break;
                        case TxSize.Tx8x8: Idct.Iht8x8Add(txType, dqcoeff.ToSpan(), dst, stride, eob); break;
                        case TxSize.Tx16x16: Idct.Iht16x16Add(txType, dqcoeff.ToSpan(), dst, stride, eob); break;
                        case TxSize.Tx32x32: Idct.Idct32x32Add(dqcoeff.ToSpan(), dst, stride, eob); break;
                        default: Debug.Assert(false, "Invalid transform size"); return;
                    }
                }
            }

            if (eob == 1)
            {
                dqcoeff.ToSpan()[0] = 0;
            }
            else
            {
                if (txType == TxType.DctDct && txSize <= TxSize.Tx16x16 && eob <= 10)
                {
                    dqcoeff.ToSpan().Slice(0, 4 * (4 << (int)txSize)).Fill(0);
                }
                else if (txSize == TxSize.Tx32x32 && eob <= 34)
                {
                    dqcoeff.ToSpan().Slice(0, 256).Fill(0);
                }
                else
                {
                    dqcoeff.ToSpan().Slice(0, 16 << ((int)txSize << 1)).Fill(0);
                }
            }
        }

        private static unsafe void PredictAndReconstructIntraBlock(
            ref TileWorkerData twd,
            ref ModeInfo mi,
            int plane,
            int row,
            int col,
            TxSize txSize)
        {
            ref MacroBlockD xd = ref twd.Xd;
            ref MacroBlockDPlane pd = ref xd.Plane[plane];
            PredictionMode mode = (plane == 0) ? mi.Mode : mi.UvMode;
            int dstOffset = 4 * row * pd.Dst.Stride + 4 * col;
            byte* dst = &pd.Dst.Buf.ToPointer()[dstOffset];
            Span<byte> dstSpan = pd.Dst.Buf.ToSpan().Slice(dstOffset);

            if (mi.SbType < BlockSize.Block8x8)
            {
                if (plane == 0)
                {
                    mode = xd.Mi[0].Value.Bmi[(row << 1) + col].Mode;
                }
            }

            ReconIntra.PredictIntraBlock(ref xd, pd.N4Wl, txSize, mode, dst, pd.Dst.Stride, dst, pd.Dst.Stride, col, row, plane);

            if (mi.Skip == 0)
            {
                TxType txType =
                    (plane != 0 || xd.Lossless) ? TxType.DctDct : ReconIntra.IntraModeToTxTypeLookup[(int)mode];
                var sc = (plane != 0 || xd.Lossless)
                    ? Luts.Vp9DefaultScanOrders[(int)txSize]
                    : Luts.Vp9ScanOrders[(int)txSize][(int)txType];
                int eob = Detokenize.DecodeBlockTokens(ref twd, plane, sc, col, row, txSize, mi.SegmentId);
                if (eob > 0)
                {
                    InverseTransformBlockIntra(ref xd, plane, txType, txSize, dstSpan, pd.Dst.Stride, eob);
                }
            }
        }

        private static int ReconstructInterBlock(
            ref TileWorkerData twd,
            ref ModeInfo mi,
            int plane,
            int row,
            int col,
            TxSize txSize)
        {
            ref MacroBlockD xd = ref twd.Xd;
            ref MacroBlockDPlane pd = ref xd.Plane[plane];
            var sc = Luts.Vp9DefaultScanOrders[(int)txSize];
            int eob = Detokenize.DecodeBlockTokens(ref twd, plane, sc, col, row, txSize, mi.SegmentId);
            Span<byte> dst = pd.Dst.Buf.ToSpan().Slice(4 * row * pd.Dst.Stride + 4 * col);

            if (eob > 0)
            {
                InverseTransformBlockInter(ref xd, plane, txSize, dst, pd.Dst.Stride, eob);
            }
            return eob;
        }

        private static unsafe void BuildMcBorder(
            byte* src,
            int srcStride,
            byte* dst,
            int dstStride,
            int x,
            int y,
            int bW,
            int bH,
            int w,
            int h)
        {
            // Get a pointer to the start of the real data for this row.
            byte* refRow = src - x - y * srcStride;

            if (y >= h)
            {
                refRow += (h - 1) * srcStride;
            }
            else if (y > 0)
            {
                refRow += y * srcStride;
            }

            do
            {
                int right = 0, copy;
                int left = x < 0 ? -x : 0;

                if (left > bW)
                {
                    left = bW;
                }

                if (x + bW > w)
                {
                    right = x + bW - w;
                }

                if (right > bW)
                {
                    right = bW;
                }

                copy = bW - left - right;

                if (left != 0)
                {
                    MemoryUtil.Fill(dst, refRow[0], left);
                }

                if (copy != 0)
                {
                    MemoryUtil.Copy(dst + left, refRow + x + left, copy);
                }

                if (right != 0)
                {
                    MemoryUtil.Fill(dst + left + copy, refRow[w - 1], right);
                }

                dst += dstStride;
                ++y;

                if (y > 0 && y < h)
                {
                    refRow += srcStride;
                }
            } while (--bH != 0);
        }

        private static unsafe void HighBuildMcBorder(
            byte* src8,
            int srcStride,
            ushort* dst,
            int dstStride,
            int x,
            int y,
            int bW,
            int bH,
            int w,
            int h)
        {
            // Get a pointer to the start of the real data for this row.
            ushort* src = (ushort*)src8;
            ushort* refRow = src - x - y * srcStride;

            if (y >= h)
            {
                refRow += (h - 1) * srcStride;
            }
            else if (y > 0)
            {
                refRow += y * srcStride;
            }

            do
            {
                int right = 0, copy;
                int left = x < 0 ? -x : 0;

                if (left > bW)
                {
                    left = bW;
                }

                if (x + bW > w)
                {
                    right = x + bW - w;
                }

                if (right > bW)
                {
                    right = bW;
                }

                copy = bW - left - right;

                if (left != 0)
                {
                    MemoryUtil.Fill(dst, refRow[0], left);
                }

                if (copy != 0)
                {
                    MemoryUtil.Copy(dst + left, refRow + x + left, copy);
                }

                if (right != 0)
                {
                    MemoryUtil.Fill(dst + left + copy, refRow[w - 1], right);
                }

                dst += dstStride;
                ++y;

                if (y > 0 && y < h)
                {
                    refRow += srcStride;
                }
            } while (--bH != 0);
        }

        [SkipLocalsInit]
        private static unsafe void ExtendAndPredict(
            byte* bufPtr1,
            int preBufStride,
            int x0,
            int y0,
            int bW,
            int bH,
            int frameWidth,
            int frameHeight,
            int borderOffset,
            byte* dst,
            int dstBufStride,
            int subpelX,
            int subpelY,
            Array8<short>[] kernel,
            ref ScaleFactors sf,
            ref MacroBlockD xd,
            int w,
            int h,
            int refr,
            int xs,
            int ys)
        {
            ushort* mcBufHigh = stackalloc ushort[80 * 2 * 80 * 2];
            if (xd.CurBuf.HighBd)
            {
                HighBuildMcBorder(bufPtr1, preBufStride, mcBufHigh, bW, x0, y0, bW, bH, frameWidth, frameHeight);
                ReconInter.HighbdInterPredictor(
                    mcBufHigh + borderOffset,
                    bW,
                    (ushort*)dst,
                    dstBufStride,
                    subpelX,
                    subpelY,
                    ref sf,
                    w,
                    h,
                    refr,
                    kernel,
                    xs,
                    ys,
                    xd.Bd);
            }
            else
            {
                BuildMcBorder(bufPtr1, preBufStride, (byte*)mcBufHigh, bW, x0, y0, bW, bH, frameWidth, frameHeight);
                ReconInter.InterPredictor(
                    (byte*)mcBufHigh + borderOffset,
                    bW,
                    dst,
                    dstBufStride,
                    subpelX,
                    subpelY,
                    ref sf,
                    w,
                    h,
                    refr,
                    kernel,
                    xs,
                    ys);
            }
        }

        private static unsafe void DecBuildInterPredictors(
            ref MacroBlockD xd,
            int plane,
            int bw,
            int bh,
            int x,
            int y,
            int w,
            int h,
            int miX,
            int miY,
            Array8<short>[] kernel,
            ref ScaleFactors sf,
            ref Buf2D preBuf,
            ref Buf2D dstBuf,
            ref Mv mv,
            ref Surface refFrameBuf,
            bool isScaled,
            int refr)
        {
            ref MacroBlockDPlane pd = ref xd.Plane[plane];
            byte* dst = dstBuf.Buf.ToPointer() + dstBuf.Stride * y + x;
            Mv32 scaledMv;
            int xs, ys, x0, y0, x0_16, y0_16, frameWidth, frameHeight, bufStride, subpelX, subpelY;
            byte* refFrame;
            byte* bufPtr;

            // Get reference frame pointer, width and height.
            if (plane == 0)
            {
                frameWidth = refFrameBuf.Width;
                frameHeight = refFrameBuf.Height;
                refFrame = refFrameBuf.YBuffer.ToPointer();
            }
            else
            {
                frameWidth = refFrameBuf.UvWidth;
                frameHeight = refFrameBuf.UvHeight;
                refFrame = plane == 1 ? refFrameBuf.UBuffer.ToPointer() : refFrameBuf.VBuffer.ToPointer();
            }

            if (isScaled)
            {
                Mv mvQ4 = ReconInter.ClampMvToUmvBorderSb(ref xd, ref mv, bw, bh, pd.SubsamplingX, pd.SubsamplingY);
                // Co-ordinate of containing block to pixel precision.
                int xStart = (-xd.MbToLeftEdge >> (3 + pd.SubsamplingX));
                int yStart = (-xd.MbToTopEdge >> (3 + pd.SubsamplingY));
                // Co-ordinate of the block to 1/16th pixel precision.
                x0_16 = (xStart + x) << Filter.SubpelBits;
                y0_16 = (yStart + y) << Filter.SubpelBits;

                // Co-ordinate of current block in reference frame
                // to 1/16th pixel precision.
                x0_16 = sf.ScaleValueX(x0_16);
                y0_16 = sf.ScaleValueY(y0_16);

                // Map the top left corner of the block into the reference frame.
                x0 = sf.ScaleValueX(xStart + x);
                y0 = sf.ScaleValueY(yStart + y);

                // Scale the MV and incorporate the sub-pixel offset of the block
                // in the reference frame.
                scaledMv = sf.ScaleMv(ref mvQ4, miX + x, miY + y);
                xs = sf.XStepQ4;
                ys = sf.YStepQ4;
            }
            else
            {
                // Co-ordinate of containing block to pixel precision.
                x0 = (-xd.MbToLeftEdge >> (3 + pd.SubsamplingX)) + x;
                y0 = (-xd.MbToTopEdge >> (3 + pd.SubsamplingY)) + y;

                // Co-ordinate of the block to 1/16th pixel precision.
                x0_16 = x0 << Filter.SubpelBits;
                y0_16 = y0 << Filter.SubpelBits;

                scaledMv.Row = mv.Row * (1 << (1 - pd.SubsamplingY));
                scaledMv.Col = mv.Col * (1 << (1 - pd.SubsamplingX));
                xs = ys = 16;
            }
            subpelX = scaledMv.Col & Filter.SubpelMask;
            subpelY = scaledMv.Row & Filter.SubpelMask;

            // Calculate the top left corner of the best matching block in the
            // reference frame.
            x0 += scaledMv.Col >> Filter.SubpelBits;
            y0 += scaledMv.Row >> Filter.SubpelBits;
            x0_16 += scaledMv.Col;
            y0_16 += scaledMv.Row;

            // Get reference block pointer.
            bufPtr = refFrame + y0 * preBuf.Stride + x0;
            bufStride = preBuf.Stride;

            // Do border extension if there is motion or the
            // width/height is not a multiple of 8 pixels.
            if (isScaled || scaledMv.Col != 0 || scaledMv.Row != 0 || (frameWidth & 0x7) != 0 || (frameHeight & 0x7) != 0)
            {
                int y1 = ((y0_16 + (h - 1) * ys) >> Filter.SubpelBits) + 1;

                // Get reference block bottom right horizontal coordinate.
                int x1 = ((x0_16 + (w - 1) * xs) >> Filter.SubpelBits) + 1;
                int xPad = 0, yPad = 0;

                if (subpelX != 0 || (sf.XStepQ4 != Filter.SubpelShifts))
                {
                    x0 -= Constants.Vp9InterpExtend - 1;
                    x1 += Constants.Vp9InterpExtend;
                    xPad = 1;
                }

                if (subpelY != 0 || (sf.YStepQ4 != Filter.SubpelShifts))
                {
                    y0 -= Constants.Vp9InterpExtend - 1;
                    y1 += Constants.Vp9InterpExtend;
                    yPad = 1;
                }

                // Skip border extension if block is inside the frame.
                if (x0 < 0 || x0 > frameWidth - 1 || x1 < 0 || x1 > frameWidth - 1 ||
                    y0 < 0 || y0 > frameHeight - 1 || y1 < 0 || y1 > frameHeight - 1)
                {
                    // Extend the border.
                    byte* bufPtr1 = refFrame + y0 * bufStride + x0;
                    int bW = x1 - x0 + 1;
                    int bH = y1 - y0 + 1;
                    int borderOffset = yPad * 3 * bW + xPad * 3;

                    ExtendAndPredict(
                        bufPtr1,
                        bufStride,
                        x0,
                        y0,
                        bW,
                        bH,
                        frameWidth,
                        frameHeight,
                        borderOffset,
                        dst,
                        dstBuf.Stride,
                        subpelX,
                        subpelY,
                        kernel,
                        ref sf,
                        ref xd,
                        w,
                        h,
                        refr,
                        xs,
                        ys);
                    return;
                }
            }
            if (xd.CurBuf.HighBd)
            {
                ReconInter.HighbdInterPredictor(
                    (ushort*)bufPtr,
                    bufStride,
                    (ushort*)dst,
                    dstBuf.Stride,
                    subpelX,
                    subpelY,
                    ref sf,
                    w,
                    h,
                    refr,
                    kernel,
                    xs,
                    ys,
                    xd.Bd);
            }
            else
            {
                ReconInter.InterPredictor(
                    bufPtr,
                    bufStride,
                    dst,
                    dstBuf.Stride,
                    subpelX,
                    subpelY,
                    ref sf,
                    w,
                    h,
                    refr,
                    kernel,
                    xs,
                    ys);
            }
        }

        private static void DecBuildInterPredictorsSb(ref Vp9Common cm, ref MacroBlockD xd, int miRow, int miCol)
        {
            int plane;
            int miX = miCol * Constants.MiSize;
            int miY = miRow * Constants.MiSize;
            ref ModeInfo mi = ref xd.Mi[0].Value;
            Array8<short>[] kernel = Luts.Vp9FilterKernels[mi.InterpFilter];
            BlockSize sbType = mi.SbType;
            int isCompound = mi.HasSecondRef() ? 1 : 0;
            int refr;
            bool isScaled;

            for (refr = 0; refr < 1 + isCompound; ++refr)
            {
                int frame = mi.RefFrame[refr];
                ref RefBuffer refBuf = ref cm.FrameRefs[frame - Constants.LastFrame];
                ref ScaleFactors sf = ref refBuf.Sf;
                ref Surface refFrameBuf = ref refBuf.Buf;

                if (!sf.IsValidScale())
                {
                    xd.ErrorInfo.Value.InternalError(CodecErr.CodecUnsupBitstream, "Reference frame has invalid dimensions");
                }

                isScaled = sf.IsScaled();
                ReconInter.SetupPrePlanes(ref xd, refr, ref refFrameBuf, miRow, miCol, isScaled ? new Ptr<ScaleFactors>(ref sf) : Ptr<ScaleFactors>.Null);
                xd.BlockRefs[refr] = new Ptr<RefBuffer>(ref refBuf);

                if (sbType < BlockSize.Block8x8)
                {
                    for (plane = 0; plane < Constants.MaxMbPlane; ++plane)
                    {
                        ref MacroBlockDPlane pd = ref xd.Plane[plane];
                        ref Buf2D dstBuf = ref pd.Dst;
                        int num4x4W = pd.N4W;
                        int num4x4H = pd.N4H;
                        int n4Wx4 = 4 * num4x4W;
                        int n4Hx4 = 4 * num4x4H;
                        ref Buf2D preBuf = ref pd.Pre[refr];
                        int i = 0, x, y;
                        for (y = 0; y < num4x4H; ++y)
                        {
                            for (x = 0; x < num4x4W; ++x)
                            {
                                Mv mv = ReconInter.AverageSplitMvs(ref pd, ref mi, refr, i++);
                                DecBuildInterPredictors(
                                    ref xd,
                                    plane,
                                    n4Wx4,
                                    n4Hx4,
                                    4 * x,
                                    4 * y,
                                    4,
                                    4,
                                    miX,
                                    miY,
                                    kernel,
                                    ref sf,
                                    ref preBuf,
                                    ref dstBuf,
                                    ref mv,
                                    ref refFrameBuf,
                                    isScaled,
                                    refr);
                            }
                        }
                    }
                }
                else
                {
                    Mv mv = mi.Mv[refr];
                    for (plane = 0; plane < Constants.MaxMbPlane; ++plane)
                    {
                        ref MacroBlockDPlane pd = ref xd.Plane[plane];
                        ref Buf2D dstBuf = ref pd.Dst;
                        int num4x4W = pd.N4W;
                        int num4x4H = pd.N4H;
                        int n4Wx4 = 4 * num4x4W;
                        int n4Hx4 = 4 * num4x4H;
                        ref Buf2D preBuf = ref pd.Pre[refr];
                        DecBuildInterPredictors(
                            ref xd,
                            plane,
                            n4Wx4,
                            n4Hx4,
                            0,
                            0,
                            n4Wx4,
                            n4Hx4,
                            miX,
                            miY,
                            kernel,
                            ref sf,
                            ref preBuf,
                            ref dstBuf,
                            ref mv,
                            ref refFrameBuf,
                            isScaled,
                            refr);
                    }
                }
            }
        }

        private static unsafe void DecResetSkipContext(ref MacroBlockD xd)
        {
            int i;
            for (i = 0; i < Constants.MaxMbPlane; i++)
            {
                ref MacroBlockDPlane pd = ref xd.Plane[i];
                MemoryUtil.Fill(pd.AboveContext.ToPointer(), (sbyte)0, pd.N4W);
                MemoryUtil.Fill(pd.LeftContext.ToPointer(), (sbyte)0, pd.N4H);
            }
        }

        private static void SetPlaneN4(ref MacroBlockD xd, int bw, int bh, int bwl, int bhl)
        {
            int i;
            for (i = 0; i < Constants.MaxMbPlane; i++)
            {
                xd.Plane[i].N4W = (ushort)((bw << 1) >> xd.Plane[i].SubsamplingX);
                xd.Plane[i].N4H = (ushort)((bh << 1) >> xd.Plane[i].SubsamplingY);
                xd.Plane[i].N4Wl = (byte)(bwl - xd.Plane[i].SubsamplingX);
                xd.Plane[i].N4Hl = (byte)(bhl - xd.Plane[i].SubsamplingY);
            }
        }

        private static ref ModeInfo SetOffsets(
            ref Vp9Common cm,
            ref MacroBlockD xd,
            BlockSize bsize,
            int miRow,
            int miCol,
            int bw,
            int bh,
            int xMis,
            int yMis,
            int bwl,
            int bhl)
        {
            int offset = miRow * cm.MiStride + miCol;
            int x, y;
            ref TileInfo tile = ref xd.Tile;

            xd.Mi = cm.MiGridVisible.Slice(offset);
            xd.Mi[0] = new Ptr<ModeInfo>(ref cm.Mi[offset]);
            xd.Mi[0].Value.SbType = bsize;
            for (y = 0; y < yMis; ++y)
            {
                for (x = y == 0 ? 1 : 0; x < xMis; ++x)
                {
                    xd.Mi[y * cm.MiStride + x] = xd.Mi[0];
                }
            }

            SetPlaneN4(ref xd, bw, bh, bwl, bhl);

            xd.SetSkipContext(miRow, miCol);

            // Distance of Mb to the various image edges. These are specified to 8th pel
            // as they are always compared to values that are in 1/8th pel units
            xd.SetMiRowCol(ref tile, miRow, bh, miCol, bw, cm.MiRows, cm.MiCols);

            ReconInter.SetupDstPlanes(ref xd.Plane, ref xd.CurBuf, miRow, miCol);
            return ref xd.Mi[0].Value;
        }

        private static void DecodeBlock(
            ref TileWorkerData twd,
            ref Vp9Common cm,
            int miRow,
            int miCol,
            BlockSize bsize,
            int bwl,
            int bhl)
        {
            bool less8x8 = bsize < BlockSize.Block8x8;
            int bw = 1 << (bwl - 1);
            int bh = 1 << (bhl - 1);
            int xMis = Math.Min(bw, cm.MiCols - miCol);
            int yMis = Math.Min(bh, cm.MiRows - miRow);
            ref Reader r = ref twd.BitReader;
            ref MacroBlockD xd = ref twd.Xd;

            ref ModeInfo mi = ref SetOffsets(ref cm, ref xd, bsize, miRow, miCol, bw, bh, xMis, yMis, bwl, bhl);

            if (bsize >= BlockSize.Block8x8 && (cm.SubsamplingX != 0 || cm.SubsamplingY != 0))
            {
                BlockSize uvSubsize = Luts.SsSizeLookup[(int)bsize][cm.SubsamplingX][cm.SubsamplingY];
                if (uvSubsize == BlockSize.BlockInvalid)
                {
                    xd.ErrorInfo.Value.InternalError(CodecErr.CodecCorruptFrame, "Invalid block size.");
                }
            }

            DecodeMv.ReadModeInfo(ref twd, ref cm, miRow, miCol, xMis, yMis);

            if (mi.Skip != 0)
            {
                DecResetSkipContext(ref xd);
            }

            if (!mi.IsInterBlock())
            {
                int plane;
                for (plane = 0; plane < Constants.MaxMbPlane; ++plane)
                {
                    ref MacroBlockDPlane pd = ref xd.Plane[plane];
                    TxSize txSize = plane != 0 ? mi.GetUvTxSize(ref pd) : mi.TxSize;
                    int num4x4W = pd.N4W;
                    int num4x4H = pd.N4H;
                    int step = 1 << (int)txSize;
                    int row, col;
                    int maxBlocksWide = num4x4W + (xd.MbToRightEdge >= 0 ? 0 : xd.MbToRightEdge >> (5 + pd.SubsamplingX));
                    int maxBlocksHigh = num4x4H + (xd.MbToBottomEdge >= 0 ? 0 : xd.MbToBottomEdge >> (5 + pd.SubsamplingY));

                    xd.MaxBlocksWide = (uint)(xd.MbToRightEdge >= 0 ? 0 : maxBlocksWide);
                    xd.MaxBlocksHigh = (uint)(xd.MbToBottomEdge >= 0 ? 0 : maxBlocksHigh);

                    for (row = 0; row < maxBlocksHigh; row += step)
                    {
                        for (col = 0; col < maxBlocksWide; col += step)
                        {
                            PredictAndReconstructIntraBlock(ref twd, ref mi, plane, row, col, txSize);
                        }
                    }
                }
            }
            else
            {
                // Prediction
                DecBuildInterPredictorsSb(ref cm, ref xd, miRow, miCol);

                // Reconstruction
                if (mi.Skip == 0)
                {
                    int eobtotal = 0;
                    int plane;

                    for (plane = 0; plane < Constants.MaxMbPlane; ++plane)
                    {
                        ref MacroBlockDPlane pd = ref xd.Plane[plane];
                        TxSize txSize = plane != 0 ? mi.GetUvTxSize(ref pd) : mi.TxSize;
                        int num4x4W = pd.N4W;
                        int num4x4H = pd.N4H;
                        int step = 1 << (int)txSize;
                        int row, col;
                        int maxBlocksWide = num4x4W + (xd.MbToRightEdge >= 0 ? 0 : xd.MbToRightEdge >> (5 + pd.SubsamplingX));
                        int maxBlocksHigh = num4x4H + (xd.MbToBottomEdge >= 0 ? 0 : xd.MbToBottomEdge >> (5 + pd.SubsamplingY));

                        xd.MaxBlocksWide = (uint)(xd.MbToRightEdge >= 0 ? 0 : maxBlocksWide);
                        xd.MaxBlocksHigh = (uint)(xd.MbToBottomEdge >= 0 ? 0 : maxBlocksHigh);

                        for (row = 0; row < maxBlocksHigh; row += step)
                        {
                            for (col = 0; col < maxBlocksWide; col += step)
                            {
                                eobtotal += ReconstructInterBlock(ref twd, ref mi, plane, row, col, txSize);
                            }
                        }
                    }

                    if (!less8x8 && eobtotal == 0)
                    {
                        mi.Skip = 1;  // Skip loopfilter
                    }
                }
            }

            xd.Corrupted |= r.HasError();

            if (cm.Lf.FilterLevel != 0)
            {
                LoopFilter.BuildMask(ref cm, ref mi, miRow, miCol, bw, bh);
            }
        }

        private static int DecPartitionPlaneContext(ref TileWorkerData twd, int miRow, int miCol, int bsl)
        {
            ref sbyte aboveCtx = ref twd.Xd.AboveSegContext[miCol];
            ref sbyte leftCtx = ref twd.Xd.LeftSegContext[miRow & Constants.MiMask];
            int above = (aboveCtx >> bsl) & 1, left = (leftCtx >> bsl) & 1;

            return (left * 2 + above) + bsl * Constants.PartitionPloffset;
        }

        private static void DecUpdatePartitionContext(
            ref TileWorkerData twd,
            int miRow,
            int miCol,
            BlockSize subsize,
            int bw)
        {
            Span<sbyte> aboveCtx = twd.Xd.AboveSegContext.Slice(miCol).ToSpan();
            Span<sbyte> leftCtx = MemoryMarshal.CreateSpan(ref twd.Xd.LeftSegContext[miRow & Constants.MiMask], 8 - (miRow & Constants.MiMask));

            // Update the partition context at the end notes. Set partition bits
            // of block sizes larger than the current one to be one, and partition
            // bits of smaller block sizes to be zero.
            aboveCtx.Slice(0, bw).Fill(Luts.PartitionContextLookup[(int)subsize].Above);
            leftCtx.Slice(0, bw).Fill(Luts.PartitionContextLookup[(int)subsize].Left);
        }

        private static PartitionType ReadPartition(
            ref TileWorkerData twd,
            int miRow,
            int miCol,
            int hasRows,
            int hasCols,
            int bsl)
        {
            int ctx = DecPartitionPlaneContext(ref twd, miRow, miCol, bsl);
            ReadOnlySpan<byte> probs = MemoryMarshal.CreateReadOnlySpan(ref twd.Xd.PartitionProbs[ctx][0], 3);
            PartitionType p;
            ref Reader r = ref twd.BitReader;

            if (hasRows != 0 && hasCols != 0)
            {
                p = (PartitionType)r.ReadTree(Luts.Vp9PartitionTree, probs);
            }
            else if (hasRows == 0 && hasCols != 0)
            {
                p = r.Read(probs[1]) != 0 ? PartitionType.PartitionSplit : PartitionType.PartitionHorz;
            }
            else if (hasRows != 0 && hasCols == 0)
            {
                p = r.Read(probs[2]) != 0 ? PartitionType.PartitionSplit : PartitionType.PartitionVert;
            }
            else
            {
                p = PartitionType.PartitionSplit;
            }

            if (!twd.Xd.Counts.IsNull)
            {
                ++twd.Xd.Counts.Value.Partition[ctx][(int)p];
            }

            return p;
        }

        private static void DecodePartition(
            ref TileWorkerData twd,
            ref Vp9Common cm,
            int miRow,
            int miCol,
            BlockSize bsize,
            int n4x4L2)
        {
            int n8x8L2 = n4x4L2 - 1;
            int num8x8Wh = 1 << n8x8L2;
            int hbs = num8x8Wh >> 1;
            PartitionType partition;
            BlockSize subsize;
            bool hasRows = (miRow + hbs) < cm.MiRows;
            bool hasCols = (miCol + hbs) < cm.MiCols;
            ref MacroBlockD xd = ref twd.Xd;

            if (miRow >= cm.MiRows || miCol >= cm.MiCols)
            {
                return;
            }

            partition = ReadPartition(ref twd, miRow, miCol, hasRows ? 1 : 0, hasCols ? 1 : 0, n8x8L2);
            subsize = Luts.SubsizeLookup[(int)partition][(int)bsize];
            if (hbs == 0)
            {
                // Calculate bmode block dimensions (log 2)
                xd.BmodeBlocksWl = (byte)(1 >> ((partition & PartitionType.PartitionVert) != 0 ? 1 : 0));
                xd.BmodeBlocksHl = (byte)(1 >> ((partition & PartitionType.PartitionHorz) != 0 ? 1 : 0));
                DecodeBlock(ref twd, ref cm, miRow, miCol, subsize, 1, 1);
            }
            else
            {
                switch (partition)
                {
                    case PartitionType.PartitionNone:
                        DecodeBlock(ref twd, ref cm, miRow, miCol, subsize, n4x4L2, n4x4L2);
                        break;
                    case PartitionType.PartitionHorz:
                        DecodeBlock(ref twd, ref cm, miRow, miCol, subsize, n4x4L2, n8x8L2);
                        if (hasRows)
                        {
                            DecodeBlock(ref twd, ref cm, miRow + hbs, miCol, subsize, n4x4L2, n8x8L2);
                        }

                        break;
                    case PartitionType.PartitionVert:
                        DecodeBlock(ref twd, ref cm, miRow, miCol, subsize, n8x8L2, n4x4L2);
                        if (hasCols)
                        {
                            DecodeBlock(ref twd, ref cm, miRow, miCol + hbs, subsize, n8x8L2, n4x4L2);
                        }

                        break;
                    case PartitionType.PartitionSplit:
                        DecodePartition(ref twd, ref cm, miRow, miCol, subsize, n8x8L2);
                        DecodePartition(ref twd, ref cm, miRow, miCol + hbs, subsize, n8x8L2);
                        DecodePartition(ref twd, ref cm, miRow + hbs, miCol, subsize, n8x8L2);
                        DecodePartition(ref twd, ref cm, miRow + hbs, miCol + hbs, subsize, n8x8L2);
                        break;
                    default: Debug.Assert(false, "Invalid partition type"); break;
                }
            }

            // Update partition context
            if (bsize >= BlockSize.Block8x8 && (bsize == BlockSize.Block8x8 || partition != PartitionType.PartitionSplit))
            {
                DecUpdatePartitionContext(ref twd, miRow, miCol, subsize, num8x8Wh);
            }
        }

        private static void SetupTokenDecoder(
            ArrayPtr<byte> data,
            int readSize,
            ref InternalErrorInfo errorInfo,
            ref Reader r)
        {
            // Validate the calculated partition length. If the buffer described by the
            // partition can't be fully read then throw an error.
            if (!ReadIsValid(data, readSize))
            {
                errorInfo.InternalError(CodecErr.CodecCorruptFrame, "Truncated packet or corrupt tile length");
            }

            if (r.Init(data, readSize))
            {
                errorInfo.InternalError(CodecErr.CodecMemError, "Failed to allocate bool decoder 1");
            }
        }

        // Reads the next tile returning its size and adjusting '*data' accordingly
        // based on 'isLast'.
        private static void GetTileBuffer(
            bool isLast,
            ref InternalErrorInfo errorInfo,
            ref ArrayPtr<byte> data,
            ref TileBuffer buf)
        {
            int size;

            if (!isLast)
            {
                if (!ReadIsValid(data, 4))
                {
                    errorInfo.InternalError(CodecErr.CodecCorruptFrame, "Truncated packet or corrupt tile length");
                }

                size = BinaryPrimitives.ReadInt32BigEndian(data.ToSpan());
                data = data.Slice(4);

                if (size > data.Length)
                {
                    errorInfo.InternalError(CodecErr.CodecCorruptFrame, "Truncated packet or corrupt tile size");
                }
            }
            else
            {
                size = data.Length;
            }

            buf.Data = data;
            buf.Size = size;

            data = data.Slice(size);
        }

        private static void GetTileBuffers(ref Vp9Common cm, ArrayPtr<byte> data, int tileCols, ref Array64<TileBuffer> tileBuffers)
        {
            int c;

            for (c = 0; c < tileCols; ++c)
            {
                bool isLast = c == tileCols - 1;
                ref TileBuffer buf = ref tileBuffers[c];
                buf.Col = c;
                GetTileBuffer(isLast, ref cm.Error, ref data, ref buf);
            }
        }

        private static void GetTileBuffers(
            ref Vp9Common cm,
            ArrayPtr<byte> data,
            int tileCols,
            int tileRows,
            ref Array4<Array64<TileBuffer>> tileBuffers)
        {
            int r, c;

            for (r = 0; r < tileRows; ++r)
            {
                for (c = 0; c < tileCols; ++c)
                {
                    bool isLast = (r == tileRows - 1) && (c == tileCols - 1);
                    ref TileBuffer buf = ref tileBuffers[r][c];
                    GetTileBuffer(isLast, ref cm.Error, ref data, ref buf);
                }
            }
        }

        public static unsafe ArrayPtr<byte> DecodeTiles(ref Vp9Common cm, ArrayPtr<byte> data)
        {
            int alignedCols = TileInfo.MiColsAlignedToSb(cm.MiCols);
            int tileCols = 1 << cm.Log2TileCols;
            int tileRows = 1 << cm.Log2TileRows;
            Array4<Array64<TileBuffer>> tileBuffers = new Array4<Array64<TileBuffer>>();
            int tileRow, tileCol;
            int miRow, miCol;

            Debug.Assert(tileRows <= 4);
            Debug.Assert(tileCols <= (1 << 6));

            // Note: this memset assumes above_context[0], [1] and [2]
            // are allocated as part of the same buffer.
            MemoryUtil.Fill(cm.AboveContext.ToPointer(), (sbyte)0, Constants.MaxMbPlane * 2 * alignedCols);
            MemoryUtil.Fill(cm.AboveSegContext.ToPointer(), (sbyte)0, alignedCols);

            LoopFilter.ResetLfm(ref cm);

            GetTileBuffers(ref cm, data, tileCols, tileRows, ref tileBuffers);
            // Load all tile information into tile_data.
            for (tileRow = 0; tileRow < tileRows; ++tileRow)
            {
                for (tileCol = 0; tileCol < tileCols; ++tileCol)
                {
                    ref TileBuffer buf = ref tileBuffers[tileRow][tileCol];
                    ref TileWorkerData tileData = ref cm.TileWorkerData[tileCols * tileRow + tileCol];
                    tileData.Xd = cm.Mb;
                    tileData.Xd.Corrupted = false;
                    tileData.Xd.Counts = cm.Counts;
                    tileData.Dqcoeff = new Array32<Array32<int>>();
                    tileData.Xd.Tile.Init(ref cm, tileRow, tileCol);
                    SetupTokenDecoder(buf.Data, buf.Size, ref cm.Error, ref tileData.BitReader);
                    cm.InitMacroBlockD(ref tileData.Xd, new ArrayPtr<int>(ref tileData.Dqcoeff[0][0], 32 * 32));
                }
            }

            for (tileRow = 0; tileRow < tileRows; ++tileRow)
            {
                TileInfo tile = new TileInfo();
                tile.SetRow(ref cm, tileRow);
                for (miRow = tile.MiRowStart; miRow < tile.MiRowEnd; miRow += Constants.MiBlockSize)
                {
                    for (tileCol = 0; tileCol < tileCols; ++tileCol)
                    {
                        int col = tileCol;
                        ref TileWorkerData tileData = ref cm.TileWorkerData[tileCols * tileRow + col];
                        tile.SetCol(ref cm, col);
                        tileData.Xd.LeftContext = new Array3<Array16<sbyte>>();
                        tileData.Xd.LeftSegContext = new Array8<sbyte>();
                        for (miCol = tile.MiColStart; miCol < tile.MiColEnd; miCol += Constants.MiBlockSize)
                        {
                            DecodePartition(ref tileData, ref cm, miRow, miCol, BlockSize.Block64x64, 4);
                        }
                        cm.Mb.Corrupted |= tileData.Xd.Corrupted;
                        if (cm.Mb.Corrupted)
                        {
                            cm.Error.InternalError(CodecErr.CodecCorruptFrame, "Failed to decode tile data");
                        };
                    }
                }
            }

            // Get last tile data.
            return cm.TileWorkerData[tileCols * tileRows - 1].BitReader.FindEnd();
        }

        private static bool DecodeTileCol(ref TileWorkerData tileData, ref Vp9Common cm, ref Array64<TileBuffer> tileBuffers)
        {
            ref TileInfo tile = ref tileData.Xd.Tile;
            int finalCol = (1 << cm.Log2TileCols) - 1;
            ArrayPtr<byte> bitReaderEnd = ArrayPtr<byte>.Null;

            int n = tileData.BufStart;

            tileData.Xd.Corrupted = false;

            do
            {
                ref TileBuffer buf = ref tileBuffers[n];

                Debug.Assert(cm.Log2TileRows == 0);
                tileData.Dqcoeff = new Array32<Array32<int>>();
                tile.Init(ref cm, 0, buf.Col);
                SetupTokenDecoder(buf.Data, buf.Size, ref tileData.ErrorInfo, ref tileData.BitReader);
                cm.InitMacroBlockD(ref tileData.Xd, new ArrayPtr<int>(ref tileData.Dqcoeff[0][0], 32 * 32));
                tileData.Xd.ErrorInfo = new Ptr<InternalErrorInfo>(ref tileData.ErrorInfo);

                for (int miRow = tile.MiRowStart; miRow < tile.MiRowEnd; miRow += Constants.MiBlockSize)
                {
                    tileData.Xd.LeftContext = new Array3<Array16<sbyte>>();
                    tileData.Xd.LeftSegContext = new Array8<sbyte>();
                    for (int miCol = tile.MiColStart; miCol < tile.MiColEnd; miCol += Constants.MiBlockSize)
                    {
                        DecodePartition(ref tileData, ref cm, miRow, miCol, BlockSize.Block64x64, 4);
                    }
                }

                if (buf.Col == finalCol)
                {
                    bitReaderEnd = tileData.BitReader.FindEnd();
                }
            } while (!tileData.Xd.Corrupted && ++n <= tileData.BufEnd);

            tileData.DataEnd = bitReaderEnd;
            return !tileData.Xd.Corrupted;
        }

        public static unsafe ArrayPtr<byte> DecodeTilesMt(ref Vp9Common cm, ArrayPtr<byte> data, int maxThreads)
        {
            ArrayPtr<byte> bitReaderEnd = ArrayPtr<byte>.Null;

            int tileCols = 1 << cm.Log2TileCols;
            int tileRows = 1 << cm.Log2TileRows;
            int totalTiles = tileCols * tileRows;
            int numWorkers = Math.Min(maxThreads, tileCols);
            int n;

            Debug.Assert(tileCols <= (1 << 6));
            Debug.Assert(tileRows == 1);

            cm.AboveContext.ToSpan().Fill(0);
            cm.AboveSegContext.ToSpan().Fill(0);

            for (n = 0; n < numWorkers; ++n)
            {
                ref TileWorkerData tileData = ref cm.TileWorkerData[n + totalTiles];

                tileData.Xd = cm.Mb;
                tileData.Xd.Counts = new Ptr<Vp9BackwardUpdates>(ref tileData.Counts);
                tileData.Counts = new Vp9BackwardUpdates();
            }

            Array64<TileBuffer> tileBuffers = new Array64<TileBuffer>();

            GetTileBuffers(ref cm, data, tileCols, ref tileBuffers);

            tileBuffers.ToSpan().Slice(0, tileCols).Sort(CompareTileBuffers);

            if (numWorkers == tileCols)
            {
                TileBuffer largest = tileBuffers[0];
                Span<TileBuffer> buffers = tileBuffers.ToSpan();
                buffers.Slice(1).CopyTo(buffers.Slice(0, tileBuffers.Length - 1));
                tileBuffers[tileCols - 1] = largest;
            }
            else
            {
                int start = 0, end = tileCols - 2;
                TileBuffer tmp;

                // Interleave the tiles to distribute the load between threads, assuming a
                // larger tile implies it is more difficult to decode.
                while (start < end)
                {
                    tmp = tileBuffers[start];
                    tileBuffers[start] = tileBuffers[end];
                    tileBuffers[end] = tmp;
                    start += 2;
                    end -= 2;
                }
            }

            int baseVal = tileCols / numWorkers;
            int remain = tileCols % numWorkers;
            int bufStart = 0;

            for (n = 0; n < numWorkers; ++n)
            {
                int count = baseVal + (remain + n) / numWorkers;
                ref TileWorkerData tileData = ref cm.TileWorkerData[n + totalTiles];

                tileData.BufStart = bufStart;
                tileData.BufEnd = bufStart + count - 1;
                tileData.DataEnd = data.Slice(data.Length);
                bufStart += count;
            }

            Ptr<Vp9Common> cmPtr = new Ptr<Vp9Common>(ref cm);

            Parallel.For(0, numWorkers, (n) =>
            {
                ref TileWorkerData tileData = ref cmPtr.Value.TileWorkerData[n + totalTiles];

                if (!DecodeTileCol(ref tileData, ref cmPtr.Value, ref tileBuffers))
                {
                    cmPtr.Value.Mb.Corrupted = true;
                }
            });

            for (; n > 0; --n)
            {
                if (bitReaderEnd.IsNull)
                {
                    ref TileWorkerData tileData = ref cm.TileWorkerData[n - 1 + totalTiles];
                    bitReaderEnd = tileData.DataEnd;
                }
            }

            for (n = 0; n < numWorkers; ++n)
            {
                ref TileWorkerData tileData = ref cm.TileWorkerData[n + totalTiles];
                AccumulateFrameCounts(ref cm.Counts.Value, ref tileData.Counts);
            }

            Debug.Assert(!bitReaderEnd.IsNull || cm.Mb.Corrupted);
            return bitReaderEnd;
        }

        private static int CompareTileBuffers(TileBuffer bufA, TileBuffer bufB)
        {
            return (bufA.Size < bufB.Size ? 1 : 0) - (bufA.Size > bufB.Size ? 1 : 0);
        }

        private static void AccumulateFrameCounts(ref Vp9BackwardUpdates accum, ref Vp9BackwardUpdates counts)
        {
            Span<uint> a = MemoryMarshal.Cast<Vp9BackwardUpdates, uint>(MemoryMarshal.CreateSpan(ref accum, 1));
            Span<uint> c = MemoryMarshal.Cast<Vp9BackwardUpdates, uint>(MemoryMarshal.CreateSpan(ref counts, 1));

            for (int i = 0; i < a.Length; i++)
            {
                a[i] += c[i];
            }
        }
    }
}
