namespace Ryujinx.HLE.HOS.Kernel.Process
{
    enum CapabilityType : uint
    {
        CorePriority = (1u << 3) - 1,
        SyscallMask = (1u << 4) - 1,
        MapRange = (1u << 6) - 1,
        MapIoPage = (1u << 7) - 1,
        MapRegion = (1u << 10) - 1,
        InterruptPair = (1u << 11) - 1,
        ProgramType = (1u << 13) - 1,
        KernelVersion = (1u << 14) - 1,
        HandleTable = (1u << 15) - 1,
        DebugFlags = (1u << 16) - 1,

        Invalid = 0u,
        Padding = ~0u,
    }
}
