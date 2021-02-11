using Ryujinx.Common.Memory;
using Ryujinx.Graphics.Nvdec.Vp9.Common;
using Ryujinx.Graphics.Video;

namespace Ryujinx.Graphics.Nvdec.Vp9.Types
{
    internal struct Vp9Common
    {
        public MacroBlockD Mb;

        public ArrayPtr<TileWorkerData> TileWorkerData;

        public InternalErrorInfo Error;

        public int Width;
        public int Height;

        public int SubsamplingX;
        public int SubsamplingY;

        public ArrayPtr<MvRef> PrevFrameMvs;
        public ArrayPtr<MvRef> CurFrameMvs;

        public Array3<RefBuffer> FrameRefs;

        public FrameType FrameType;

        // Flag signaling that the frame is encoded using only Intra modes.
        public bool IntraOnly;

        public bool AllowHighPrecisionMv;

        // MBs, MbRows/Cols is in 16-pixel units; MiRows/Cols is in
        // ModeInfo (8-pixel) units.
        public int MBs;
        public int MbRows, MiRows;
        public int MbCols, MiCols;
        public int MiStride;

        /* Profile settings */
        public TxMode TxMode;

        public int BaseQindex;
        public int YDcDeltaQ;
        public int UvDcDeltaQ;
        public int UvAcDeltaQ;
        public Array8<Array2<short>> YDequant;
        public Array8<Array2<short>> UvDequant;

        /* We allocate a ModeInfo struct for each macroblock, together with
           an extra row on top and column on the left to simplify prediction. */
        public ArrayPtr<ModeInfo> Mip; /* Base of allocated array */
        public ArrayPtr<ModeInfo> Mi;  /* Corresponds to upper left visible macroblock */

        public ArrayPtr<Ptr<ModeInfo>> MiGridBase;
        public ArrayPtr<Ptr<ModeInfo>> MiGridVisible;

        // Whether to use previous frame's motion vectors for prediction.
        public bool UsePrevFrameMvs;

        // Persistent mb segment id map used in prediction.
        public int SegMapIdx;
        public int PrevSegMapIdx;

        public Array2<ArrayPtr<byte>> SegMapArray;
        public ArrayPtr<byte> LastFrameSegMap;
        public ArrayPtr<byte> CurrentFrameSegMap;

        public byte InterpFilter;

        public LoopFilterInfoN LfInfo;

        public Array4<sbyte> RefFrameSignBias; /* Two state 0, 1 */

        public LoopFilter Lf;
        public Segmentation Seg;

        // Context probabilities for reference frame prediction
        public sbyte CompFixedRef;
        public Array2<sbyte> CompVarRef;
        public ReferenceMode ReferenceMode;

        public Ptr<Vp9EntropyProbs> Fc;
        public Ptr<Vp9BackwardUpdates> Counts;

        public int Log2TileCols, Log2TileRows;

        public ArrayPtr<sbyte> AboveSegContext;
        public ArrayPtr<sbyte> AboveContext;

        public bool FrameIsIntraOnly()
        {
            return FrameType == FrameType.KeyFrame || IntraOnly;
        }

        public bool CompoundReferenceAllowed()
        {
            int i;
            for (i = 1; i < Constants.RefsPerFrame; ++i)
            {
                if (RefFrameSignBias[i + 1] != RefFrameSignBias[1])
                {
                    return true;
                }
            }

            return false;
        }

        private static int CalcMiSize(int len)
        {
            // Len is in mi units.
            return len + Constants.MiBlockSize;
        }

        public void SetMbMi(int width, int height)
        {
            int alignedWidth = BitUtils.AlignPowerOfTwo(width, Constants.MiSizeLog2);
            int alignedHeight = BitUtils.AlignPowerOfTwo(height, Constants.MiSizeLog2);

            MiCols = alignedWidth >> Constants.MiSizeLog2;
            MiRows = alignedHeight >> Constants.MiSizeLog2;
            MiStride = CalcMiSize(MiCols);

            MbCols = (MiCols + 1) >> 1;
            MbRows = (MiRows + 1) >> 1;
            MBs = MbRows * MbCols;
        }

