using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ryujinx.Common
{
    public static class BinaryReaderExtensions
    {
        public unsafe static T ReadStruct<T>(this BinaryReader reader)
            where T : unmanaged
        {
            return MemoryMarshal.Cast<byte, T>(reader.ReadBytes(Unsafe.SizeOf<T>()))[0];
        }

        public unsafe static void WriteStruct<T>(this BinaryWriter writer, T value)
            where T : unmanaged
        {
            ReadOnlySpan<byte> data = MemoryMarshal.Cast<T, byte>(MemoryMarshal.CreateReadOnlySpan(ref value, 1));

            writer.Write(data);
        }

        public static void Write(this BinaryWriter writer, UInt128 value)
        {
            writer.Write((ulong)value);
            writer.Write((ulong)(value >> 64));
        }
    }
}
