using System;

namespace Ryujinx.Graphics.Gpu.Memory
{
    public interface IPhysicalMemory
    {
        int GetPageSize();

        Span<byte> Read(ulong address, ulong size);

        void Write(ulong address, Span<byte> data);

        (ulong, ulong)[] GetModifiedRanges(ulong address, ulong size, ResourceName name);
    }
}