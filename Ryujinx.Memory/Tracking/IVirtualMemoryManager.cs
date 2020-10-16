namespace Ryujinx.Memory.Tracking
{
    public interface IVirtualMemoryManager
    {
        (ulong address, ulong size)[] GetPhysicalRegions(ulong va, ulong size);

        void TrackingReprotect(ulong va, ulong size, MemoryPermission protection);
    }
}
