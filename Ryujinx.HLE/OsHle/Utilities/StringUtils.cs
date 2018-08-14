using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Ryujinx.HLE.OsHle.Utilities
{
    static class StringUtils
    {
        public static byte[] GetFixedLengthBytes(string InputString, int Size, Encoding Encoding)
        {
            InputString = InputString + "\0";

            int ByteCount = Encoding.GetByteCount(InputString);

            byte[] Output = new byte[Size];

            if (ByteCount < Size)
            {
                Encoding.GetBytes(InputString, 0, InputString.Length, Output, Size - ByteCount);
            }
            else
            {
                int NullSize = Encoding.GetByteCount("\0");

                Output = Encoding.GetBytes(InputString);

                Array.Resize(ref Output, Size - NullSize);

                Output = Output.Concat(Encoding.GetBytes("\0")).ToArray();
            }

            return Output;
        }

        public static byte[] HexToBytes(string HexString)
        {
            //Ignore last charactor if HexLength % 2 != 0
            int BytesInHex = HexString.Length / 2;

            byte[] Output = new byte[BytesInHex];

            for (int Index = 0; Index < BytesInHex; Index++)
            {
                Output[Index] = byte.Parse(HexString.Substring(Index * 2, 2),
                    NumberStyles.HexNumber);
            }

            return Output;
        }
    }
}
