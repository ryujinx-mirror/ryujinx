using System;

namespace Ryujinx.HLE.Loaders.Compression
{
    static class Lz4
    {
        public static byte[] Decompress(byte[] Cmp, int DecLength)
        {
            byte[] Dec = new byte[DecLength];

            int CmpPos = 0;
            int DecPos = 0;

            int GetLength(int Length)
            {
                byte Sum;

                if (Length == 0xf)
                {
                    do
                    {
                        Length += (Sum = Cmp[CmpPos++]);
                    }
                    while (Sum == 0xff);
                }

                return Length;
            }

            do
            {
                byte Token = Cmp[CmpPos++];

                int EncCount = (Token >> 0) & 0xf;
                int LitCount = (Token >> 4) & 0xf;

                //Copy literal chunck
                LitCount = GetLength(LitCount);

                Buffer.BlockCopy(Cmp, CmpPos, Dec, DecPos, LitCount);

                CmpPos += LitCount;
                DecPos += LitCount;

                if (CmpPos >= Cmp.Length)
                {
                    break;
                }

                //Copy compressed chunck
                int Back = Cmp[CmpPos++] << 0 |
                           Cmp[CmpPos++] << 8;

                EncCount = GetLength(EncCount) + 4;

                int EncPos = DecPos - Back;

                if (EncCount <= Back)
                {
                    Buffer.BlockCopy(Dec, EncPos, Dec, DecPos, EncCount);

                    DecPos += EncCount;
                }
                else
                {
                    while (EncCount-- > 0)
                    {
                        Dec[DecPos++] = Dec[EncPos++];
                    }
                }
            }
            while (CmpPos < Cmp.Length &&
                   DecPos < Dec.Length);

            return Dec;
        }
    }
}