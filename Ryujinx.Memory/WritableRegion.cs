using System;

namespace Ryujinx.Memory
{
    public sealed class WritableRegion : IDisposable
    {
        private readonly IVirtualMemoryManager _mm;
        private readonly ulong _va;

        private bool NeedsWriteback => _mm != null;

        public Memory<byte> Memory { get; }

        public WritableRegion(IVirtualMemoryManager mm, ulong va, Memory<byte> memory)
        {
            _mm = mm;
            _va = va;
            Memory = memory;
        }

        public void Dispose()
        {
            if (NeedsWriteback)
            {
                _mm.Write(_va, Memory.Span);
            }
        }
    }
}
