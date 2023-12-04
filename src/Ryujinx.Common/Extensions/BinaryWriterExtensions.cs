using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Ryujinx.Common
{
    public static class BinaryWriterExtensions
    {
        public static void WriteStruct<T>(this BinaryWriter writer, T value) where T : unmanaged
        {
            ReadOnlySpan<byte> data = MemoryMarshal.Cast<T, byte>(MemoryMarshal.CreateReadOnlySpan(ref value, 1));

            writer.Write(data);
        }

        public static void Write(this BinaryWriter writer, UInt128 value)
        {
            writer.Write((ulong)value);
            writer.Write((ulong)(value >> 64));
        }

        public static void Write(this BinaryWriter writer, MemoryStream stream)
        {
            stream.CopyTo(writer.BaseStream);
        }
    }
}
