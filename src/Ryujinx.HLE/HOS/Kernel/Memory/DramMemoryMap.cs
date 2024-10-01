namespace Ryujinx.HLE.HOS.Kernel.Memory
{
    static class DramMemoryMap
    {
        public const ulong DramBase = 0x80000000;

        public const ulong KernelReserveBase = DramBase + 0x60000;

        public const ulong SlabHeapBase = KernelReserveBase + 0x85000;
        public const ulong SlapHeapSize = 0xa21000;
        public const ulong SlabHeapEnd = SlabHeapBase + SlapHeapSize;

        public static bool IsHeapPhysicalAddress(ulong address)
        {
            return address >= SlabHeapEnd;
        }
    }
}