        public void AllocTileWorkerData(MemoryAllocator allocator, int tileCols, int tileRows, int maxThreads)
        {
            TileWorkerData = allocator.Allocate<TileWorkerData>(tileCols * tileRows + (maxThreads > 1 ? maxThreads : 0));
        }

        public void FreeTileWorkerData(MemoryAllocator allocator)
        {
            allocator.Free(TileWorkerData);
        }

        private void AllocSegMap(MemoryAllocator allocator, int segMapSize)
        {
            int i;

            for (i = 0; i < Constants.NumPingPongBuffers; ++i)
            {
                SegMapArray[i] = allocator.Allocate<byte>(segMapSize);
            }

            // Init the index.
            SegMapIdx = 0;
            PrevSegMapIdx = 1;

            CurrentFrameSegMap = SegMapArray[SegMapIdx];
            LastFrameSegMap = SegMapArray[PrevSegMapIdx];
        }

        private void FreeSegMap(MemoryAllocator allocator)
        {
            int i;

            for (i = 0; i < Constants.NumPingPongBuffers; ++i)
            {
                allocator.Free(SegMapArray[i]);
                SegMapArray[i] = ArrayPtr<byte>.Null;
            }

            CurrentFrameSegMap = ArrayPtr<byte>.Null;
            LastFrameSegMap = ArrayPtr<byte>.Null;
        }

        private void DecAllocMi(MemoryAllocator allocator, int miSize)
        {
            Mip = allocator.Allocate<ModeInfo>(miSize);
            MiGridBase = allocator.Allocate<Ptr<ModeInfo>>(miSize);
        }

        private void DecFreeMi(MemoryAllocator allocator)
        {
            allocator.Free(Mip);
            Mip = ArrayPtr<ModeInfo>.Null;
            allocator.Free(MiGridBase);
            MiGridBase = ArrayPtr<Ptr<ModeInfo>>.Null;
        }

        public void FreeContextBuffers(MemoryAllocator allocator)
        {
            DecFreeMi(allocator);
            FreeSegMap(allocator);
            allocator.Free(AboveContext);
            AboveContext = ArrayPtr<sbyte>.Null;
            allocator.Free(AboveSegContext);
            AboveSegContext = ArrayPtr<sbyte>.Null;
            allocator.Free(Lf.Lfm);
            Lf.Lfm = ArrayPtr<LoopFilterMask>.Null;
            allocator.Free(CurFrameMvs);
            CurFrameMvs = ArrayPtr<MvRef>.Null;
            if (UsePrevFrameMvs)
            {
                allocator.Free(PrevFrameMvs);
                PrevFrameMvs = ArrayPtr<MvRef>.Null;
            }
        }

        private void AllocLoopFilter(MemoryAllocator allocator)
        {
            // Each lfm holds bit masks for all the 8x8 blocks in a 64x64 region. The
            // stride and rows are rounded up / truncated to a multiple of 8.
            Lf.LfmStride = (MiCols + (Constants.MiBlockSize - 1)) >> 3;
            Lf.Lfm = allocator.Allocate<LoopFilterMask>(((MiRows + (Constants.MiBlockSize - 1)) >> 3) * Lf.LfmStride);
        }

        public void AllocContextBuffers(MemoryAllocator allocator, int width, int height)
        {
            SetMbMi(width, height);
            int newMiSize = MiStride * CalcMiSize(MiRows);
            if (newMiSize != 0)
            {
                DecAllocMi(allocator, newMiSize);
            }

            if (MiRows * MiCols != 0)
            {
                // Create the segmentation map structure and set to 0.
                AllocSegMap(allocator, MiRows * MiCols);
            }

            if (MiCols != 0)
            {
                AboveContext = allocator.Allocate<sbyte>(2 * TileInfo.MiColsAlignedToSb(MiCols) * Constants.MaxMbPlane);
                AboveSegContext = allocator.Allocate<sbyte>(TileInfo.MiColsAlignedToSb(MiCols));
            }

            AllocLoopFilter(allocator);

            CurFrameMvs = allocator.Allocate<MvRef>(MiRows * MiCols);
            // Using the same size as the current frame is fine here,
            // as this is never true when we have a resolution change.
            if (UsePrevFrameMvs)
            {
                PrevFrameMvs = allocator.Allocate<MvRef>(MiRows * MiCols);
            }
        }

