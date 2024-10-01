using ARMeilleure.Memory;
using Ryujinx.Memory;

namespace Ryujinx.Cpu.Jit
{
    public class JitMemoryAllocator : IJitMemoryAllocator
    {
        private readonly MemoryAllocationFlags _jitFlag;

        public JitMemoryAllocator(bool forJit = false)
        {
            _jitFlag = forJit ? MemoryAllocationFlags.Jit : MemoryAllocationFlags.None;
        }

        public IJitMemoryBlock Allocate(ulong size) => new JitMemoryBlock(size, MemoryAllocationFlags.None);
        public IJitMemoryBlock Reserve(ulong size) => new JitMemoryBlock(size, MemoryAllocationFlags.Reserve | _jitFlag);
    }
}
