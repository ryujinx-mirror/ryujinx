namespace Ryujinx.HLE.HOS.Kernel
{
    static class DramMemoryMap
    {
        public const ulong DramBase = 0x80000000;
        public const ulong DramSize = 0x100000000;
        public const ulong DramEnd  = DramBase + DramSize;

        public const ulong KernelReserveBase = DramBase + 0x60000;

        public const ulong SlabHeapBase = KernelReserveBase + 0x85000;
        public const ulong SlapHeapSize = 0xa21000;
        public const ulong SlabHeapEnd  = SlabHeapBase + SlapHeapSize;
    }
}