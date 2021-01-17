using System;

namespace Ryujinx.Memory
{
    public sealed class WritableRegion : IDisposable
    {
        private readonly IWritableBlock _block;
        private readonly ulong _va;

        private bool NeedsWriteback => _block != null;

        public Memory<byte> Memory { get; }

        public WritableRegion(IWritableBlock block, ulong va, Memory<byte> memory)
        {
            _block = block;
            _va = va;
            Memory = memory;
        }

        public void Dispose()
        {
            if (NeedsWriteback)
            {
                _block.Write(_va, Memory.Span);
            }
        }
    }
}
