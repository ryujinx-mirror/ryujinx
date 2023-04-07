namespace Ryujinx.HLE.HOS.Kernel.SupervisorCall
{
    enum CodeMemoryOperation : uint
    {
        Map,
        MapToOwner,
        Unmap,
        UnmapFromOwner
    };
}