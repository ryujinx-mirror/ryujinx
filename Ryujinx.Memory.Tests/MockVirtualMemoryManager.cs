using Ryujinx.Memory.Range;
using System;
using System.Collections.Generic;

namespace Ryujinx.Memory.Tests
{
    class MockVirtualMemoryManager : IVirtualMemoryManager
    {
        public bool NoMappings = false;

        public event Action<ulong, ulong, MemoryPermission> OnProtect;

        public MockVirtualMemoryManager(ulong size, int pageSize)
        {
        }

        public void Map(ulong va, nuint hostAddress, ulong size)
        {
            throw new NotImplementedException();
        }

        public void Unmap(ulong va, ulong size)
        {
            throw new NotImplementedException();
        }

        public T Read<T>(ulong va) where T : unmanaged
        {
            throw new NotImplementedException();
        }

        public void Read(ulong va, Span<byte> data)
        {
            throw new NotImplementedException();
        }

        public void Write<T>(ulong va, T value) where T : unmanaged
        {
            throw new NotImplementedException();
        }

        public void Write(ulong va, ReadOnlySpan<byte> data)
        {
            throw new NotImplementedException();
        }

        public ReadOnlySpan<byte> GetSpan(ulong va, int size, bool tracked = false)
        {
            throw new NotImplementedException();
        }

        public WritableRegion GetWritableRegion(ulong va, int size, bool tracked = false)
        {
            throw new NotImplementedException();
        }

        public ref T GetRef<T>(ulong va) where T : unmanaged
        {
            throw new NotImplementedException();
        }

        IEnumerable<HostMemoryRange> IVirtualMemoryManager.GetPhysicalRegions(ulong va, ulong size)
        {
            return NoMappings ? new HostMemoryRange[0] : new HostMemoryRange[] { new HostMemoryRange((nuint)va, size) };
        }

        public bool IsMapped(ulong va)
        {
            return true;
        }

        public bool IsRangeMapped(ulong va, ulong size)
        {
            return true;
        }

        public ulong GetPhysicalAddress(ulong va)
        {
            throw new NotImplementedException();
        }

        public void SignalMemoryTracking(ulong va, ulong size, bool write, bool precise = false)
        {
            throw new NotImplementedException();
        }

        public void TrackingReprotect(ulong va, ulong size, MemoryPermission protection)
        {
            OnProtect?.Invoke(va, size, protection);
        }
    }
}
