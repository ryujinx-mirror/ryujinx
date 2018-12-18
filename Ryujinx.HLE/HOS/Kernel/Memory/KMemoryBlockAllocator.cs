namespace Ryujinx.HLE.HOS.Kernel.Memory
{
    class KMemoryBlockAllocator
    {
        private ulong _capacityElements;

        public int Count { get; set; }

        public KMemoryBlockAllocator(ulong capacityElements)
        {
            _capacityElements = capacityElements;
        }

        public bool CanAllocate(int count)
        {
            return (ulong)(Count + count) <= _capacityElements;
        }
    }
}