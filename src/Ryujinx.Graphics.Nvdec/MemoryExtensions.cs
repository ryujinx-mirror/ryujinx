using Ryujinx.Graphics.Gpu.Memory;
using System;

namespace Ryujinx.Graphics.Nvdec
{
    static class MemoryExtensions
    {
        public static T DeviceRead<T>(this MemoryManager gmm, uint offset) where T : unmanaged
        {
            return gmm.Read<T>((ulong)offset << 8);
        }

        public static ReadOnlySpan<byte> DeviceGetSpan(this MemoryManager gmm, uint offset, int size)
        {
            return gmm.GetSpan((ulong)offset << 8, size);
        }

        public static void DeviceWrite(this MemoryManager gmm, uint offset, ReadOnlySpan<byte> data)
        {
            gmm.Write((ulong)offset << 8, data);
        }

        public static ulong ExtendOffset(uint offset)
        {
            return (ulong)offset << 8;
        }
    }
}
