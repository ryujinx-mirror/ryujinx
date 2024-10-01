using Ryujinx.Memory;
using System;

namespace Ryujinx.Cpu
{
    readonly struct PrivateMemoryAllocation : IDisposable
    {
        private readonly PrivateMemoryAllocator _owner;
        private readonly PrivateMemoryAllocator.Block _block;

        public bool IsValid => _owner != null;
        public MemoryBlock Memory => _block?.Memory;
        public ulong Offset { get; }
        public ulong Size { get; }

        public PrivateMemoryAllocation(
            PrivateMemoryAllocator owner,
            PrivateMemoryAllocator.Block block,
            ulong offset,
            ulong size)
        {
            _owner = owner;
            _block = block;
            Offset = offset;
            Size = size;
        }

        public (PrivateMemoryAllocation, PrivateMemoryAllocation) Split(ulong splitOffset)
        {
            PrivateMemoryAllocation left = new(_owner, _block, Offset, splitOffset);
            PrivateMemoryAllocation right = new(_owner, _block, Offset + splitOffset, Size - splitOffset);

            return (left, right);
        }

        public void Dispose()
        {
            _owner.Free(_block, Offset, Size);
        }
    }
}
