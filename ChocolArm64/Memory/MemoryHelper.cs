using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace ChocolArm64.Memory
{
    public static class MemoryHelper
    {
        public static void FillWithZeros(MemoryManager memory, long position, int size)
        {
            int size8 = size & ~(8 - 1);

            for (int offs = 0; offs < size8; offs += 8)
            {
                memory.WriteInt64(position + offs, 0);
            }

            for (int offs = size8; offs < (size - size8); offs++)
            {
                memory.WriteByte(position + offs, 0);
            }
        }

        public unsafe static T Read<T>(MemoryManager memory, long position) where T : struct
        {
            long size = Marshal.SizeOf<T>();

            memory.EnsureRangeIsValid(position, size);

            IntPtr ptr = (IntPtr)memory.Translate(position);

            return Marshal.PtrToStructure<T>(ptr);
        }

        public unsafe static void Write<T>(MemoryManager memory, long position, T value) where T : struct
        {
            long size = Marshal.SizeOf<T>();

            memory.EnsureRangeIsValid(position, size);

            IntPtr ptr = (IntPtr)memory.TranslateWrite(position);

            Marshal.StructureToPtr<T>(value, ptr, false);
        }

        public static string ReadAsciiString(MemoryManager memory, long position, long maxSize = -1)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                for (long offs = 0; offs < maxSize || maxSize == -1; offs++)
                {
                    byte value = (byte)memory.ReadByte(position + offs);

                    if (value == 0)
                    {
                        break;
                    }

                    ms.WriteByte(value);
                }

                return Encoding.ASCII.GetString(ms.ToArray());
            }
        }
    }
}