using System;

namespace Ryujinx.Memory
{
    public interface IWritableBlock
    {
        void Write(ulong va, ReadOnlySpan<byte> data);

        void WriteUntracked(ulong va, ReadOnlySpan<byte> data) => Write(va, data);
    }
}
