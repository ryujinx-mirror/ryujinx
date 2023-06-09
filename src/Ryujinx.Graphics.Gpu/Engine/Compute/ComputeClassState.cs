// This file was auto-generated from NVIDIA official Maxwell definitions.

using Ryujinx.Common.Memory;
using Ryujinx.Graphics.Gpu.Engine.InlineToMemory;

namespace Ryujinx.Graphics.Gpu.Engine.Compute
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
    /// CWD control SM selection.
    /// </summary>
    enum SetCwdControlSmSelection
    {
        LoadBalanced = 0,
        RoundRobin = 1,
    }

    /// <summary>
    /// Cache lines to invalidate.
    /// </summary>
    enum InvalidateCacheLines
    {
        All = 0,
        One = 1,
    }

    /// <summary>
    /// GWC SCG type.
    /// </summary>
    enum SetGwcScgTypeScgType
    {
        GraphicsCompute0 = 0,
        Compute1 = 1,
    }

    /// <summary>
    /// Render enable override mode.
    /// </summary>
    enum SetRenderEnableOverrideMode
    {
        UseRenderEnable = 0,
        AlwaysRender = 1,
        NeverRender = 2,
    }

    /// <summary>
    /// Semaphore report operation.
    /// </summary>
    enum SetReportSemaphoreDOperation
    {
        Release = 0,
        Trap = 3,
    }

    /// <summary>
    /// Semaphore report structure size.
    /// </summary>
    enum SetReportSemaphoreDStructureSize
    {
        FourWords = 0,
        OneWord = 1,
    }

    /// <summary>
    /// Semaphore report reduction operation.
    /// </summary>
    enum SetReportSemaphoreDReductionOp
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
    /// Semaphore report reduction format.
    /// </summary>
    enum SetReportSemaphoreDReductionFormat
    {
        Unsigned32 = 0,
        Signed32 = 1,
    }

    /// <summary>
    /// Compute class state.
    /// </summary>
    unsafe struct ComputeClassState
    {
#pragma warning disable CS0649
        public uint SetObject;
        public int SetObjectClassId => (int)(SetObject & 0xFFFF);
        public int SetObjectEngineId => (int)((SetObject >> 16) & 0x1F);
        public fixed uint Reserved04[63];
        public uint NoOperation;
        public uint SetNotifyA;
        public int SetNotifyAAddressUpper => (int)(SetNotifyA & 0xFF);
        public uint SetNotifyB;
        public uint Notify;
        public NotifyType NotifyType => (NotifyType)(Notify);
        public uint WaitForIdle;
        public fixed uint Reserved114[7];
        public uint SetGlobalRenderEnableA;
        public int SetGlobalRenderEnableAOffsetUpper => (int)(SetGlobalRenderEnableA & 0xFF);
        public uint SetGlobalRenderEnableB;
        public uint SetGlobalRenderEnableC;
        public int SetGlobalRenderEnableCMode => (int)(SetGlobalRenderEnableC & 0x7);
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
        public int OffsetOutUpperValue => (int)(OffsetOutUpper & 0xFF);
        public uint OffsetOut;
        public uint PitchOut;
        public uint SetDstBlockSize;
        public SetDstBlockSizeWidth SetDstBlockSizeWidth => (SetDstBlockSizeWidth)(SetDstBlockSize & 0xF);
        public SetDstBlockSizeHeight SetDstBlockSizeHeight => (SetDstBlockSizeHeight)((SetDstBlockSize >> 4) & 0xF);
        public SetDstBlockSizeDepth SetDstBlockSizeDepth => (SetDstBlockSizeDepth)((SetDstBlockSize >> 8) & 0xF);
        public uint SetDstWidth;
        public uint SetDstHeight;
        public uint SetDstDepth;
        public uint SetDstLayer;
        public uint SetDstOriginBytesX;
        public int SetDstOriginBytesXV => (int)(SetDstOriginBytesX & 0xFFFFF);
        public uint SetDstOriginSamplesY;
        public int SetDstOriginSamplesYV => (int)(SetDstOriginSamplesY & 0xFFFF);
        public uint LaunchDma;
        public LaunchDmaDstMemoryLayout LaunchDmaDstMemoryLayout => (LaunchDmaDstMemoryLayout)(LaunchDma & 0x1);
        public LaunchDmaCompletionType LaunchDmaCompletionType => (LaunchDmaCompletionType)((LaunchDma >> 4) & 0x3);
        public LaunchDmaInterruptType LaunchDmaInterruptType => (LaunchDmaInterruptType)((LaunchDma >> 8) & 0x3);
        public LaunchDmaSemaphoreStructSize LaunchDmaSemaphoreStructSize => (LaunchDmaSemaphoreStructSize)((LaunchDma >> 12) & 0x1);
        public bool LaunchDmaReductionEnable => (LaunchDma & 0x2) != 0;
        public LaunchDmaReductionOp LaunchDmaReductionOp => (LaunchDmaReductionOp)((LaunchDma >> 13) & 0x7);
        public LaunchDmaReductionFormat LaunchDmaReductionFormat => (LaunchDmaReductionFormat)((LaunchDma >> 2) & 0x3);
        public bool LaunchDmaSysmembarDisable => (LaunchDma & 0x40) != 0;
        public uint LoadInlineData;
        public fixed uint Reserved1B8[9];
        public uint SetI2mSemaphoreA;
        public int SetI2mSemaphoreAOffsetUpper => (int)(SetI2mSemaphoreA & 0xFF);
        public uint SetI2mSemaphoreB;
        public uint SetI2mSemaphoreC;
        public fixed uint Reserved1E8[2];
        public uint SetI2mSpareNoop00;
        public uint SetI2mSpareNoop01;
        public uint SetI2mSpareNoop02;
        public uint SetI2mSpareNoop03;
        public uint SetValidSpanOverflowAreaA;
        public int SetValidSpanOverflowAreaAAddressUpper => (int)(SetValidSpanOverflowAreaA & 0xFF);
        public uint SetValidSpanOverflowAreaB;
        public uint SetValidSpanOverflowAreaC;
        public uint SetCoalesceWaitingPeriodUnit;
        public uint PerfmonTransfer;
        public uint SetShaderSharedMemoryWindow;
        public uint SetSelectMaxwellTextureHeaders;
        public bool SetSelectMaxwellTextureHeadersV => (SetSelectMaxwellTextureHeaders & 0x1) != 0;
        public uint InvalidateShaderCaches;
        public bool InvalidateShaderCachesInstruction => (InvalidateShaderCaches & 0x1) != 0;
        public bool InvalidateShaderCachesData => (InvalidateShaderCaches & 0x10) != 0;
        public bool InvalidateShaderCachesConstant => (InvalidateShaderCaches & 0x1000) != 0;
        public bool InvalidateShaderCachesLocks => (InvalidateShaderCaches & 0x2) != 0;
        public bool InvalidateShaderCachesFlushData => (InvalidateShaderCaches & 0x4) != 0;
        public uint SetReservedSwMethod00;
        public uint SetReservedSwMethod01;
        public uint SetReservedSwMethod02;
        public uint SetReservedSwMethod03;
        public uint SetReservedSwMethod04;
        public uint SetReservedSwMethod05;
        public uint SetReservedSwMethod06;
        public uint SetReservedSwMethod07;
        public uint SetCwdControl;
        public SetCwdControlSmSelection SetCwdControlSmSelection => (SetCwdControlSmSelection)(SetCwdControl & 0x1);
        public uint InvalidateTextureHeaderCacheNoWfi;
        public InvalidateCacheLines InvalidateTextureHeaderCacheNoWfiLines => (InvalidateCacheLines)(InvalidateTextureHeaderCacheNoWfi & 0x1);
        public int InvalidateTextureHeaderCacheNoWfiTag => (int)((InvalidateTextureHeaderCacheNoWfi >> 4) & 0x3FFFFF);
        public uint SetCwdRefCounter;
        public int SetCwdRefCounterSelect => (int)(SetCwdRefCounter & 0x3F);
        public int SetCwdRefCounterValue => (int)((SetCwdRefCounter >> 8) & 0xFFFF);
        public uint SetReservedSwMethod08;
        public uint SetReservedSwMethod09;
        public uint SetReservedSwMethod10;
        public uint SetReservedSwMethod11;
        public uint SetReservedSwMethod12;
        public uint SetReservedSwMethod13;
        public uint SetReservedSwMethod14;
        public uint SetReservedSwMethod15;
        public uint SetGwcScgType;
        public SetGwcScgTypeScgType SetGwcScgTypeScgType => (SetGwcScgTypeScgType)(SetGwcScgType & 0x1);
        public uint SetScgControl;
        public int SetScgControlCompute1MaxSmCount => (int)(SetScgControl & 0x1FF);
        public uint InvalidateConstantBufferCacheA;
        public int InvalidateConstantBufferCacheAAddressUpper => (int)(InvalidateConstantBufferCacheA & 0xFF);
        public uint InvalidateConstantBufferCacheB;
        public uint InvalidateConstantBufferCacheC;
        public int InvalidateConstantBufferCacheCByteCount => (int)(InvalidateConstantBufferCacheC & 0x1FFFF);
        public bool InvalidateConstantBufferCacheCThruL2 => (InvalidateConstantBufferCacheC & 0x80000000) != 0;
        public uint SetComputeClassVersion;
        public int SetComputeClassVersionCurrent => (int)(SetComputeClassVersion & 0xFFFF);
        public int SetComputeClassVersionOldestSupported => (int)((SetComputeClassVersion >> 16) & 0xFFFF);
        public uint CheckComputeClassVersion;
        public int CheckComputeClassVersionCurrent => (int)(CheckComputeClassVersion & 0xFFFF);
        public int CheckComputeClassVersionOldestSupported => (int)((CheckComputeClassVersion >> 16) & 0xFFFF);
        public uint SetQmdVersion;
        public int SetQmdVersionCurrent => (int)(SetQmdVersion & 0xFFFF);
        public int SetQmdVersionOldestSupported => (int)((SetQmdVersion >> 16) & 0xFFFF);
        public uint SetWfiConfig;
        public bool SetWfiConfigEnableScgTypeWfi => (SetWfiConfig & 0x1) != 0;
        public uint CheckQmdVersion;
        public int CheckQmdVersionCurrent => (int)(CheckQmdVersion & 0xFFFF);
        public int CheckQmdVersionOldestSupported => (int)((CheckQmdVersion >> 16) & 0xFFFF);
        public uint WaitForIdleScgType;
        public uint InvalidateSkedCaches;
        public bool InvalidateSkedCachesV => (InvalidateSkedCaches & 0x1) != 0;
        public uint SetScgRenderEnableControl;
        public bool SetScgRenderEnableControlCompute1UsesRenderEnable => (SetScgRenderEnableControl & 0x1) != 0;
        public fixed uint Reserved2A0[4];
        public uint SetCwdSlotCount;
        public int SetCwdSlotCountV => (int)(SetCwdSlotCount & 0xFF);
        public uint SendPcasA;
        public uint SendPcasB;
        public int SendPcasBFrom => (int)(SendPcasB & 0xFFFFFF);
        public int SendPcasBDelta => (int)((SendPcasB >> 24) & 0xFF);
        public uint SendSignalingPcasB;
        public bool SendSignalingPcasBInvalidate => (SendSignalingPcasB & 0x1) != 0;
        public bool SendSignalingPcasBSchedule => (SendSignalingPcasB & 0x2) != 0;
        public fixed uint Reserved2C0[9];
        public uint SetShaderLocalMemoryNonThrottledA;
        public int SetShaderLocalMemoryNonThrottledASizeUpper => (int)(SetShaderLocalMemoryNonThrottledA & 0xFF);
        public uint SetShaderLocalMemoryNonThrottledB;
        public uint SetShaderLocalMemoryNonThrottledC;
        public int SetShaderLocalMemoryNonThrottledCMaxSmCount => (int)(SetShaderLocalMemoryNonThrottledC & 0x1FF);
        public uint SetShaderLocalMemoryThrottledA;
        public int SetShaderLocalMemoryThrottledASizeUpper => (int)(SetShaderLocalMemoryThrottledA & 0xFF);
        public uint SetShaderLocalMemoryThrottledB;
        public uint SetShaderLocalMemoryThrottledC;
        public int SetShaderLocalMemoryThrottledCMaxSmCount => (int)(SetShaderLocalMemoryThrottledC & 0x1FF);
        public fixed uint Reserved2FC[5];
        public uint SetSpaVersion;
        public int SetSpaVersionMinor => (int)(SetSpaVersion & 0xFF);
        public int SetSpaVersionMajor => (int)((SetSpaVersion >> 8) & 0xFF);
        public fixed uint Reserved314[123];
        public uint SetFalcon00;
        public uint SetFalcon01;
        public uint SetFalcon02;
        public uint SetFalcon03;
        public uint SetFalcon04;
        public uint SetFalcon05;
        public uint SetFalcon06;
        public uint SetFalcon07;
        public uint SetFalcon08;
        public uint SetFalcon09;
        public uint SetFalcon10;
        public uint SetFalcon11;
        public uint SetFalcon12;
        public uint SetFalcon13;
        public uint SetFalcon14;
        public uint SetFalcon15;
        public uint SetFalcon16;
        public uint SetFalcon17;
        public uint SetFalcon18;
        public uint SetFalcon19;
        public uint SetFalcon20;
        public uint SetFalcon21;
        public uint SetFalcon22;
        public uint SetFalcon23;
        public uint SetFalcon24;
        public uint SetFalcon25;
        public uint SetFalcon26;
        public uint SetFalcon27;
        public uint SetFalcon28;
        public uint SetFalcon29;
        public uint SetFalcon30;
        public uint SetFalcon31;
        public fixed uint Reserved580[127];
        public uint SetShaderLocalMemoryWindow;
        public fixed uint Reserved780[4];
        public uint SetShaderLocalMemoryA;
        public int SetShaderLocalMemoryAAddressUpper => (int)(SetShaderLocalMemoryA & 0xFF);
        public uint SetShaderLocalMemoryB;
        public fixed uint Reserved798[383];
        public uint SetShaderCacheControl;
        public bool SetShaderCacheControlIcachePrefetchEnable => (SetShaderCacheControl & 0x1) != 0;
        public fixed uint ReservedD98[19];
        public uint SetSmTimeoutInterval;
        public int SetSmTimeoutIntervalCounterBit => (int)(SetSmTimeoutInterval & 0x3F);
        public fixed uint ReservedDE8[87];
        public uint SetSpareNoop12;
        public uint SetSpareNoop13;
        public uint SetSpareNoop14;
        public uint SetSpareNoop15;
        public fixed uint ReservedF54[59];
        public uint SetSpareNoop00;
        public uint SetSpareNoop01;
        public uint SetSpareNoop02;
        public uint SetSpareNoop03;
        public uint SetSpareNoop04;
        public uint SetSpareNoop05;
        public uint SetSpareNoop06;
        public uint SetSpareNoop07;
        public uint SetSpareNoop08;
        public uint SetSpareNoop09;
        public uint SetSpareNoop10;
        public uint SetSpareNoop11;
        public fixed uint Reserved1070[103];
        public uint InvalidateSamplerCacheAll;
        public bool InvalidateSamplerCacheAllV => (InvalidateSamplerCacheAll & 0x1) != 0;
        public uint InvalidateTextureHeaderCacheAll;
        public bool InvalidateTextureHeaderCacheAllV => (InvalidateTextureHeaderCacheAll & 0x1) != 0;
        public fixed uint Reserved1214[29];
        public uint InvalidateTextureDataCacheNoWfi;
        public InvalidateCacheLines InvalidateTextureDataCacheNoWfiLines => (InvalidateCacheLines)(InvalidateTextureDataCacheNoWfi & 0x1);
        public int InvalidateTextureDataCacheNoWfiTag => (int)((InvalidateTextureDataCacheNoWfi >> 4) & 0x3FFFFF);
        public fixed uint Reserved128C[7];
        public uint ActivatePerfSettingsForComputeContext;
        public bool ActivatePerfSettingsForComputeContextAll => (ActivatePerfSettingsForComputeContext & 0x1) != 0;
        public fixed uint Reserved12AC[33];
        public uint InvalidateSamplerCache;
        public InvalidateCacheLines InvalidateSamplerCacheLines => (InvalidateCacheLines)(InvalidateSamplerCache & 0x1);
        public int InvalidateSamplerCacheTag => (int)((InvalidateSamplerCache >> 4) & 0x3FFFFF);
        public uint InvalidateTextureHeaderCache;
        public InvalidateCacheLines InvalidateTextureHeaderCacheLines => (InvalidateCacheLines)(InvalidateTextureHeaderCache & 0x1);
        public int InvalidateTextureHeaderCacheTag => (int)((InvalidateTextureHeaderCache >> 4) & 0x3FFFFF);
        public uint InvalidateTextureDataCache;
        public InvalidateCacheLines InvalidateTextureDataCacheLines => (InvalidateCacheLines)(InvalidateTextureDataCache & 0x1);
        public int InvalidateTextureDataCacheTag => (int)((InvalidateTextureDataCache >> 4) & 0x3FFFFF);
        public fixed uint Reserved133C[58];
        public uint InvalidateSamplerCacheNoWfi;
        public InvalidateCacheLines InvalidateSamplerCacheNoWfiLines => (InvalidateCacheLines)(InvalidateSamplerCacheNoWfi & 0x1);
        public int InvalidateSamplerCacheNoWfiTag => (int)((InvalidateSamplerCacheNoWfi >> 4) & 0x3FFFFF);
        public fixed uint Reserved1428[64];
        public uint SetShaderExceptions;
        public bool SetShaderExceptionsEnable => (SetShaderExceptions & 0x1) != 0;
        public fixed uint Reserved152C[9];
        public uint SetRenderEnableA;
        public int SetRenderEnableAOffsetUpper => (int)(SetRenderEnableA & 0xFF);
        public uint SetRenderEnableB;
        public uint SetRenderEnableC;
        public int SetRenderEnableCMode => (int)(SetRenderEnableC & 0x7);
        public uint SetTexSamplerPoolA;
        public int SetTexSamplerPoolAOffsetUpper => (int)(SetTexSamplerPoolA & 0xFF);
        public uint SetTexSamplerPoolB;
        public uint SetTexSamplerPoolC;
        public int SetTexSamplerPoolCMaximumIndex => (int)(SetTexSamplerPoolC & 0xFFFFF);
        public fixed uint Reserved1568[3];
        public uint SetTexHeaderPoolA;
        public int SetTexHeaderPoolAOffsetUpper => (int)(SetTexHeaderPoolA & 0xFF);
        public uint SetTexHeaderPoolB;
        public uint SetTexHeaderPoolC;
        public int SetTexHeaderPoolCMaximumIndex => (int)(SetTexHeaderPoolC & 0x3FFFFF);
        public fixed uint Reserved1580[34];
        public uint SetProgramRegionA;
        public int SetProgramRegionAAddressUpper => (int)(SetProgramRegionA & 0xFF);
        public uint SetProgramRegionB;
        public fixed uint Reserved1610[34];
        public uint InvalidateShaderCachesNoWfi;
        public bool InvalidateShaderCachesNoWfiInstruction => (InvalidateShaderCachesNoWfi & 0x1) != 0;
        public bool InvalidateShaderCachesNoWfiGlobalData => (InvalidateShaderCachesNoWfi & 0x10) != 0;
        public bool InvalidateShaderCachesNoWfiConstant => (InvalidateShaderCachesNoWfi & 0x1000) != 0;
        public fixed uint Reserved169C[170];
        public uint SetRenderEnableOverride;
        public SetRenderEnableOverrideMode SetRenderEnableOverrideMode => (SetRenderEnableOverrideMode)(SetRenderEnableOverride & 0x3);
        public fixed uint Reserved1948[57];
        public uint PipeNop;
        public uint SetSpare00;
        public uint SetSpare01;
        public uint SetSpare02;
        public uint SetSpare03;
        public fixed uint Reserved1A40[48];
        public uint SetReportSemaphoreA;
        public int SetReportSemaphoreAOffsetUpper => (int)(SetReportSemaphoreA & 0xFF);
        public uint SetReportSemaphoreB;
        public uint SetReportSemaphoreC;
        public uint SetReportSemaphoreD;
        public SetReportSemaphoreDOperation SetReportSemaphoreDOperation => (SetReportSemaphoreDOperation)(SetReportSemaphoreD & 0x3);
        public bool SetReportSemaphoreDAwakenEnable => (SetReportSemaphoreD & 0x100000) != 0;
        public SetReportSemaphoreDStructureSize SetReportSemaphoreDStructureSize => (SetReportSemaphoreDStructureSize)((SetReportSemaphoreD >> 28) & 0x1);
        public bool SetReportSemaphoreDFlushDisable => (SetReportSemaphoreD & 0x4) != 0;
        public bool SetReportSemaphoreDReductionEnable => (SetReportSemaphoreD & 0x8) != 0;
        public SetReportSemaphoreDReductionOp SetReportSemaphoreDReductionOp => (SetReportSemaphoreDReductionOp)((SetReportSemaphoreD >> 9) & 0x7);
        public SetReportSemaphoreDReductionFormat SetReportSemaphoreDReductionFormat => (SetReportSemaphoreDReductionFormat)((SetReportSemaphoreD >> 17) & 0x3);
        public fixed uint Reserved1B10[702];
        public uint SetBindlessTexture;
        public int SetBindlessTextureConstantBufferSlotSelect => (int)(SetBindlessTexture & 0x7);
        public uint SetTrapHandler;
        public fixed uint Reserved2610[843];
        public Array8<uint> SetShaderPerformanceCounterValueUpper;
        public Array8<uint> SetShaderPerformanceCounterValue;
        public Array8<uint> SetShaderPerformanceCounterEvent;
        public int SetShaderPerformanceCounterEventEvent(int i) => (int)((SetShaderPerformanceCounterEvent[i] >> 0) & 0xFF);
        public Array8<uint> SetShaderPerformanceCounterControlA;
        public int SetShaderPerformanceCounterControlAEvent0(int i) => (int)((SetShaderPerformanceCounterControlA[i] >> 0) & 0x3);
        public int SetShaderPerformanceCounterControlABitSelect0(int i) => (int)((SetShaderPerformanceCounterControlA[i] >> 2) & 0x7);
        public int SetShaderPerformanceCounterControlAEvent1(int i) => (int)((SetShaderPerformanceCounterControlA[i] >> 5) & 0x3);
        public int SetShaderPerformanceCounterControlABitSelect1(int i) => (int)((SetShaderPerformanceCounterControlA[i] >> 7) & 0x7);
        public int SetShaderPerformanceCounterControlAEvent2(int i) => (int)((SetShaderPerformanceCounterControlA[i] >> 10) & 0x3);
        public int SetShaderPerformanceCounterControlABitSelect2(int i) => (int)((SetShaderPerformanceCounterControlA[i] >> 12) & 0x7);
        public int SetShaderPerformanceCounterControlAEvent3(int i) => (int)((SetShaderPerformanceCounterControlA[i] >> 15) & 0x3);
        public int SetShaderPerformanceCounterControlABitSelect3(int i) => (int)((SetShaderPerformanceCounterControlA[i] >> 17) & 0x7);
        public int SetShaderPerformanceCounterControlAEvent4(int i) => (int)((SetShaderPerformanceCounterControlA[i] >> 20) & 0x3);
        public int SetShaderPerformanceCounterControlABitSelect4(int i) => (int)((SetShaderPerformanceCounterControlA[i] >> 22) & 0x7);
        public int SetShaderPerformanceCounterControlAEvent5(int i) => (int)((SetShaderPerformanceCounterControlA[i] >> 25) & 0x3);
        public int SetShaderPerformanceCounterControlABitSelect5(int i) => (int)((SetShaderPerformanceCounterControlA[i] >> 27) & 0x7);
        public int SetShaderPerformanceCounterControlASpare(int i) => (int)((SetShaderPerformanceCounterControlA[i] >> 30) & 0x3);
        public Array8<uint> SetShaderPerformanceCounterControlB;
        public bool SetShaderPerformanceCounterControlBEdge(int i) => (SetShaderPerformanceCounterControlB[i] & 0x1) != 0;
        public int SetShaderPerformanceCounterControlBMode(int i) => (int)((SetShaderPerformanceCounterControlB[i] >> 1) & 0x3);
        public bool SetShaderPerformanceCounterControlBWindowed(int i) => (SetShaderPerformanceCounterControlB[i] & 0x8) != 0;
        public int SetShaderPerformanceCounterControlBFunc(int i) => (int)((SetShaderPerformanceCounterControlB[i] >> 4) & 0xFFFF);
        public uint SetShaderPerformanceCounterTrapControl;
        public int SetShaderPerformanceCounterTrapControlMask => (int)(SetShaderPerformanceCounterTrapControl & 0xFF);
        public uint StartShaderPerformanceCounter;
        public int StartShaderPerformanceCounterCounterMask => (int)(StartShaderPerformanceCounter & 0xFF);
        public uint StopShaderPerformanceCounter;
        public int StopShaderPerformanceCounterCounterMask => (int)(StopShaderPerformanceCounter & 0xFF);
        public fixed uint Reserved33E8[6];
        public MmeShadowScratch SetMmeShadowScratch;
#pragma warning restore CS0649
    }
}
