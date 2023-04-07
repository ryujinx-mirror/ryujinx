using System;

namespace ARMeilleure.Memory
{
    public interface IJitMemoryBlock : IDisposable
    {
        IntPtr Pointer { get; }

        bool Commit(ulong offset, ulong size);

        void MapAsRx(ulong offset, ulong size);
        void MapAsRwx(ulong offset, ulong size);
    }
}
