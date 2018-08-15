using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace ChocolArm64.Memory
{
    public static class AMemoryHelper
    {
        public static void FillWithZeros(AMemory Memory, long Position, int Size)
        {
            int Size8 = Size & ~(8 - 1);

            for (int Offs = 0; Offs < Size8; Offs += 8)
            {
                Memory.WriteInt64(Position + Offs, 0);
            }

            for (int Offs = Size8; Offs < (Size - Size8); Offs++)
            {
                Memory.WriteByte(Position + Offs, 0);
            }
        }

        public unsafe static T Read<T>(AMemory Memory, long Position) where T : struct
        {
            long Size = Marshal.SizeOf<T>();

            Memory.EnsureRangeIsValid(Position, Size);

            IntPtr Ptr = (IntPtr)Memory.Translate(Position);

            return Marshal.PtrToStructure<T>(Ptr);
        }

        public unsafe static void Write<T>(AMemory Memory, long Position, T Value) where T : struct
        {
            long Size = Marshal.SizeOf<T>();

            Memory.EnsureRangeIsValid(Position, Size);

            IntPtr Ptr = (IntPtr)Memory.TranslateWrite(Position);

            Marshal.StructureToPtr<T>(Value, Ptr, false);
        }

        public static string ReadAsciiString(AMemory Memory, long Position, long MaxSize = -1)
        {
            using (MemoryStream MS = new MemoryStream())
            {
                for (long Offs = 0; Offs < MaxSize || MaxSize == -1; Offs++)
                {
                    byte Value = (byte)Memory.ReadByte(Position + Offs);

                    if (Value == 0)
                    {
                        break;
                    }

                    MS.WriteByte(Value);
                }

                return Encoding.ASCII.GetString(MS.ToArray());
            }
        }
    }
}