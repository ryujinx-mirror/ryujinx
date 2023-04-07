using Ryujinx.Memory;
using System.Collections.Generic;

namespace Ryujinx.Cpu.AppleHv
{
    class HvMemoryBlockAllocator : PrivateMemoryAllocatorImpl<HvMemoryBlockAllocator.Block>
    {
        private const ulong InvalidOffset = ulong.MaxValue;

        public class Block : PrivateMemoryAllocator.Block
        {
            private readonly HvIpaAllocator _ipaAllocator;
            public ulong Ipa { get; }

            public Block(HvIpaAllocator ipaAllocator, MemoryBlock memory, ulong size) : base(memory, size)
            {
                _ipaAllocator = ipaAllocator;

                lock (ipaAllocator)
                {
                    Ipa = ipaAllocator.Allocate(size);
                }

                HvApi.hv_vm_map((ulong)Memory.Pointer, Ipa, size, hv_memory_flags_t.HV_MEMORY_READ | hv_memory_flags_t.HV_MEMORY_WRITE).ThrowOnError();
            }

            public override void Destroy()
            {
                HvApi.hv_vm_unmap(Ipa, Size).ThrowOnError();

                lock (_ipaAllocator)
                {
                    _ipaAllocator.Free(Ipa, Size);
                }

                base.Destroy();
            }
        }

        private readonly HvIpaAllocator _ipaAllocator;

        public HvMemoryBlockAllocator(HvIpaAllocator ipaAllocator, int blockAlignment) : base(blockAlignment, MemoryAllocationFlags.None)
        {
            _ipaAllocator = ipaAllocator;
        }

        public unsafe HvMemoryBlockAllocation Allocate(ulong size, ulong alignment)
        {
            var allocation = Allocate(size, alignment, CreateBlock);

            return new HvMemoryBlockAllocation(this, allocation.Block, allocation.Offset, allocation.Size);
        }

        private Block CreateBlock(MemoryBlock memory, ulong size)
        {
            return new Block(_ipaAllocator, memory, size);
        }
    }
}