        private unsafe void DecSetupMi()
        {
            Mi = Mip.Slice(MiStride + 1);
            MiGridVisible = MiGridBase.Slice(MiStride + 1);
            MemoryUtil.Fill(MiGridBase.ToPointer(), Ptr<ModeInfo>.Null, MiStride * (MiRows + 1));
        }

        public unsafe void InitContextBuffers()
        {
            DecSetupMi();
            if (!LastFrameSegMap.IsNull)
            {
                MemoryUtil.Fill(LastFrameSegMap.ToPointer(), (byte)0, MiRows * MiCols);
            }
        }

        private void SetPartitionProbs(ref MacroBlockD xd)
        {
            xd.PartitionProbs = FrameIsIntraOnly()
                ? new ArrayPtr<Array3<byte>>(ref Fc.Value.KfPartitionProb[0], 16)
                : new ArrayPtr<Array3<byte>>(ref Fc.Value.PartitionProb[0], 16);
        }

        internal void InitMacroBlockD(ref MacroBlockD xd, ArrayPtr<int> dqcoeff)
        {
            int i;

            for (i = 0; i < Constants.MaxMbPlane; ++i)
            {
                xd.Plane[i].DqCoeff = dqcoeff;
                xd.AboveContext[i] = AboveContext.Slice(i * 2 * TileInfo.MiColsAlignedToSb(MiCols));

                if (i == 0)
                {
                    MemoryUtil.Copy(ref xd.Plane[i].SegDequant, ref YDequant);
                }
                else
                {
                    MemoryUtil.Copy(ref xd.Plane[i].SegDequant, ref UvDequant);
                }
                xd.Fc = new Ptr<Vp9EntropyProbs>(ref Fc.Value);
            }

            xd.AboveSegContext = AboveSegContext;
            xd.MiStride = MiStride;
            xd.ErrorInfo = new Ptr<InternalErrorInfo>(ref Error);

            SetPartitionProbs(ref xd);
        }

        public void SetupSegmentationDequant()
        {
            const BitDepth bitDepth = BitDepth.Bits8; // TODO: Configurable
            // Build y/uv dequant values based on segmentation.
            if (Seg.Enabled)
            {
                int i;
                for (i = 0; i < Constants.MaxSegments; ++i)
                {
                    int qIndex = QuantCommon.GetQIndex(ref Seg, i, BaseQindex);
                    YDequant[i][0] = QuantCommon.DcQuant(qIndex, YDcDeltaQ, bitDepth);
                    YDequant[i][1] = QuantCommon.AcQuant(qIndex, 0, bitDepth);
                    UvDequant[i][0] = QuantCommon.DcQuant(qIndex, UvDcDeltaQ, bitDepth);
                    UvDequant[i][1] = QuantCommon.AcQuant(qIndex, UvAcDeltaQ, bitDepth);
                }
            }
            else
            {
                int qIndex = BaseQindex;
                // When segmentation is disabled, only the first value is used.  The
                // remaining are don't cares.
                YDequant[0][0] = QuantCommon.DcQuant(qIndex, YDcDeltaQ, bitDepth);
                YDequant[0][1] = QuantCommon.AcQuant(qIndex, 0, bitDepth);
                UvDequant[0][0] = QuantCommon.DcQuant(qIndex, UvDcDeltaQ, bitDepth);
                UvDequant[0][1] = QuantCommon.AcQuant(qIndex, UvAcDeltaQ, bitDepth);
            }
        }

        public void SetupScaleFactors()
        {
            for (int i = 0; i < Constants.RefsPerFrame; ++i)
            {
                ref RefBuffer refBuf = ref FrameRefs[i];
                refBuf.Sf.SetupScaleFactorsForFrame(refBuf.Buf.Width, refBuf.Buf.Height, Width, Height);
            }
        }
    }
}
