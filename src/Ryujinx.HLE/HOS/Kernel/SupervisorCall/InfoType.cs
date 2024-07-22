namespace Ryujinx.HLE.HOS.Kernel.SupervisorCall
{
    enum InfoType : uint
    {
        CoreMask,
        PriorityMask,
        AliasRegionAddress,
        AliasRegionSize,
        HeapRegionAddress,
        HeapRegionSize,
        TotalMemorySize,
        UsedMemorySize,
        DebuggerAttached,
        ResourceLimit,
        IdleTickCount,
        RandomEntropy,
        AslrRegionAddress,
        AslrRegionSize,
        StackRegionAddress,
        StackRegionSize,
        SystemResourceSizeTotal,
        SystemResourceSizeUsed,
        ProgramId,
        InitialProcessIdRange, // NOTE: Added in 4.0.0, removed in 5.0.0.
        UserExceptionContextAddress,
        TotalNonSystemMemorySize,
        UsedNonSystemMemorySize,
        IsApplication,
        FreeThreadCount,
        ThreadTickCount,
        IsSvcPermitted,
        IoRegionHint,
        AliasRegionExtraSize,

        MesosphereCurrentProcess = 65001,
    }
}
