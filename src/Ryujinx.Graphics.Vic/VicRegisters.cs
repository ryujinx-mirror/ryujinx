using Ryujinx.Common.Memory;

namespace Ryujinx.Graphics.Vic
{
    struct PlaneOffsets
    {
#pragma warning disable CS0649 // Field is never assigned to
        public uint LumaOffset;
        public uint ChromaUOffset;
        public uint ChromaVOffset;
#pragma warning restore CS0649
    }

    struct VicRegisters
    {
#pragma warning disable CS0649 // Field is never assigned to
        public Array64<uint> Reserved0;
        public uint Nop;
        public Array15<uint> Reserved104;
        public uint PmTrigger;
        public Array47<uint> Reserved144;
        public uint SetApplicationID;
        public uint SetWatchdogTimer;
        public Array14<uint> Reserved208;
        public uint SemaphoreA;
        public uint SemaphoreB;
        public uint SemaphoreC;
        public uint CtxSaveArea;
        public uint CtxSwitch;
        public Array43<uint> Reserved254;
        public uint Execute;
        public uint SemaphoreD;
        public Array62<uint> Reserved308;
        public Array8<Array8<PlaneOffsets>> SetSurfacexSlotx;
        public uint SetPictureIndex;
        public uint SetControlParams;
        public uint SetConfigStructOffset;
        public uint SetFilterStructOffset;
        public uint SetPaletteOffset;
        public uint SetHistOffset;
        public uint SetContextId;
        public uint SetFceUcodeSize;
        public PlaneOffsets SetOutputSurface;
        public uint SetFceUcodeOffset;
        public Array4<uint> Reserved730;
        public Array8<uint> SetSlotContextId;
        public Array8<uint> SetCompTagBufferOffset;
        public Array8<uint> SetHistoryBufferOffset;
#pragma warning restore CS0649
    }
}
