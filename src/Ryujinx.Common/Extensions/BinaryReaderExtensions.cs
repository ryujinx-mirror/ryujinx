using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ryujinx.Common
{
    public static class BinaryReaderExtensions
    {
        public static T ReadStruct<T>(this BinaryReader reader) where T : unmanaged
        {
            return MemoryMarshal.Cast<byte, T>(reader.ReadBytes(Unsafe.SizeOf<T>()))[0];
        }
    }
}
