// This file was auto-generated from NVIDIA official Maxwell definitions.

using Ryujinx.Common.Memory;

namespace Ryujinx.Graphics.Gpu.Engine.GPFifo
{
    enum SemaphoredOperation
    {
        Acquire = 1,
        Release = 2,
        AcqGeq = 4,
        AcqAnd = 8,
        Reduction = 16
    }

    enum SemaphoredAcquireSwitch
    {
        Disabled = 0,
        Enabled = 1
    }

    enum SemaphoredReleaseWfi
    {
        En = 0,
        Dis = 1
    }

    enum SemaphoredReleaseSize
    {
        SixteenBytes = 0,
        FourBytes = 1
    }

    enum SemaphoredReduction
    {
        Min = 0,
        Max = 1,
        Xor = 2,
        And = 3,
        Or = 4,
        Add = 5,
        Inc = 6,
        Dec = 7
    }

    enum SemaphoredFormat
    {
        Signed = 0,
        Unsigned = 1
    }

    enum MemOpCTlbInvalidatePdb
    {
        One = 0,
        All = 1
    }

    enum MemOpCTlbInvalidateGpc
    {
        Enable = 0,
        Disable = 1
    }

    enum MemOpCTlbInvalidateTarget
    {
        VidMem = 0,
        SysMemCoherent = 2,
        SysMemNoncoherent = 3
    }

    enum MemOpDOperation
    {
        Membar = 5,
        MmuTlbInvalidate = 9,
        L2PeermemInvalidate = 13,
        L2SysmemInvalidate = 14,
        L2CleanComptags = 15,
        L2FlushDirty = 16
    }

    enum SyncpointbOperation
    {
        Wait = 0,
        Incr = 1
    }

    enum SyncpointbWaitSwitch
    {
        Dis = 0,
        En = 1
    }

    enum WfiScope
    {
        CurrentScgType = 0,
        All = 1
    }

    enum YieldOp
    {
        Nop = 0,
        PbdmaTimeslice = 1,
        RunlistTimeslice = 2,
        Tsg = 3
    }

    struct GPFifoClassState
    {
#pragma warning disable CS0649
        public uint SetObject;
        public int SetObjectNvclass => (int)((SetObject >> 0) & 0xFFFF);
        public int SetObjectEngine => (int)((SetObject >> 16) & 0x1F);
        public uint Illegal;
        public int IllegalHandle => (int)(Illegal);
        public uint Nop;
        public int NopHandle => (int)(Nop);
        public uint Reserved0C;
        public uint Semaphorea;
        public int SemaphoreaOffsetUpper => (int)((Semaphorea >> 0) & 0xFF);
        public uint Semaphoreb;
        public int SemaphorebOffsetLower => (int)((Semaphoreb >> 2) & 0x3FFFFFFF);
        public uint Semaphorec;
        public int SemaphorecPayload => (int)(Semaphorec);
        public uint Semaphored;
        public SemaphoredOperation SemaphoredOperation => (SemaphoredOperation)((Semaphored >> 0) & 0x1F);
        public SemaphoredAcquireSwitch SemaphoredAcquireSwitch => (SemaphoredAcquireSwitch)((Semaphored >> 12) & 0x1);
        public SemaphoredReleaseWfi SemaphoredReleaseWfi => (SemaphoredReleaseWfi)((Semaphored >> 20) & 0x1);
        public SemaphoredReleaseSize SemaphoredReleaseSize => (SemaphoredReleaseSize)((Semaphored >> 24) & 0x1);
        public SemaphoredReduction SemaphoredReduction => (SemaphoredReduction)((Semaphored >> 27) & 0xF);
        public SemaphoredFormat SemaphoredFormat => (SemaphoredFormat)((Semaphored >> 31) & 0x1);
        public uint NonStallInterrupt;
        public int NonStallInterruptHandle => (int)(NonStallInterrupt);
        public uint FbFlush;
        public int FbFlushHandle => (int)(FbFlush);
        public uint Reserved28;
        public uint Reserved2C;
        public uint MemOpC;
        public int MemOpCOperandLow => (int)((MemOpC >> 2) & 0x3FFFFFFF);
        public MemOpCTlbInvalidatePdb MemOpCTlbInvalidatePdb => (MemOpCTlbInvalidatePdb)((MemOpC >> 0) & 0x1);
        public MemOpCTlbInvalidateGpc MemOpCTlbInvalidateGpc => (MemOpCTlbInvalidateGpc)((MemOpC >> 1) & 0x1);
        public MemOpCTlbInvalidateTarget MemOpCTlbInvalidateTarget => (MemOpCTlbInvalidateTarget)((MemOpC >> 10) & 0x3);
        public int MemOpCTlbInvalidateAddrLo => (int)((MemOpC >> 12) & 0xFFFFF);
        public uint MemOpD;
        public int MemOpDOperandHigh => (int)((MemOpD >> 0) & 0xFF);
        public MemOpDOperation MemOpDOperation => (MemOpDOperation)((MemOpD >> 27) & 0x1F);
        public int MemOpDTlbInvalidateAddrHi => (int)((MemOpD >> 0) & 0xFF);
        public uint Reserved38;
        public uint Reserved3C;
        public uint Reserved40;
        public uint Reserved44;
        public uint Reserved48;
        public uint Reserved4C;
        public uint SetReference;
        public int SetReferenceCount => (int)(SetReference);
        public uint Reserved54;
        public uint Reserved58;
        public uint Reserved5C;
        public uint Reserved60;
        public uint Reserved64;
        public uint Reserved68;
        public uint Reserved6C;
        public uint Syncpointa;
        public int SyncpointaPayload => (int)(Syncpointa);
        public uint Syncpointb;
        public SyncpointbOperation SyncpointbOperation => (SyncpointbOperation)((Syncpointb >> 0) & 0x1);
        public SyncpointbWaitSwitch SyncpointbWaitSwitch => (SyncpointbWaitSwitch)((Syncpointb >> 4) & 0x1);
        public int SyncpointbSyncptIndex => (int)((Syncpointb >> 8) & 0xFFF);
        public uint Wfi;
        public WfiScope WfiScope => (WfiScope)((Wfi >> 0) & 0x1);
        public uint CrcCheck;
        public int CrcCheckValue => (int)(CrcCheck);
        public uint Yield;
        public YieldOp YieldOp => (YieldOp)((Yield >> 0) & 0x3);
        // TODO: Eventually move this to per-engine state.
        public Array31<uint> Reserved84;
        public uint NoOperation;
        public uint SetNotifyA;
        public uint SetNotifyB;
        public uint Notify;
        public uint WaitForIdle;
        public uint LoadMmeInstructionRamPointer;
        public uint LoadMmeInstructionRam;
        public uint LoadMmeStartAddressRamPointer;
        public uint LoadMmeStartAddressRam;
        public uint SetMmeShadowRamControl;
#pragma warning restore CS0649
    }
}
