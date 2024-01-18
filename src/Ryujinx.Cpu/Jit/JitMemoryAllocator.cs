using ARMeilleure.Memory;
using Ryujinx.Memory;

namespace Ryujinx.Cpu.Jit
{
    public class JitMemoryAllocator : IJitMemoryAllocator
    {
        public IJitMemoryBlock Allocate(ulong size) => new JitMemoryBlock(size, MemoryAllocationFlags.None);
        public IJitMemoryBlock Reserve(ulong size) => new JitMemoryBlock(size, MemoryAllocationFlags.Reserve | MemoryAllocationFlags.Jit);
    }
}
