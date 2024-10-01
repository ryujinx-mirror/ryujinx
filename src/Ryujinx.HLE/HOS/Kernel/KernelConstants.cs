using Ryujinx.HLE.HOS.Kernel.Memory;

namespace Ryujinx.HLE.HOS.Kernel
{
    static class KernelConstants
    {
        public const int InitialKipId = 1;
        public const int InitialProcessId = 0x51;

        public const int SupervisorCallCount = 0xC0;

        public const int MemoryBlockAllocatorSize = 0x2710;

        public const ulong UserSlabHeapBase = DramMemoryMap.SlabHeapBase;
        public const ulong UserSlabHeapItemSize = KPageTableBase.PageSize;
        public const ulong UserSlabHeapSize = 0x3de000;

        public const ulong CounterFrequency = 19200000;
    }
}
