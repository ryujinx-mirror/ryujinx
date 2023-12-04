using Ryujinx.Common.Memory;

namespace Ryujinx.Graphics.Video
{
    public struct H264PictureInfo
    {
        public Array2<int> FieldOrderCnt;
        public bool IsReference;
        public ushort ChromaFormatIdc;
        public ushort FrameNum;
        public bool FieldPicFlag;
        public bool BottomFieldFlag;
        public uint NumRefFrames;
        public bool MbAdaptiveFrameFieldFlag;
        public bool ConstrainedIntraPredFlag;
        public bool WeightedPredFlag;
        public uint WeightedBipredIdc;
        public bool FrameMbsOnlyFlag;
        public bool Transform8x8ModeFlag;
        public int ChromaQpIndexOffset;
        public int SecondChromaQpIndexOffset;
        public int PicInitQpMinus26;
        public uint NumRefIdxL0ActiveMinus1;
        public uint NumRefIdxL1ActiveMinus1;
        public uint Log2MaxFrameNumMinus4;
        public uint PicOrderCntType;
        public uint Log2MaxPicOrderCntLsbMinus4;
        public bool DeltaPicOrderAlwaysZeroFlag;
        public bool Direct8x8InferenceFlag;
        public bool EntropyCodingModeFlag;
        public bool PicOrderPresentFlag;
        public bool DeblockingFilterControlPresentFlag;
        public bool RedundantPicCntPresentFlag;
        public uint NumSliceGroupsMinus1;
        public uint SliceGroupMapType;
        public uint SliceGroupChangeRateMinus1;
        // TODO: Slice group map
        public bool FmoAsoEnable;
        public bool ScalingMatrixPresent;
        public Array6<Array16<byte>> ScalingLists4x4;
        public Array2<Array64<byte>> ScalingLists8x8;
        public uint FrameType;
        public uint PicWidthInMbsMinus1;
        public uint PicHeightInMapUnitsMinus1;
        public bool QpprimeYZeroTransformBypassFlag;
    }
}
