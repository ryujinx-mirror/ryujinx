using System;

namespace Ryujinx.Cpu
{
    public sealed class WritableRegion : IDisposable
    {
        private readonly MemoryManager _mm;
        private readonly ulong _va;

        private bool NeedsWriteback => _mm != null;

        public Memory<byte> Memory { get; }

        internal WritableRegion(MemoryManager mm, ulong va, Memory<byte> memory)
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
