namespace Ryujinx.Graphics.Texture.Astc
{
    internal static class Bits
    {
        public static readonly ushort[] Replicate8_16Table;
        public static readonly byte[] Replicate1_7Table;

        static Bits()
        {
            Replicate8_16Table = new ushort[0x200];
            Replicate1_7Table = new byte[0x200];

            for (int i = 0; i < 0x200; i++)
            {
                Replicate8_16Table[i] = (ushort)Replicate(i, 8, 16);
                Replicate1_7Table[i] = (byte)Replicate(i, 1, 7);
            }
        }

        public static int Replicate8_16(int value)
        {
            return Replicate8_16Table[value];
        }

        public static int Replicate1_7(int value)
        {
            return Replicate1_7Table[value];
        }

        public static int Replicate(int value, int numberBits, int toBit)
        {
            if (numberBits == 0)
            {
                return 0;
            }

            if (toBit == 0)
            {
                return 0;
            }

            int tempValue = value & ((1 << numberBits) - 1);
            int retValue = tempValue;
            int resLength = numberBits;

            while (resLength < toBit)
            {
                int comp = 0;
                if (numberBits > toBit - resLength)
                {
                    int newShift = toBit - resLength;
                    comp = numberBits - newShift;
                    numberBits = newShift;
                }
                retValue <<= numberBits;
                retValue |= tempValue >> comp;
                resLength += numberBits;
            }

            return retValue;
        }

        // Transfers a bit as described in C.2.14
        public static void BitTransferSigned(ref int a, ref int b)
        {
            b >>= 1;
            b |= a & 0x80;
            a >>= 1;
            a &= 0x3F;
            if ((a & 0x20) != 0)
            {
                a -= 0x40;
            }
        }
    }
}
