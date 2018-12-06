using System;
using System.IO;

namespace Ryujinx.HLE.Loaders.Compression
{
    static class BackwardsLz
    {
        private class BackwardsReader
        {
            private Stream _baseStream;

            public BackwardsReader(Stream baseStream)
            {
                _baseStream = baseStream;
            }

            public byte ReadByte()
            {
                _baseStream.Seek(-1, SeekOrigin.Current);

                byte value = (byte)_baseStream.ReadByte();

                _baseStream.Seek(-1, SeekOrigin.Current);

                return value;
            }

            public short ReadInt16()
            {
                return (short)((ReadByte() << 8) | (ReadByte() << 0));
            }

            public int ReadInt32()
            {
                return ((ReadByte() << 24) |
                        (ReadByte() << 16) |
                        (ReadByte() << 8)  |
                        (ReadByte() << 0));
            }
        }

        public static byte[] Decompress(Stream input, int decompressedLength)
        {
            long end = input.Position;

            BackwardsReader reader = new BackwardsReader(input);

            int additionalDecLength = reader.ReadInt32();
            int startOffset         = reader.ReadInt32();
            int compressedLength    = reader.ReadInt32();

            input.Seek(12 - startOffset, SeekOrigin.Current);

            byte[] dec = new byte[decompressedLength];

            int decompressedLengthUnpadded = compressedLength + additionalDecLength;

            int decompressionStart = decompressedLength - decompressedLengthUnpadded;

            int decPos = dec.Length;

            byte mask   = 0;
            byte header = 0;

            while (decPos > decompressionStart)
            {
                if ((mask >>= 1) == 0)
                {
                    header = reader.ReadByte();
                    mask   = 0x80;
                }

                if ((header & mask) == 0)
                {
                    dec[--decPos] = reader.ReadByte();
                }
                else
                {
                    ushort pair = (ushort)reader.ReadInt16();

                    int length   = (pair >> 12)   + 3;
                    int position = (pair & 0xfff) + 3;

                    decPos -= length;

                    if (length <= position)
                    {
                        int srcPos = decPos + position;

                        Buffer.BlockCopy(dec, srcPos, dec, decPos, length);
                    }
                    else
                    {
                        for (int offset = 0; offset < length; offset++)
                        {
                            dec[decPos + offset] = dec[decPos + position + offset];
                        }
                    }
                }
            }

            return dec;
        }
    }
}