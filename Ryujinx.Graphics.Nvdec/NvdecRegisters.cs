using Ryujinx.Common.Memory;

namespace Ryujinx.Graphics.Nvdec
{
    // Note: Most of those names are not official.
    unsafe struct NvdecRegisters
    {
        public fixed uint Reserved0[128];
        public uint SetCodecID;
        public fixed uint Reserved204[63];
        public uint Execute;
        public fixed uint Reserved304[63];
        public uint SetPlatformID;
        public uint SetPictureInfoOffset;
        public uint SetBitstreamOffset;
        public uint SetFrameNumber;
        public uint SetH264SliceDataOffsetsOffset; // Also used by VC1
        public uint SetH264MvDumpOffset; // Also used by VC1
        public uint Unknown418; // Used by VC1
        public uint Unknown41C;
        public uint Unknown420; // Used by VC1
        public uint SetFrameStatsOffset;
        public uint SetH264LastSurfaceLumaOffset;
        public uint SetH264LastSurfaceChromaOffset;
        public Array17<uint> SetSurfaceLumaOffset;
        public Array17<uint> SetSurfaceChromaOffset;
        public uint Unknown4B8;
        public uint Unknown4BC;
        public uint SetCryptoData0Offset;
        public uint SetCryptoData1Offset;
        public Array62<uint> Unknown4C8;
        public uint SetVp9EntropyProbsOffset;
        public uint SetVp9BackwardUpdatesOffset;
        public uint SetVp9LastFrameSegMapOffset;
        public uint SetVp9CurrFrameSegMapOffset;
        public uint Unknown5D0;
        public uint SetVp9LastFrameMvsOffset;
        public uint SetVp9CurrFrameMvsOffset;
        public uint Unknown5DC;
    }
}
