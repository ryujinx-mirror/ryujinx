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
        // NOTE: Added in 4.0.0, removed in 5.0.0.
        InitialProcessIdRange,
        UserExceptionContextAddress,
        TotalNonSystemMemorySize,
        UsedNonSystemMemorySize,
        IsApplication,
        FreeThreadCount,
        ThreadTickCount,
        MesosphereCurrentProcess = 65001
    }
}
