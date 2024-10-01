using Ryujinx.Memory;
using System;

namespace Ryujinx.Cpu
{
    public class AddressSpace : IDisposable
    {
        private readonly MemoryBlock _backingMemory;

        public MemoryBlock Base { get; }
        public MemoryBlock Mirror { get; }

        public ulong AddressSpaceSize { get; }

        public AddressSpace(MemoryBlock backingMemory, MemoryBlock baseMemory, MemoryBlock mirrorMemory, ulong addressSpaceSize)
        {
            _backingMemory = backingMemory;

            Base = baseMemory;
            Mirror = mirrorMemory;
            AddressSpaceSize = addressSpaceSize;
        }

        public static bool TryCreate(MemoryBlock backingMemory, ulong asSize, out AddressSpace addressSpace)
        {
            addressSpace = null;

            const MemoryAllocationFlags AsFlags = MemoryAllocationFlags.Reserve | MemoryAllocationFlags.ViewCompatible;

            ulong minAddressSpaceSize = Math.Min(asSize, 1UL << 36);

            // Attempt to create the address space with expected size or try to reduce it until it succeed.
            for (ulong addressSpaceSize = asSize; addressSpaceSize >= minAddressSpaceSize; addressSpaceSize >>= 1)
            {
                MemoryBlock baseMemory = null;
                MemoryBlock mirrorMemory = null;

                try
                {
                    baseMemory = new MemoryBlock(addressSpaceSize, AsFlags);
                    mirrorMemory = new MemoryBlock(addressSpaceSize, AsFlags);
                    addressSpace = new AddressSpace(backingMemory, baseMemory, mirrorMemory, addressSpaceSize);

                    break;
                }
                catch (SystemException)
                {
                    baseMemory?.Dispose();
                    mirrorMemory?.Dispose();
                }
            }

            return addressSpace != null;
        }

        public void Map(ulong va, ulong pa, ulong size, MemoryMapFlags flags)
        {
            Base.MapView(_backingMemory, pa, va, size);
            Mirror.MapView(_backingMemory, pa, va, size);
        }

        public void Unmap(ulong va, ulong size)
        {
            Base.UnmapView(_backingMemory, va, size);
            Mirror.UnmapView(_backingMemory, va, size);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            Base.Dispose();
            Mirror.Dispose();
        }
    }
}
