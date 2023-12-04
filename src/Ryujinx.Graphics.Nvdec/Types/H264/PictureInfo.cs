using Ryujinx.Common.Memory;
using Ryujinx.Graphics.Video;

namespace Ryujinx.Graphics.Nvdec.Types.H264
{
    struct PictureInfo
    {
#pragma warning disable IDE0051, CS0169, CS0649 // Remove unused private member
        Array18<uint> Unknown0;
#pragma warning restore IDE0051
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

        public readonly bool MbAdaptiveFrameFieldFlag => (Flags & (1 << 0)) != 0;
        public readonly bool Direct8x8InferenceFlag => (Flags & (1 << 1)) != 0;
        public readonly bool WeightedPredFlag => (Flags & (1 << 2)) != 0;
        public readonly bool ConstrainedIntraPredFlag => (Flags & (1 << 3)) != 0;
        public readonly bool IsReference => (Flags & (1 << 4)) != 0;
        public readonly bool FieldPicFlag => (Flags & (1 << 5)) != 0;
        public readonly bool BottomFieldFlag => (Flags & (1 << 6)) != 0;
        public readonly uint Log2MaxFrameNumMinus4 => (uint)(Flags >> 8) & 0xf;
        public readonly ushort ChromaFormatIdc => (ushort)((Flags >> 12) & 3);
        public readonly uint PicOrderCntType => (uint)(Flags >> 14) & 3;
        public readonly int PicInitQpMinus26 => ExtractSx(Flags, 16, 6);
        public readonly int ChromaQpIndexOffset => ExtractSx(Flags, 22, 5);
        public readonly int SecondChromaQpIndexOffset => ExtractSx(Flags, 27, 5);
        public readonly uint WeightedBipredIdc => (uint)(Flags >> 32) & 3;
        public readonly uint OutputSurfaceIndex => (uint)(Flags >> 34) & 0x7f;
        public readonly uint ColIndex => (uint)(Flags >> 41) & 0x1f;
        public readonly ushort FrameNum => (ushort)(Flags >> 46);
        public readonly bool QpprimeYZeroTransformBypassFlag => (Flags2 & (1 << 1)) != 0;

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
                QpprimeYZeroTransformBypassFlag = QpprimeYZeroTransformBypassFlag,
            };
        }
    }
}
