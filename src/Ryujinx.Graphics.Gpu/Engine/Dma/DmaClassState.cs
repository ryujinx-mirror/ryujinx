// This file was auto-generated from NVIDIA official Maxwell definitions.

namespace Ryujinx.Graphics.Gpu.Engine.Dma
{
    /// <summary>
    /// Physical mode target.
    /// </summary>
    enum SetPhysModeTarget
    {
        LocalFb = 0,
        CoherentSysmem = 1,
        NoncoherentSysmem = 2,
    }

    /// <summary>
    /// DMA data transfer type.
    /// </summary>
    enum LaunchDmaDataTransferType
    {
        None = 0,
        Pipelined = 1,
        NonPipelined = 2,
    }

    /// <summary>
    /// DMA semaphore type.
    /// </summary>
    enum LaunchDmaSemaphoreType
    {
        None = 0,
        ReleaseOneWordSemaphore = 1,
        ReleaseFourWordSemaphore = 2,
    }

    /// <summary>
    /// DMA interrupt type.
    /// </summary>
    enum LaunchDmaInterruptType
    {
        None = 0,
        Blocking = 1,
        NonBlocking = 2,
    }

    /// <summary>
    /// DMA destination memory layout.
    /// </summary>
    enum LaunchDmaMemoryLayout
    {
        Blocklinear = 0,
        Pitch = 1,
    }

    /// <summary>
    /// DMA type.
    /// </summary>
    enum LaunchDmaType
    {
        Virtual = 0,
        Physical = 1,
    }

    /// <summary>
    /// DMA semaphore reduction operation.
    /// </summary>
    enum LaunchDmaSemaphoreReduction
    {
        Imin = 0,
        Imax = 1,
        Ixor = 2,
        Iand = 3,
        Ior = 4,
        Iadd = 5,
        Inc = 6,
        Dec = 7,
        Fadd = 10,
    }

    /// <summary>
    /// DMA semaphore reduction signedness.
    /// </summary>
    enum LaunchDmaSemaphoreReductionSign
    {
        Signed = 0,
        Unsigned = 1,
    }

    /// <summary>
    /// DMA L2 cache bypass.
    /// </summary>
    enum LaunchDmaBypassL2
    {
        UsePteSetting = 0,
        ForceVolatile = 1,
    }

    /// <summary>
    /// DMA component remapping source component.
    /// </summary>
    enum SetRemapComponentsDst
    {
        SrcX = 0,
        SrcY = 1,
        SrcZ = 2,
        SrcW = 3,
        ConstA = 4,
        ConstB = 5,
        NoWrite = 6,
    }

    /// <summary>
    /// DMA component remapping component size.
    /// </summary>
    enum SetRemapComponentsComponentSize
    {
        One = 0,
        Two = 1,
        Three = 2,
        Four = 3,
    }

    /// <summary>
    /// DMA component remapping number of components.
    /// </summary>
    enum SetRemapComponentsNumComponents
    {
        One = 0,
        Two = 1,
        Three = 2,
        Four = 3,
    }

    /// <summary>
    /// Width in GOBs of the destination texture.
    /// </summary>
    enum SetBlockSizeWidth
    {
        QuarterGob = 14,
        OneGob = 0,
    }

    /// <summary>
    /// Height in GOBs of the destination texture.
    /// </summary>
    enum SetBlockSizeHeight
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
    enum SetBlockSizeDepth
    {
        OneGob = 0,
        TwoGobs = 1,
        FourGobs = 2,
        EightGobs = 3,
        SixteenGobs = 4,
        ThirtytwoGobs = 5,
    }

    /// <summary>
    /// Height of a single GOB in lines.
    /// </summary>
    enum SetBlockSizeGobHeight
    {
        GobHeightTesla4 = 0,
        GobHeightFermi8 = 1,
    }

