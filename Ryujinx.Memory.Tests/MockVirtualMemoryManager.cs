using Ryujinx.Memory.Tracking;

namespace Ryujinx.Memory.Tests
{
    class MockVirtualMemoryManager : IVirtualMemoryManager
    {
        public bool NoMappings;

        public MockVirtualMemoryManager(ulong size, int pageSize)
        {
        }

        public (ulong address, ulong size)[] GetPhysicalRegions(ulong va, ulong size)
        {
            return NoMappings ? new (ulong address, ulong size)[0] : new (ulong address, ulong size)[] { (va, size) };
        }

        public void TrackingReprotect(ulong va, ulong size, MemoryPermission protection)
        {
            
        }
    }
}
