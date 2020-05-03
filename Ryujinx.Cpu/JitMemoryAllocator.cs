using ARMeilleure.Memory;
using Ryujinx.Memory;

namespace Ryujinx.Cpu
{
    class JitMemoryAllocator : IJitMemoryAllocator
    {
        public IJitMemoryBlock Allocate(ulong size) => new JitMemoryBlock(size, MemoryAllocationFlags.None);
        public IJitMemoryBlock Reserve(ulong size) => new JitMemoryBlock(size, MemoryAllocationFlags.Reserve);
    }
}
