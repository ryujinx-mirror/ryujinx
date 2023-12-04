// This file was auto-generated from NVIDIA official Maxwell definitions.

using Ryujinx.Common.Memory;

namespace Ryujinx.Graphics.Gpu.Engine.InlineToMemory
{
    /// <summary>
    /// Notify type.
    /// </summary>
    enum NotifyType
    {
        WriteOnly = 0,
        WriteThenAwaken = 1,
    }

    /// <summary>
    /// Width in GOBs of the destination texture.
    /// </summary>
    enum SetDstBlockSizeWidth
    {
        OneGob = 0,
    }

    /// <summary>
    /// Height in GOBs of the destination texture.
    /// </summary>
    enum SetDstBlockSizeHeight
    {
        OneGob = 0,
        TwoGobs = 1,
        FourGobs = 2,
        EightGobs = 3,
        SixteenGobs = 4,
        ThirtytwoGobs = 5,
    }

    /// <summary>
    /// Depth in GOBs of the destination texture.
    /// </summary>
    enum SetDstBlockSizeDepth
    {
        OneGob = 0,
        TwoGobs = 1,
        FourGobs = 2,
        EightGobs = 3,
        SixteenGobs = 4,
        ThirtytwoGobs = 5,
    }

    /// <summary>
    /// Memory layout of the destination texture.
    /// </summary>
    enum LaunchDmaDstMemoryLayout
    {
        Blocklinear = 0,
        Pitch = 1,
    }

    /// <summary>
    /// DMA completion type.
    /// </summary>
    enum LaunchDmaCompletionType
    {
        FlushDisable = 0,
        FlushOnly = 1,
        ReleaseSemaphore = 2,
    }

    /// <summary>
    /// DMA interrupt type.
    /// </summary>
    enum LaunchDmaInterruptType
    {
        None = 0,
        Interrupt = 1,
    }

    /// <summary>
    /// DMA semaphore structure size.
    /// </summary>
    enum LaunchDmaSemaphoreStructSize
    {
        FourWords = 0,
        OneWord = 1,
    }

    /// <summary>
    /// DMA semaphore reduction operation.
    /// </summary>
    enum LaunchDmaReductionOp
    {
        RedAdd = 0,
        RedMin = 1,
        RedMax = 2,
        RedInc = 3,
        RedDec = 4,
        RedAnd = 5,
        RedOr = 6,
        RedXor = 7,
    }

    /// <summary>
    /// DMA semaphore reduction format.
    /// </summary>
    enum LaunchDmaReductionFormat
    {
        Unsigned32 = 0,
        Signed32 = 1,
    }

    /// <summary>
    /// Inline-to-Memory class state.
    /// </summary>
    unsafe struct InlineToMemoryClassState
    {
#pragma warning disable CS0649 // Field is never assigned to
        public uint SetObject;
        public readonly int SetObjectClassId => (int)(SetObject & 0xFFFF);
        public readonly int SetObjectEngineId => (int)((SetObject >> 16) & 0x1F);
        public fixed uint Reserved04[63];
        public uint NoOperation;
        public uint SetNotifyA;
        public readonly int SetNotifyAAddressUpper => (int)(SetNotifyA & 0xFF);
        public uint SetNotifyB;
        public uint Notify;
        public readonly NotifyType NotifyType => (NotifyType)(Notify);
        public uint WaitForIdle;
        public fixed uint Reserved114[7];
        public uint SetGlobalRenderEnableA;
        public readonly int SetGlobalRenderEnableAOffsetUpper => (int)(SetGlobalRenderEnableA & 0xFF);
        public uint SetGlobalRenderEnableB;
        public uint SetGlobalRenderEnableC;
        public readonly int SetGlobalRenderEnableCMode => (int)(SetGlobalRenderEnableC & 0x7);
        public uint SendGoIdle;
        public uint PmTrigger;
        public uint PmTriggerWfi;
        public fixed uint Reserved148[2];
        public uint SetInstrumentationMethodHeader;
        public uint SetInstrumentationMethodData;
        public fixed uint Reserved158[10];
        public uint LineLengthIn;
        public uint LineCount;
        public uint OffsetOutUpper;
        public readonly int OffsetOutUpperValue => (int)(OffsetOutUpper & 0xFF);
        public uint OffsetOut;
        public uint PitchOut;
        public uint SetDstBlockSize;
        public readonly SetDstBlockSizeWidth SetDstBlockSizeWidth => (SetDstBlockSizeWidth)(SetDstBlockSize & 0xF);
        public readonly SetDstBlockSizeHeight SetDstBlockSizeHeight => (SetDstBlockSizeHeight)((SetDstBlockSize >> 4) & 0xF);
        public readonly SetDstBlockSizeDepth SetDstBlockSizeDepth => (SetDstBlockSizeDepth)((SetDstBlockSize >> 8) & 0xF);
        public uint SetDstWidth;
        public uint SetDstHeight;
        public uint SetDstDepth;
        public uint SetDstLayer;
        public uint SetDstOriginBytesX;
        public readonly int SetDstOriginBytesXV => (int)(SetDstOriginBytesX & 0xFFFFF);
        public uint SetDstOriginSamplesY;
        public readonly int SetDstOriginSamplesYV => (int)(SetDstOriginSamplesY & 0xFFFF);
        public uint LaunchDma;
        public readonly LaunchDmaDstMemoryLayout LaunchDmaDstMemoryLayout => (LaunchDmaDstMemoryLayout)(LaunchDma & 0x1);
        public readonly LaunchDmaCompletionType LaunchDmaCompletionType => (LaunchDmaCompletionType)((LaunchDma >> 4) & 0x3);
        public readonly LaunchDmaInterruptType LaunchDmaInterruptType => (LaunchDmaInterruptType)((LaunchDma >> 8) & 0x3);
        public readonly LaunchDmaSemaphoreStructSize LaunchDmaSemaphoreStructSize => (LaunchDmaSemaphoreStructSize)((LaunchDma >> 12) & 0x1);
        public readonly bool LaunchDmaReductionEnable => (LaunchDma & 0x2) != 0;
        public readonly LaunchDmaReductionOp LaunchDmaReductionOp => (LaunchDmaReductionOp)((LaunchDma >> 13) & 0x7);
        public readonly LaunchDmaReductionFormat LaunchDmaReductionFormat => (LaunchDmaReductionFormat)((LaunchDma >> 2) & 0x3);
        public readonly bool LaunchDmaSysmembarDisable => (LaunchDma & 0x40) != 0;
        public uint LoadInlineData;
        public fixed uint Reserved1B8[9];
        public uint SetI2mSemaphoreA;
        public readonly int SetI2mSemaphoreAOffsetUpper => (int)(SetI2mSemaphoreA & 0xFF);
        public uint SetI2mSemaphoreB;
        public uint SetI2mSemaphoreC;
        public fixed uint Reserved1E8[2];
        public uint SetI2mSpareNoop00;
        public uint SetI2mSpareNoop01;
        public uint SetI2mSpareNoop02;
        public uint SetI2mSpareNoop03;
        public fixed uint Reserved200[3200];
        public Array256<uint> SetMmeShadowScratch;
#pragma warning restore CS0649
    }
}
