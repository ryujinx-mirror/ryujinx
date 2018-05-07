namespace Ryujinx.Core.OsHle.Services.Nv.NvGpuGpu
{
    struct NvGpuGpuGetCharacteristics
    {
        public long BufferSize;
        public long BufferAddress;
        public int  Arch;
        public int  Impl;
        public int  Rev;
        public int  NumGpc;
        public long L2CacheSize;
        public long OnBoardVideoMemorySize;
        public int  NumTpcPerGpc;
        public int  BusType;
        public int  BigPageSize;
        public int  CompressionPageSize;
        public int  PdeCoverageBitCount;
        public int  AvailableBigPageSizes;
        public int  GpcMask;
        public int  SmArchSmVersion;
        public int  SmArchSpaVersion;
        public int  SmArchWarpCount;
        public int  GpuVaBitCount;
        public int  Reserved;
        public long Flags;
        public int  TwodClass;
        public int  ThreedClass;
        public int  ComputeClass;
        public int  GpfifoClass;
        public int  InlineToMemoryClass;
        public int  DmaCopyClass;
        public int  MaxFbpsCount;
        public int  FbpEnMask;
        public int  MaxLtcPerFbp;
        public int  MaxLtsPerLtc;
        public int  MaxTexPerTpc;
        public int  MaxGpcCount;
        public int  RopL2EnMask0;
        public int  RopL2EnMask1;
        public long ChipName;
        public long GrCompbitStoreBaseHw;
    }
}