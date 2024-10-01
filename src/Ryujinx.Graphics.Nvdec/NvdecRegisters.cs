using Ryujinx.Common.Memory;

namespace Ryujinx.Graphics.Nvdec
{
    struct NvdecRegisters
    {
#pragma warning disable CS0649 // Field is never assigned to
        public Array64<uint> Reserved0;
        public uint Nop;
        public Array63<uint> Reserved104;
        public uint SetApplicationId;
        public uint SetWatchdogTimer;
        public Array14<uint> Reserved208;
        public uint SemaphoreA;
        public uint SemaphoreB;
        public uint SemaphoreC;
        public uint CtxSaveArea;
        public Array44<uint> Reserved254;
        public uint Execute;
        public uint SemaphoreD;
        public Array62<uint> Reserved308;
        public uint SetControlParams;
        public uint SetDrvPicSetupOffset;
        public uint SetInBufBaseOffset;
        public uint SetPictureIndex;
        public uint SetSliceOffsetsBufOffset; // Also used by VC1
        public uint SetColocDataOffset; // Also used by VC1
        public uint SetHistoryOffset; // Used by VC1
        public uint SetDisplayBufSize;
        public uint SetHistogramOffset; // Used by VC1
        public uint SetNvDecStatusOffset;
        public uint SetDisplayBufLumaOffset;
        public uint SetDisplayBufChromaOffset;
        public Array17<uint> SetPictureLumaOffset;
        public Array17<uint> SetPictureChromaOffset;
        public uint SetPicScratchBufOffset;
        public uint SetExternalMvBufferOffset;
        public uint SetCryptoData0Offset;
        public uint SetCryptoData1Offset;
        public Array14<uint> Unknown4C8;
        public uint H264SetMbHistBufOffset;
        public Array15<uint> Unknown504;
        public uint Vp8SetProbDataOffset;
        public uint Vp8SetHeaderPartitionBufBaseOffset;
        public Array14<uint> Unknown548;
        public uint HevcSetScalingListOffset;
        public uint HevcSetTileSizesOffset;
        public uint HevcSetFilterBufferOffset;
        public uint HevcSetSaoBufferOffset;
        public uint HevcSetSliceInfoBufferOffset;
        public uint HevcSetSliceGroupIndex;
        public Array10<uint> Unknown598;
        public uint Vp9SetProbTabBufOffset;
        public uint Vp9SetCtxCounterBufOffset;
        public uint Vp9SetSegmentReadBufOffset;
        public uint Vp9SetSegmentWriteBufOffset;
        public uint Vp9SetTileSizeBufOffset;
        public uint Vp9SetColMvWriteBufOffset;
        public uint Vp9SetColMvReadBufOffset;
        public uint Vp9SetFilterBufferOffset;
#pragma warning restore CS0649
    }
}
