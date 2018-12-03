using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.VDec
{
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    struct Vp9FrameDimensions
    {
        public short Width;
        public short Height;
        public short SubsamplingX; //?
        public short SubsamplingY; //?
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct Vp9FrameHeader
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public Vp9FrameDimensions[] RefFrames;

        public Vp9FrameDimensions CurrentFrame;

        public int Flags;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] RefFrameSignBias;

        public byte LoopFilterLevel;
        public byte LoopFilterSharpness;

        public byte  BaseQIndex;
        public sbyte DeltaQYDc;
        public sbyte DeltaQUvDc;
        public sbyte DeltaQUvAc;

        [MarshalAs(UnmanagedType.I1)]
        public bool Lossless;

        public byte TxMode;

        [MarshalAs(UnmanagedType.I1)]
        public bool AllowHighPrecisionMv;

        public byte RawInterpolationFilter;
        public byte CompPredMode;
        public byte FixCompRef;
        public byte VarCompRef0;
        public byte VarCompRef1;

        public byte TileColsLog2;
        public byte TileRowsLog2;

        [MarshalAs(UnmanagedType.I1)]
        public bool SegmentationEnabled;

        [MarshalAs(UnmanagedType.I1)]
        public bool SegmentationUpdate;

        [MarshalAs(UnmanagedType.I1)]
        public bool SegmentationTemporalUpdate;

        [MarshalAs(UnmanagedType.I1)]
        public bool SegmentationAbsOrDeltaUpdate;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8 * 4, ArraySubType = UnmanagedType.I1)]
        public bool[] FeatureEnabled;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8 * 4)]
        public short[] FeatureData;

        [MarshalAs(UnmanagedType.I1)]
        public bool LoopFilterDeltaEnabled;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public sbyte[] LoopFilterRefDeltas;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public sbyte[] LoopFilterModeDeltas;
    }
}