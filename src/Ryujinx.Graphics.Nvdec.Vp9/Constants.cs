namespace Ryujinx.Graphics.Nvdec.Vp9
{
    internal static class Constants
    {
        public const int Vp9InterpExtend = 4;

        public const int MaxMbPlane = 3;

        public const int None = -1;
        public const int IntraFrame = 0;
        public const int LastFrame = 1;
        public const int GoldenFrame = 2;
        public const int AltRefFrame = 3;
        public const int MaxRefFrames = 4;

        public const int MiSizeLog2 = 3;
        public const int MiBlockSizeLog2 = 6 - MiSizeLog2;  // 64 = 2^6

        public const int MiSize = 1 << MiSizeLog2;              // pixels per mi-unit
        public const int MiBlockSize = 1 << MiBlockSizeLog2;  // mi-units per max block
        public const int MiMask = MiBlockSize - 1;

        public const int PartitionPloffset = 4;  // number of probability models per block size

        /* Segment Feature Masks */
        public const int MaxMvRefCandidates = 2;

        public const int CompInterContexts = 5;
        public const int RefContexts = 5;

        public const int EightTap = 0;
        public const int EightTapSmooth = 1;
        public const int EightTapSharp = 2;
        public const int SwitchableFilters = 3; /* Number of switchable filters */
        public const int Bilinear = 3;
        public const int Switchable = 4; /* should be the last one */

        // Frame
        public const int RefsPerFrame = 3;

        public const int NumPingPongBuffers = 2;

        public const int Class0Bits = 1; /* bits at integer precision for class 0 */
        public const int Class0Size = 1 << Class0Bits;

        public const int MvInUseBits = 14;
        public const int MvUpp = (1 << MvInUseBits) - 1;
        public const int MvLow = -(1 << MvInUseBits);

        // Coefficient token alphabet
        public const int ZeroToken = 0;        // 0     Extra Bits 0+0
        public const int OneToken = 1;         // 1     Extra Bits 0+1
        public const int TwoToken = 2;         // 2     Extra Bits 0+1

        public const int PivotNode = 2;

        public const int Cat1MinVal = 5;
        public const int Cat2MinVal = 7;
        public const int Cat3MinVal = 11;
        public const int Cat4MinVal = 19;
        public const int Cat5MinVal = 35;
        public const int Cat6MinVal = 67;

        public const int EobModelToken = 3;

        public const int SegmentAbsData = 1;
        public const int MaxSegments = 8;
    }
}
