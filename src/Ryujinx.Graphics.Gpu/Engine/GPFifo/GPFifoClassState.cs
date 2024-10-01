// This file was auto-generated from NVIDIA official Maxwell definitions.

using Ryujinx.Common.Memory;

namespace Ryujinx.Graphics.Gpu.Engine.GPFifo
{
    /// <summary>
    /// Semaphore operation.
    /// </summary>
    enum SemaphoredOperation
    {
        Acquire = 1,
        Release = 2,
        AcqGeq = 4,
        AcqAnd = 8,
        Reduction = 16,
    }

    /// <summary>
    /// Semaphore acquire switch enable.
    /// </summary>
    enum SemaphoredAcquireSwitch
    {
        Disabled = 0,
        Enabled = 1,
    }

    /// <summary>
    /// Semaphore release interrupt wait enable.
    /// </summary>
    enum SemaphoredReleaseWfi
    {
        En = 0,
        Dis = 1,
    }

    /// <summary>
    /// Semaphore release structure size.
    /// </summary>
    enum SemaphoredReleaseSize
    {
        SixteenBytes = 0,
        FourBytes = 1,
    }

    /// <summary>
    /// Semaphore reduction operation.
    /// </summary>
    enum SemaphoredReduction
    {
        Min = 0,
        Max = 1,
        Xor = 2,
        And = 3,
        Or = 4,
        Add = 5,
        Inc = 6,
        Dec = 7,
    }

    /// <summary>
    /// Semaphore format.
    /// </summary>
    enum SemaphoredFormat
    {
        Signed = 0,
        Unsigned = 1,
    }

    /// <summary>
    /// Memory Translation Lookaside Buffer Page Directory Buffer invalidation.
    /// </summary>
    enum MemOpCTlbInvalidatePdb
    {
        One = 0,
        All = 1,
    }

    /// <summary>
    /// Memory Translation Lookaside Buffer GPC invalidation enable.
    /// </summary>
    enum MemOpCTlbInvalidateGpc
    {
        Enable = 0,
        Disable = 1,
    }

    /// <summary>
    /// Memory Translation Lookaside Buffer invalidation target.
    /// </summary>
    enum MemOpCTlbInvalidateTarget
    {
        VidMem = 0,
        SysMemCoherent = 2,
        SysMemNoncoherent = 3,
    }

    /// <summary>
    /// Memory operation.
    /// </summary>
    enum MemOpDOperation
    {
        Membar = 5,
        MmuTlbInvalidate = 9,
        L2PeermemInvalidate = 13,
        L2SysmemInvalidate = 14,
        L2CleanComptags = 15,
        L2FlushDirty = 16,
    }

    /// <summary>
    /// Syncpoint operation.
    /// </summary>
    enum SyncpointbOperation
    {
        Wait = 0,
        Incr = 1,
    }

    /// <summary>
    /// Syncpoint wait switch enable.
    /// </summary>
    enum SyncpointbWaitSwitch
    {
        Dis = 0,
        En = 1,
    }

    /// <summary>
    /// Wait for interrupt scope.
    /// </summary>
    enum WfiScope
    {
        CurrentScgType = 0,
        All = 1,
    }

    /// <summary>
    /// Yield operation.
    /// </summary>
    enum YieldOp
    {
        Nop = 0,
        PbdmaTimeslice = 1,
        RunlistTimeslice = 2,
        Tsg = 3,
    }

