using Ryujinx.Graphics.Device;
using System;

namespace Ryujinx.Graphics.Nvdec
{
    static class MemoryExtensions
    {
        public static T DeviceRead<T>(this DeviceMemoryManager gmm, uint offset) where T : unmanaged
        {
            return gmm.Read<T>(ExtendOffset(offset));
        }

        public static ReadOnlySpan<byte> DeviceGetSpan(this DeviceMemoryManager gmm, uint offset, int size)
        {
            return gmm.GetSpan(ExtendOffset(offset), size);
        }

        public static void DeviceWrite(this DeviceMemoryManager gmm, uint offset, ReadOnlySpan<byte> data)
        {
            gmm.Write(ExtendOffset(offset), data);
        }

        public static ulong ExtendOffset(uint offset)
        {
            return (ulong)offset << 8;
        }
    }
}
