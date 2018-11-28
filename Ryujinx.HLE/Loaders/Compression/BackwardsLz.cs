using System;
using System.IO;

namespace Ryujinx.HLE.Loaders.Compression
{
    static class BackwardsLz
    {
        private class BackwardsReader
        {
            private Stream BaseStream;

            public BackwardsReader(Stream BaseStream)
            {
                this.BaseStream = BaseStream;
            }

            public byte ReadByte()
            {
                BaseStream.Seek(-1, SeekOrigin.Current);

                byte Value = (byte)BaseStream.ReadByte();

                BaseStream.Seek(-1, SeekOrigin.Current);

                return Value;
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

        public static byte[] Decompress(Stream Input, int DecompressedLength)
        {
            long End = Input.Position;

            BackwardsReader Reader = new BackwardsReader(Input);

            int AdditionalDecLength = Reader.ReadInt32();
            int StartOffset         = Reader.ReadInt32();
            int CompressedLength    = Reader.ReadInt32();

            Input.Seek(12 - StartOffset, SeekOrigin.Current);

            byte[] Dec = new byte[DecompressedLength];

            int DecompressedLengthUnpadded = CompressedLength + AdditionalDecLength;

            int DecompressionStart = DecompressedLength - DecompressedLengthUnpadded;

            int DecPos = Dec.Length;

            byte Mask   = 0;
            byte Header = 0;

            while (DecPos > DecompressionStart)
            {
                if ((Mask >>= 1) == 0)
                {
                    Header = Reader.ReadByte();
                    Mask   = 0x80;
                }

                if ((Header & Mask) == 0)
                {
                    Dec[--DecPos] = Reader.ReadByte();
                }
                else
                {
                    ushort Pair = (ushort)Reader.ReadInt16();

                    int Length   = (Pair >> 12)   + 3;
                    int Position = (Pair & 0xfff) + 3;

                    DecPos -= Length;

                    if (Length <= Position)
                    {
                        int SrcPos = DecPos + Position;

                        Buffer.BlockCopy(Dec, SrcPos, Dec, DecPos, Length);
                    }
                    else
                    {
                        for (int Offset = 0; Offset < Length; Offset++)
                        {
                            Dec[DecPos + Offset] = Dec[DecPos + Position + Offset];
                        }
                    }
                }
            }

            return Dec;
        }
    }
}