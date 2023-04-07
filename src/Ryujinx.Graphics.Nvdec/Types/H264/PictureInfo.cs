using Ryujinx.Common.Memory;
using Ryujinx.Graphics.Video;

namespace Ryujinx.Graphics.Nvdec.Types.H264
{
    struct PictureInfo
    {
#pragma warning disable CS0169, CS0649
        Array18<uint> Unknown0;
        public uint BitstreamSize;
        public uint NumSlices;
        public uint Unknown50;
        public uint Unknown54;
        public uint Log2MaxPicOrderCntLsbMinus4;
        public uint DeltaPicOrderAlwaysZeroFlag;
        public uint FrameMbsOnlyFlag;
        public uint PicWidthInMbs;
        public uint PicHeightInMbs;
        public uint BlockLayout; // Not supported on T210
        public uint EntropyCodingModeFlag;
        public uint PicOrderPresentFlag;
        public uint NumRefIdxL0ActiveMinus1;
        public uint NumRefIdxL1ActiveMinus1;
        public uint DeblockingFilterControlPresentFlag;
        public uint RedundantPicCntPresentFlag;
        public uint Transform8x8ModeFlag;
        public uint LumaPitch;
        public uint ChromaPitch;
        public uint LumaTopFieldOffset;
        public uint LumaBottomFieldOffset;
        public uint LumaFrameOffset;
        public uint ChromaTopFieldOffset;
        public uint ChromaBottomFieldOffset;
        public uint ChromaFrameOffset;
        public uint HistBufferSize;
        public ulong Flags;
        public Array2<int> FieldOrderCnt;
        public Array16<ReferenceFrame> RefFrames;
        public Array6<Array16<byte>> ScalingLists4x4;
        public Array2<Array64<byte>> ScalingLists8x8;
        public byte MvcextNumInterViewRefsL0;
        public byte MvcextNumInterViewRefsL1;
        public ushort Padding2A2;
        public uint Unknown2A4;
        public uint Unknown2A8;
        public uint Unknown2AC;
        public Array16<byte> MvcextViewRefMasksL0;
        public Array16<byte> MvcextViewRefMasksL1;
        public uint Flags2;
        public Array10<uint> Unknown2D4;
#pragma warning restore CS0169, CS0649

        public bool MbAdaptiveFrameFieldFlag => (Flags & (1 << 0)) != 0;
        public bool Direct8x8InferenceFlag => (Flags & (1 << 1)) != 0;
        public bool WeightedPredFlag => (Flags & (1 << 2)) != 0;
        public bool ConstrainedIntraPredFlag => (Flags & (1 << 3)) != 0;
        public bool IsReference => (Flags & (1 << 4)) != 0;
        public bool FieldPicFlag => (Flags & (1 << 5)) != 0;
        public bool BottomFieldFlag => (Flags & (1 << 6)) != 0;
        public uint Log2MaxFrameNumMinus4 => (uint)(Flags >> 8) & 0xf;
        public ushort ChromaFormatIdc => (ushort)((Flags >> 12) & 3);
        public uint PicOrderCntType => (uint)(Flags >> 14) & 3;
        public int PicInitQpMinus26 => ExtractSx(Flags, 16, 6);
        public int ChromaQpIndexOffset => ExtractSx(Flags, 22, 5);
        public int SecondChromaQpIndexOffset => ExtractSx(Flags, 27, 5);
        public uint WeightedBipredIdc => (uint)(Flags >> 32) & 3;
        public uint OutputSurfaceIndex => (uint)(Flags >> 34) & 0x7f;
        public uint ColIndex => (uint)(Flags >> 41) & 0x1f;
        public ushort FrameNum => (ushort)(Flags >> 46);
        public bool QpprimeYZeroTransformBypassFlag => (Flags2 & (1 << 1)) != 0;

        private static int ExtractSx(ulong packed, int lsb, int length)
        {
            return (int)((long)packed << (64 - (lsb + length)) >> (64 - length));
        }

        public H264PictureInfo Convert()
        {
            return new H264PictureInfo()
            {
                FieldOrderCnt = FieldOrderCnt,
                IsReference = IsReference,
                ChromaFormatIdc = ChromaFormatIdc,
                FrameNum = FrameNum,
                FieldPicFlag = FieldPicFlag,
                BottomFieldFlag = BottomFieldFlag,
                NumRefFrames = 0,
                MbAdaptiveFrameFieldFlag = MbAdaptiveFrameFieldFlag,
                ConstrainedIntraPredFlag = ConstrainedIntraPredFlag,
                WeightedPredFlag = WeightedPredFlag,
                WeightedBipredIdc = WeightedBipredIdc,
                FrameMbsOnlyFlag = FrameMbsOnlyFlag != 0,
                Transform8x8ModeFlag = Transform8x8ModeFlag != 0,
                ChromaQpIndexOffset = ChromaQpIndexOffset,
                SecondChromaQpIndexOffset = SecondChromaQpIndexOffset,
                PicInitQpMinus26 = PicInitQpMinus26,
                NumRefIdxL0ActiveMinus1 = NumRefIdxL0ActiveMinus1,
                NumRefIdxL1ActiveMinus1 = NumRefIdxL1ActiveMinus1,
                Log2MaxFrameNumMinus4 = Log2MaxFrameNumMinus4,
                PicOrderCntType = PicOrderCntType,
                Log2MaxPicOrderCntLsbMinus4 = Log2MaxPicOrderCntLsbMinus4,
                DeltaPicOrderAlwaysZeroFlag = DeltaPicOrderAlwaysZeroFlag != 0,
                Direct8x8InferenceFlag = Direct8x8InferenceFlag,
                EntropyCodingModeFlag = EntropyCodingModeFlag != 0,
                PicOrderPresentFlag = PicOrderPresentFlag != 0,
                DeblockingFilterControlPresentFlag = DeblockingFilterControlPresentFlag != 0,
                RedundantPicCntPresentFlag = RedundantPicCntPresentFlag != 0,
                NumSliceGroupsMinus1 = 0,
                SliceGroupMapType = 0,
                SliceGroupChangeRateMinus1 = 0,
                FmoAsoEnable = false,
                ScalingMatrixPresent = true,
                ScalingLists4x4 = ScalingLists4x4,
                ScalingLists8x8 = ScalingLists8x8,
                FrameType = 0,
                PicWidthInMbsMinus1 = PicWidthInMbs - 1,
                PicHeightInMapUnitsMinus1 = (PicHeightInMbs >> (FrameMbsOnlyFlag != 0 ? 0 : 1)) - 1,
                QpprimeYZeroTransformBypassFlag = QpprimeYZeroTransformBypassFlag
            };
        }
    }
}