    /// <summary>
    /// General Purpose FIFO class state.
    /// </summary>
    struct GPFifoClassState
    {
#pragma warning disable CS0649 // Field is never assigned to
        public uint SetObject;
        public readonly int SetObjectNvclass => (int)(SetObject & 0xFFFF);
        public readonly int SetObjectEngine => (int)((SetObject >> 16) & 0x1F);
        public uint Illegal;
        public readonly int IllegalHandle => (int)(Illegal);
        public uint Nop;
        public readonly int NopHandle => (int)(Nop);
        public uint Reserved0C;
        public uint Semaphorea;
        public readonly int SemaphoreaOffsetUpper => (int)(Semaphorea & 0xFF);
        public uint Semaphoreb;
        public readonly int SemaphorebOffsetLower => (int)((Semaphoreb >> 2) & 0x3FFFFFFF);
        public uint Semaphorec;
        public readonly int SemaphorecPayload => (int)(Semaphorec);
        public uint Semaphored;
        public readonly SemaphoredOperation SemaphoredOperation => (SemaphoredOperation)(Semaphored & 0x1F);
        public readonly SemaphoredAcquireSwitch SemaphoredAcquireSwitch => (SemaphoredAcquireSwitch)((Semaphored >> 12) & 0x1);
        public readonly SemaphoredReleaseWfi SemaphoredReleaseWfi => (SemaphoredReleaseWfi)((Semaphored >> 20) & 0x1);
        public readonly SemaphoredReleaseSize SemaphoredReleaseSize => (SemaphoredReleaseSize)((Semaphored >> 24) & 0x1);
        public readonly SemaphoredReduction SemaphoredReduction => (SemaphoredReduction)((Semaphored >> 27) & 0xF);
        public readonly SemaphoredFormat SemaphoredFormat => (SemaphoredFormat)((Semaphored >> 31) & 0x1);
        public uint NonStallInterrupt;
        public readonly int NonStallInterruptHandle => (int)(NonStallInterrupt);
        public uint FbFlush;
        public readonly int FbFlushHandle => (int)(FbFlush);
        public uint Reserved28;
        public uint Reserved2C;
        public uint MemOpC;
        public readonly int MemOpCOperandLow => (int)((MemOpC >> 2) & 0x3FFFFFFF);
        public readonly MemOpCTlbInvalidatePdb MemOpCTlbInvalidatePdb => (MemOpCTlbInvalidatePdb)(MemOpC & 0x1);
        public readonly MemOpCTlbInvalidateGpc MemOpCTlbInvalidateGpc => (MemOpCTlbInvalidateGpc)((MemOpC >> 1) & 0x1);
        public readonly MemOpCTlbInvalidateTarget MemOpCTlbInvalidateTarget => (MemOpCTlbInvalidateTarget)((MemOpC >> 10) & 0x3);
        public readonly int MemOpCTlbInvalidateAddrLo => (int)((MemOpC >> 12) & 0xFFFFF);
        public uint MemOpD;
        public readonly int MemOpDOperandHigh => (int)(MemOpD & 0xFF);
        public readonly MemOpDOperation MemOpDOperation => (MemOpDOperation)((MemOpD >> 27) & 0x1F);
        public readonly int MemOpDTlbInvalidateAddrHi => (int)(MemOpD & 0xFF);
        public uint Reserved38;
        public uint Reserved3C;
        public uint Reserved40;
        public uint Reserved44;
        public uint Reserved48;
        public uint Reserved4C;
        public uint SetReference;
        public readonly int SetReferenceCount => (int)(SetReference);
        public uint Reserved54;
        public uint Reserved58;
        public uint Reserved5C;
        public uint Reserved60;
        public uint Reserved64;
        public uint Reserved68;
        public uint Reserved6C;
        public uint Syncpointa;
        public readonly int SyncpointaPayload => (int)(Syncpointa);
        public uint Syncpointb;
        public readonly SyncpointbOperation SyncpointbOperation => (SyncpointbOperation)(Syncpointb & 0x1);
        public readonly SyncpointbWaitSwitch SyncpointbWaitSwitch => (SyncpointbWaitSwitch)((Syncpointb >> 4) & 0x1);
        public readonly int SyncpointbSyncptIndex => (int)((Syncpointb >> 8) & 0xFFF);
        public uint Wfi;
        public readonly WfiScope WfiScope => (WfiScope)(Wfi & 0x1);
        public uint CrcCheck;
        public readonly int CrcCheckValue => (int)(CrcCheck);
        public uint Yield;
        public readonly YieldOp YieldOp => (YieldOp)(Yield & 0x3);
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
