using Ryujinx.Memory;
using System;

namespace Ryujinx.Cpu.AppleHv
{
    struct HvMemoryBlockAllocation : IDisposable
    {
        private readonly HvMemoryBlockAllocator _owner;
        private readonly HvMemoryBlockAllocator.Block _block;

        public bool IsValid => _owner != null;
        public MemoryBlock Memory => _block.Memory;
        public ulong Ipa => _block.Ipa;
        public ulong Offset { get; }
        public ulong Size { get; }

        public HvMemoryBlockAllocation(
            HvMemoryBlockAllocator owner,
            HvMemoryBlockAllocator.Block block,
            ulong offset,
            ulong size)
        {
            _owner = owner;
            _block = block;
            Offset = offset;
            Size = size;
        }

        public void Dispose()
        {
            _owner.Free(_block, Offset, Size);
        }
    }
}
