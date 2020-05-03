using LibHac.Common;
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
            // Ignore last character if HexLength % 2 != 0.
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
                    byte value = context.Memory.Read<byte>((ulong)position++);

                    if (value == 0)
                    {
                        break;
                    }

                    ms.WriteByte(value);
                }

                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        public static U8Span ReadUtf8Span(ServiceCtx context, int index = 0)
        {
            ulong position = (ulong)context.Request.PtrBuff[index].Position;
            ulong size     = (ulong)context.Request.PtrBuff[index].Size;

            ReadOnlySpan<byte> buffer = context.Memory.GetSpan(position, (int)size);

            return new U8Span(buffer);
        }

        public static string ReadUtf8StringSend(ServiceCtx context, int index = 0)
        {
            long position = context.Request.SendBuff[index].Position;
            long size     = context.Request.SendBuff[index].Size;

            using (MemoryStream ms = new MemoryStream())
            {
                while (size-- > 0)
                {
                    byte value = context.Memory.Read<byte>((ulong)position++);

                    if (value == 0)
                    {
                        break;
                    }

                    ms.WriteByte(value);
                }

                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        public static unsafe int CompareCStr(char* s1, char* s2)
        {
            int s1Index = 0;
            int s2Index = 0;

            while (s1[s1Index] != 0 && s2[s2Index] != 0 && s1[s1Index] == s2[s2Index])
            {
                s1Index += 1;
                s2Index += 1;
            }

            return s2[s2Index] - s1[s1Index];
        }

        public static unsafe int LengthCstr(char* s)
        {
            int i = 0;

            while (s[i] != '\0')
            {
                i++;
            }

            return i;
        }
    }
}
