using System;
using System.Text;

namespace Ryujinx.Common
{
    public static class HexUtils
    {
        private static readonly char[] _hexChars = "0123456789ABCDEF".ToCharArray();

        private const int HexTableColumnWidth = 8;
        private const int HexTableColumnSpace = 3;

        // Modified for Ryujinx
        // Original by Pascal Ganaye - CPOL License
        // https://www.codeproject.com/Articles/36747/Quick-and-Dirty-HexDump-of-a-Byte-Array
        public static string HexTable(byte[] bytes, int bytesPerLine = 16)
        {
            if (bytes == null)
            {
                return "<null>";
            }

            int bytesLength = bytes.Length;

            int firstHexColumn =
                  HexTableColumnWidth
                + HexTableColumnSpace;

            int firstCharColumn = firstHexColumn
                + bytesPerLine * HexTableColumnSpace
                + (bytesPerLine - 1) / HexTableColumnWidth
                + 2;

            int lineLength = firstCharColumn
                + bytesPerLine
                + Environment.NewLine.Length;

            char[] line = (new String(' ', lineLength - Environment.NewLine.Length) + Environment.NewLine).ToCharArray();

            int expectedLines = (bytesLength + bytesPerLine - 1) / bytesPerLine;

            StringBuilder result = new(expectedLines * lineLength);

            for (int i = 0; i < bytesLength; i += bytesPerLine)
            {
                line[0] = _hexChars[(i >> 28) & 0xF];
                line[1] = _hexChars[(i >> 24) & 0xF];
                line[2] = _hexChars[(i >> 20) & 0xF];
                line[3] = _hexChars[(i >> 16) & 0xF];
                line[4] = _hexChars[(i >> 12) & 0xF];
                line[5] = _hexChars[(i >> 8) & 0xF];
                line[6] = _hexChars[(i >> 4) & 0xF];
                line[7] = _hexChars[(i >> 0) & 0xF];

                int hexColumn = firstHexColumn;
                int charColumn = firstCharColumn;

                for (int j = 0; j < bytesPerLine; j++)
                {
                    if (j > 0 && (j & 7) == 0)
                    {
                        hexColumn++;
                    }

                    if (i + j >= bytesLength)
                    {
                        line[hexColumn] = ' ';
                        line[hexColumn + 1] = ' ';
                        line[charColumn] = ' ';
                    }
                    else
                    {
                        byte b = bytes[i + j];

                        line[hexColumn] = _hexChars[(b >> 4) & 0xF];
                        line[hexColumn + 1] = _hexChars[b & 0xF];
                        line[charColumn] = (b < 32 ? '·' : (char)b);
                    }

                    hexColumn += 3;
                    charColumn++;
                }

                result.Append(line);
            }

            return result.ToString();
        }
    }
}
