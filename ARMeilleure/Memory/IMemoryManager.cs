using ARMeilleure.State;
using System;

namespace ARMeilleure.Memory
{
    public interface IMemoryManager : IMemory, IDisposable
    {
        void Map(long va, long pa, long size);

        void Unmap(long position, long size);

        bool IsMapped(long position);

        long GetPhysicalAddress(long virtualAddress);

        bool IsRegionModified(long position, long size);

        bool TryGetHostAddress(long position, long size, out IntPtr ptr);

        bool IsValidPosition(long position);

        bool AtomicCompareExchangeInt32(long position, int expected, int desired);

        int AtomicIncrementInt32(long position);

        int AtomicDecrementInt32(long position);

        byte[] ReadBytes(long position, long size);

        void ReadBytes(long position, byte[] data, int startIndex, int size);

        void WriteVector128(long position, V128 value);

        void WriteBytes(long position, byte[] data);

        void WriteBytes(long position, byte[] data, int startIndex, int size);

        void CopyBytes(long src, long dst, long size);
    }
}