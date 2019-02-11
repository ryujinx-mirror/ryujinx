using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Ryujinx.Common
{
    public static class BinaryReaderExtensions
    {
        public unsafe static T ReadStruct<T>(this BinaryReader reader)
            where T : struct
        {
            int size = Marshal.SizeOf<T>();

            byte[] data = reader.ReadBytes(size);

            fixed (byte* ptr = data)
            {
                return Marshal.PtrToStructure<T>((IntPtr)ptr);
            }
        }

        public unsafe static void WriteStruct<T>(this BinaryWriter writer, T value)
            where T : struct
        {
            long size = Marshal.SizeOf<T>();

            byte[] data = new byte[size];

            fixed (byte* ptr = data)
            {
                Marshal.StructureToPtr<T>(value, (IntPtr)ptr, false);
            }

            writer.Write(data);
        }
    }
}
