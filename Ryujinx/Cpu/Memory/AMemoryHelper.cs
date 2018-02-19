using System.IO;
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

        public static byte[] ReadBytes(AMemory Memory, long Position, int Size)
        {
            byte[] Data = new byte[Size];

            for (int Offs = 0; Offs < Size; Offs++)
            {
                Data[Offs] = (byte)Memory.ReadByte(Position + Offs);
            }

            return Data;
        }

        public static void WriteBytes(AMemory Memory, long Position, byte[] Data)
        {
            for (int Offs = 0; Offs < Data.Length; Offs++)
            {
                Memory.WriteByte(Position + Offs, Data[Offs]);
            }
        }

        public static string ReadAsciiString(AMemory Memory, long Position, int MaxSize = -1)
        {
            using (MemoryStream MS = new MemoryStream())
            {
                for (int Offs = 0; Offs < MaxSize || MaxSize == -1; Offs++)
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

        public static long PageRoundUp(long Value)
        {
            return (Value + AMemoryMgr.PageMask) & ~AMemoryMgr.PageMask;
        }

        public static long PageRoundDown(long Value)
        {
            return Value & ~AMemoryMgr.PageMask;
        }
    }
}