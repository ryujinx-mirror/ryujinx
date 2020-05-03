using System;

namespace ARMeilleure.Memory
{
    public interface IMemoryManager
    {
        int AddressSpaceBits { get; }

        IntPtr PageTablePointer { get; }

        T Read<T>(ulong va) where T : unmanaged;
        void Write<T>(ulong va, T value) where T : unmanaged;

        ref T GetRef<T>(ulong va) where T : unmanaged;
        ref T GetRefNoChecks<T>(ulong va) where T : unmanaged;

        bool IsMapped(ulong va);
    }
}