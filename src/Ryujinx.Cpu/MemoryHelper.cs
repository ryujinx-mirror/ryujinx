using Microsoft.IO;
using Ryujinx.Common.Memory;
using Ryujinx.Memory;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Ryujinx.Cpu
{
    public static class MemoryHelper
    {
        public static void FillWithZeros(IVirtualMemoryManager memory, ulong position, int size)
        {
            int size8 = size & ~(8 - 1);

            for (int offs = 0; offs < size8; offs += 8)
            {
                memory.Write<long>(position + (ulong)offs, 0);
            }

            for (int offs = size8; offs < (size - size8); offs++)
            {
                memory.Write<byte>(position + (ulong)offs, 0);
            }
        }

        public static T Read<T>(IVirtualMemoryManager memory, ulong position) where T : unmanaged
        {
            return MemoryMarshal.Cast<byte, T>(memory.GetSpan(position, Unsafe.SizeOf<T>()))[0];
        }

        public static ulong Write<T>(IVirtualMemoryManager memory, ulong position, T value) where T : unmanaged
        {
            ReadOnlySpan<byte> data = MemoryMarshal.Cast<T, byte>(MemoryMarshal.CreateReadOnlySpan(ref value, 1));

            memory.Write(position, data);

            return (ulong)data.Length;
        }

        public static string ReadAsciiString(IVirtualMemoryManager memory, ulong position, long maxSize = -1)
        {
            using RecyclableMemoryStream ms = MemoryStreamManager.Shared.GetStream();

            for (long offs = 0; offs < maxSize || maxSize == -1; offs++)
            {
                byte value = memory.Read<byte>(position + (ulong)offs);

                if (value == 0)
                {
                    break;
                }

                ms.WriteByte(value);
            }

            return Encoding.ASCII.GetString(ms.GetReadOnlySequence());
        }
    }
}
