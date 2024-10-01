namespace Ryujinx.HLE.HOS.Kernel.Memory
{
    class KMemoryBlockSlabManager
    {
        private readonly ulong _capacityElements;

        public int Count { get; set; }

        public KMemoryBlockSlabManager(ulong capacityElements)
        {
            _capacityElements = capacityElements;
        }

        public bool CanAllocate(int count)
        {
            return (ulong)(Count + count) <= _capacityElements;
        }
    }
}