    /// <summary>
    /// DMA copy class state.
    /// </summary>
    unsafe struct DmaClassState
    {
#pragma warning disable CS0649 // Field is never assigned to
        public fixed uint Reserved00[64];
        public uint Nop;
        public fixed uint Reserved104[15];
        public uint PmTrigger;
        public fixed uint Reserved144[63];
        public uint SetSemaphoreA;
        public readonly int SetSemaphoreAUpper => (int)(SetSemaphoreA & 0xFF);
        public uint SetSemaphoreB;
        public uint SetSemaphorePayload;
        public fixed uint Reserved24C[2];
        public uint SetRenderEnableA;
        public readonly int SetRenderEnableAUpper => (int)(SetRenderEnableA & 0xFF);
        public uint SetRenderEnableB;
        public uint SetRenderEnableC;
        public readonly int SetRenderEnableCMode => (int)(SetRenderEnableC & 0x7);
        public uint SetSrcPhysMode;
        public readonly SetPhysModeTarget SetSrcPhysModeTarget => (SetPhysModeTarget)(SetSrcPhysMode & 0x3);
        public uint SetDstPhysMode;
        public readonly SetPhysModeTarget SetDstPhysModeTarget => (SetPhysModeTarget)(SetDstPhysMode & 0x3);
        public fixed uint Reserved268[38];
        public uint LaunchDma;
        public readonly LaunchDmaDataTransferType LaunchDmaDataTransferType => (LaunchDmaDataTransferType)(LaunchDma & 0x3);
        public readonly bool LaunchDmaFlushEnable => (LaunchDma & 0x4) != 0;
        public readonly LaunchDmaSemaphoreType LaunchDmaSemaphoreType => (LaunchDmaSemaphoreType)((LaunchDma >> 3) & 0x3);
        public readonly LaunchDmaInterruptType LaunchDmaInterruptType => (LaunchDmaInterruptType)((LaunchDma >> 5) & 0x3);
        public readonly LaunchDmaMemoryLayout LaunchDmaSrcMemoryLayout => (LaunchDmaMemoryLayout)((LaunchDma >> 7) & 0x1);
        public readonly LaunchDmaMemoryLayout LaunchDmaDstMemoryLayout => (LaunchDmaMemoryLayout)((LaunchDma >> 8) & 0x1);
        public readonly bool LaunchDmaMultiLineEnable => (LaunchDma & 0x200) != 0;
        public readonly bool LaunchDmaRemapEnable => (LaunchDma & 0x400) != 0;
        public readonly bool LaunchDmaForceRmwdisable => (LaunchDma & 0x800) != 0;
        public readonly LaunchDmaType LaunchDmaSrcType => (LaunchDmaType)((LaunchDma >> 12) & 0x1);
        public readonly LaunchDmaType LaunchDmaDstType => (LaunchDmaType)((LaunchDma >> 13) & 0x1);
        public readonly LaunchDmaSemaphoreReduction LaunchDmaSemaphoreReduction => (LaunchDmaSemaphoreReduction)((LaunchDma >> 14) & 0xF);
        public readonly LaunchDmaSemaphoreReductionSign LaunchDmaSemaphoreReductionSign => (LaunchDmaSemaphoreReductionSign)((LaunchDma >> 18) & 0x1);
        public readonly bool LaunchDmaSemaphoreReductionEnable => (LaunchDma & 0x80000) != 0;
        public readonly LaunchDmaBypassL2 LaunchDmaBypassL2 => (LaunchDmaBypassL2)((LaunchDma >> 20) & 0x1);
        public fixed uint Reserved304[63];
        public uint OffsetInUpper;
        public readonly int OffsetInUpperUpper => (int)(OffsetInUpper & 0xFF);
        public uint OffsetInLower;
        public uint OffsetOutUpper;
        public readonly int OffsetOutUpperUpper => (int)(OffsetOutUpper & 0xFF);
        public uint OffsetOutLower;
        public uint PitchIn;
        public uint PitchOut;
        public uint LineLengthIn;
        public uint LineCount;
        public fixed uint Reserved420[184];
        public uint SetRemapConstA;
        public uint SetRemapConstB;
        public uint SetRemapComponents;
        public readonly SetRemapComponentsDst SetRemapComponentsDstX => (SetRemapComponentsDst)(SetRemapComponents & 0x7);
        public readonly SetRemapComponentsDst SetRemapComponentsDstY => (SetRemapComponentsDst)((SetRemapComponents >> 4) & 0x7);
        public readonly SetRemapComponentsDst SetRemapComponentsDstZ => (SetRemapComponentsDst)((SetRemapComponents >> 8) & 0x7);
        public readonly SetRemapComponentsDst SetRemapComponentsDstW => (SetRemapComponentsDst)((SetRemapComponents >> 12) & 0x7);
        public readonly SetRemapComponentsComponentSize SetRemapComponentsComponentSize => (SetRemapComponentsComponentSize)((SetRemapComponents >> 16) & 0x3);
        public readonly SetRemapComponentsNumComponents SetRemapComponentsNumSrcComponents => (SetRemapComponentsNumComponents)((SetRemapComponents >> 20) & 0x3);
        public readonly SetRemapComponentsNumComponents SetRemapComponentsNumDstComponents => (SetRemapComponentsNumComponents)((SetRemapComponents >> 24) & 0x3);
        public uint SetDstBlockSize;
        public readonly SetBlockSizeWidth SetDstBlockSizeWidth => (SetBlockSizeWidth)(SetDstBlockSize & 0xF);
        public readonly SetBlockSizeHeight SetDstBlockSizeHeight => (SetBlockSizeHeight)((SetDstBlockSize >> 4) & 0xF);
        public readonly SetBlockSizeDepth SetDstBlockSizeDepth => (SetBlockSizeDepth)((SetDstBlockSize >> 8) & 0xF);
        public readonly SetBlockSizeGobHeight SetDstBlockSizeGobHeight => (SetBlockSizeGobHeight)((SetDstBlockSize >> 12) & 0xF);
        public uint SetDstWidth;
        public uint SetDstHeight;
        public uint SetDstDepth;
        public uint SetDstLayer;
        public uint SetDstOrigin;
        public readonly int SetDstOriginX => (int)(SetDstOrigin & 0xFFFF);
        public readonly int SetDstOriginY => (int)((SetDstOrigin >> 16) & 0xFFFF);
        public uint Reserved724;
        public uint SetSrcBlockSize;
        public readonly SetBlockSizeWidth SetSrcBlockSizeWidth => (SetBlockSizeWidth)(SetSrcBlockSize & 0xF);
        public readonly SetBlockSizeHeight SetSrcBlockSizeHeight => (SetBlockSizeHeight)((SetSrcBlockSize >> 4) & 0xF);
        public readonly SetBlockSizeDepth SetSrcBlockSizeDepth => (SetBlockSizeDepth)((SetSrcBlockSize >> 8) & 0xF);
        public readonly SetBlockSizeGobHeight SetSrcBlockSizeGobHeight => (SetBlockSizeGobHeight)((SetSrcBlockSize >> 12) & 0xF);
        public uint SetSrcWidth;
        public uint SetSrcHeight;
        public uint SetSrcDepth;
        public uint SetSrcLayer;
        public uint SetSrcOrigin;
        public readonly int SetSrcOriginX => (int)(SetSrcOrigin & 0xFFFF);
        public readonly int SetSrcOriginY => (int)((SetSrcOrigin >> 16) & 0xFFFF);
        public fixed uint Reserved740[629];
        public uint PmTriggerEnd;
        public fixed uint Reserved1118[2490];
#pragma warning restore CS0649
    }
}
