using Ryujinx.HLE.HOS;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Ryujinx.HLE.Utilities
{
    static class StringUtils
    {
        public static byte[] GetFixedLengthBytes(string inputString, int size, Encoding encoding)
        {
            inputString = inputString + "\0";

            int bytesCount = encoding.GetByteCount(inputString);

            byte[] output = new byte[size];

            if (bytesCount < size)
            {
                encoding.GetBytes(inputString, 0, inputString.Length, output, 0);
            }
            else
            {
                int nullSize = encoding.GetByteCount("\0");

                output = encoding.GetBytes(inputString);

                Array.Resize(ref output, size - nullSize);

                output = output.Concat(encoding.GetBytes("\0")).ToArray();
            }

            return output;
        }

        public static byte[] HexToBytes(string hexString)
        {
            //Ignore last charactor if HexLength % 2 != 0.
            int bytesInHex = hexString.Length / 2;

            byte[] output = new byte[bytesInHex];

            for (int index = 0; index < bytesInHex; index++)
            {
                output[index] = byte.Parse(hexString.Substring(index * 2, 2), NumberStyles.HexNumber);
            }

            return output;
        }

        public static string ReadUtf8String(ServiceCtx context, int index = 0)
        {
            long position = context.Request.PtrBuff[index].Position;
            long size     = context.Request.PtrBuff[index].Size;

            using (MemoryStream ms = new MemoryStream())
            {
                while (size-- > 0)
                {
                    byte value = context.Memory.ReadByte(position++);

                    if (value == 0)
                    {
                        break;
                    }

                    ms.WriteByte(value);
                }

                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }
    }
}
