using System;

namespace Ryujinx.HLE.Loaders.Compression
{
    static class BackwardsLz
    {
        private class BackwardsReader
        {
            private byte[] _data;

            private int _position;

            public int Position => _position;

            public BackwardsReader(byte[] data, int end)
            {
                _data     = data;
                _position = end;
            }

            public void SeekCurrent(int offset)
            {
                _position += offset;
            }

            public byte ReadByte()
            {
                return _data[--_position];
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

        public static void DecompressInPlace(byte[] buffer, int headerEnd)
        {
            BackwardsReader reader = new BackwardsReader(buffer, headerEnd);

            int additionalDecLength = reader.ReadInt32();
            int startOffset         = reader.ReadInt32();
            int compressedLength    = reader.ReadInt32();

            reader.SeekCurrent(12 - startOffset);

            int decBase = headerEnd - compressedLength;

            int decPos = compressedLength + additionalDecLength;

            byte mask   = 0;
            byte header = 0;

            while (decPos > 0)
            {
                if ((mask >>= 1) == 0)
                {
                    header = reader.ReadByte();
                    mask   = 0x80;
                }

                if ((header & mask) == 0)
                {
                    buffer[decBase + --decPos] = reader.ReadByte();
                }
                else
                {
                    ushort pair = (ushort)reader.ReadInt16();

                    int length   = (pair >> 12)   + 3;
                    int position = (pair & 0xfff) + 3;

                    if (length > decPos)
                    {
                        length = decPos;
                    }

                    decPos -= length;

                    int dstPos = decBase + decPos;

                    if (length <= position)
                    {
                        int srcPos = dstPos + position;

                        Buffer.BlockCopy(buffer, srcPos, buffer, dstPos, length);
                    }
                    else
                    {
                        for (int offset = 0; offset < length; offset++)
                        {
                            buffer[dstPos + offset] = buffer[dstPos + position + offset];
                        }
                    }
                }
            }
        }
    }
}