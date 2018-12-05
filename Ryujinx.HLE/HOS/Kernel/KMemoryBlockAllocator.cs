namespace Ryujinx.HLE.HOS.Kernel
{
    class KMemoryBlockAllocator
    {
        private ulong CapacityElements;

        public int Count { get; set; }

        public KMemoryBlockAllocator(ulong CapacityElements)
        {
            this.CapacityElements = CapacityElements;
        }

        public bool CanAllocate(int Count)
        {
            return (ulong)(this.Count + Count) <= CapacityElements;
        }
    }
}