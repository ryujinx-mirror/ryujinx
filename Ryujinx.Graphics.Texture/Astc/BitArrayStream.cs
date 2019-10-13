using System;
using System.Collections;

namespace Ryujinx.Graphics.Texture.Astc
{
    public class BitArrayStream
    {
        public BitArray BitsArray;

        public int Position { get; private set; }

        public BitArrayStream(BitArray bitArray)
        {
            BitsArray = bitArray;
            Position  = 0;
        }

        public short ReadBits(int length)
        {
            int retValue = 0;
            for (int i = Position; i < Position + length; i++)
            {
                if (BitsArray[i])
                {
                    retValue |= 1 << (i - Position);
                }
            }

            Position += length;
            return (short)retValue;
        }

        public int ReadBits(int start, int end)
        {
            int retValue = 0;
            for (int i = start; i <= end; i++)
            {
                if (BitsArray[i])
                {
                    retValue |= 1 << (i - start);
                }
            }

            return retValue;
        }

        public int ReadBit(int index)
        {
            return Convert.ToInt32(BitsArray[index]);
        }

        public void WriteBits(int value, int length)
        {
            for (int i = Position; i < Position + length; i++)
            {
                BitsArray[i] = ((value >> (i - Position)) & 1) != 0;
            }

            Position += length;
        }

        public byte[] ToByteArray()
        {
            byte[] retArray = new byte[(BitsArray.Length + 7) / 8];
            BitsArray.CopyTo(retArray, 0);
            return retArray;
        }

        public static int Replicate(int value, int numberBits, int toBit)
        {
            if (numberBits == 0) return 0;
            if (toBit == 0) return 0;

            int tempValue = value & ((1 << numberBits) - 1);
            int retValue  = tempValue;
            int resLength = numberBits;

            while (resLength < toBit)
            {
                int comp = 0;
                if (numberBits > toBit - resLength)
                {
                    int newShift = toBit - resLength;
                    comp         = numberBits - newShift;
                    numberBits   = newShift;
                }
                retValue <<= numberBits;
                retValue  |= tempValue >> comp;
                resLength += numberBits;
            }
            return retValue;
        }

        public static int PopCnt(int number)
        {
            int counter;
            for (counter = 0; number != 0; counter++)
            {
                number &= number - 1;
            }
            return counter;
        }

        public static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp = lhs;
            lhs = rhs;
            rhs = temp;
        }

        // Transfers a bit as described in C.2.14
        public static void BitTransferSigned(ref int a, ref int b)
        {
            b >>= 1;
            b |= a & 0x80;
            a >>= 1;
            a &= 0x3F;
            if ((a & 0x20) != 0) a -= 0x40;
        }
    }
}
