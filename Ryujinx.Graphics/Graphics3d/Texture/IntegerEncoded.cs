using System.Collections;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Texture
{
    public struct IntegerEncoded
    {
        public enum EIntegerEncoding
        {
            JustBits,
            Quint,
            Trit
        }

        EIntegerEncoding _encoding;
        public int NumberBits { get; private set; }
        public int BitValue   { get; private set; }
        public int TritValue  { get; private set; }
        public int QuintValue { get; private set; }

        public IntegerEncoded(EIntegerEncoding encoding, int numBits)
        {
            _encoding  = encoding;
            NumberBits = numBits;
            BitValue   = 0;
            TritValue  = 0;
            QuintValue = 0;
        }

        public bool MatchesEncoding(IntegerEncoded other)
        {
            return _encoding == other._encoding && NumberBits == other.NumberBits;
        }

        public EIntegerEncoding GetEncoding()
        {
            return _encoding;
        }

        public int GetBitLength(int numberVals)
        {
            int totalBits = NumberBits * numberVals;
            if (_encoding == EIntegerEncoding.Trit)
            {
                totalBits += (numberVals * 8 + 4) / 5;
            }
            else if (_encoding == EIntegerEncoding.Quint)
            {
                totalBits += (numberVals * 7 + 2) / 3;
            }
            return totalBits;
        }

        public static IntegerEncoded CreateEncoding(int maxVal)
        {
            while (maxVal > 0)
            {
                int check = maxVal + 1;

                // Is maxVal a power of two?
                if ((check & (check - 1)) == 0)
                {
                    return new IntegerEncoded(EIntegerEncoding.JustBits, BitArrayStream.PopCnt(maxVal));
                }

                // Is maxVal of the type 3*2^n - 1?
                if ((check % 3 == 0) && ((check / 3) & ((check / 3) - 1)) == 0)
                {
                    return new IntegerEncoded(EIntegerEncoding.Trit, BitArrayStream.PopCnt(check / 3 - 1));
                }

                // Is maxVal of the type 5*2^n - 1?
                if ((check % 5 == 0) && ((check / 5) & ((check / 5) - 1)) == 0)
                {
                    return new IntegerEncoded(EIntegerEncoding.Quint, BitArrayStream.PopCnt(check / 5 - 1));
                }

                // Apparently it can't be represented with a bounded integer sequence...
                // just iterate.
                maxVal--;
            }

            return new IntegerEncoded(EIntegerEncoding.JustBits, 0);
        }

        public static void DecodeTritBlock(
            BitArrayStream       bitStream, 
            List<IntegerEncoded> listIntegerEncoded, 
            int                  numberBitsPerValue)
        {
            // Implement the algorithm in section C.2.12
            int[] m = new int[5];
            int[] t = new int[5];
            int T;

            // Read the trit encoded block according to
            // table C.2.14
            m[0] = bitStream.ReadBits(numberBitsPerValue);
            T    = bitStream.ReadBits(2);
            m[1] = bitStream.ReadBits(numberBitsPerValue);
            T   |= bitStream.ReadBits(2) << 2;
            m[2] = bitStream.ReadBits(numberBitsPerValue);
            T   |= bitStream.ReadBits(1) << 4;
            m[3] = bitStream.ReadBits(numberBitsPerValue);
            T   |= bitStream.ReadBits(2) << 5;
            m[4] = bitStream.ReadBits(numberBitsPerValue);
            T   |= bitStream.ReadBits(1) << 7;

            int c = 0;

            BitArrayStream tb = new BitArrayStream(new BitArray(new int[] { T }));
            if (tb.ReadBits(2, 4) == 7)
            {
                c    = (tb.ReadBits(5, 7) << 2) | tb.ReadBits(0, 1);
                t[4] = t[3] = 2;
            }
            else
            {
                c = tb.ReadBits(0, 4);
                if (tb.ReadBits(5, 6) == 3)
                {
                    t[4] = 2;
                    t[3] = tb.ReadBit(7);
                }
                else
                {
                    t[4] = tb.ReadBit(7);
                    t[3] = tb.ReadBits(5, 6);
                }
            }

            BitArrayStream cb = new BitArrayStream(new BitArray(new int[] { c }));
            if (cb.ReadBits(0, 1) == 3)
            {
                t[2] = 2;
                t[1] = cb.ReadBit(4);
                t[0] = (cb.ReadBit(3) << 1) | (cb.ReadBit(2) & ~cb.ReadBit(3));
            }
            else if (cb.ReadBits(2, 3) == 3)
            {
                t[2] = 2;
                t[1] = 2;
                t[0] = cb.ReadBits(0, 1);
            }
            else
            {
                t[2] = cb.ReadBit(4);
                t[1] = cb.ReadBits(2, 3);
                t[0] = (cb.ReadBit(1) << 1) | (cb.ReadBit(0) & ~cb.ReadBit(1));
            }

            for (int i = 0; i < 5; i++)
            {
                IntegerEncoded intEncoded = new IntegerEncoded(EIntegerEncoding.Trit, numberBitsPerValue)
                {
                    BitValue  = m[i],
                    TritValue = t[i]
                };
                listIntegerEncoded.Add(intEncoded);
            }
        }

        public static void DecodeQuintBlock(
            BitArrayStream       bitStream, 
            List<IntegerEncoded> listIntegerEncoded, 
            int                  numberBitsPerValue)
        {
            // Implement the algorithm in section C.2.12
            int[] m = new int[3];
            int[] qa = new int[3];
            int q;

            // Read the trit encoded block according to
            // table C.2.15
            m[0] = bitStream.ReadBits(numberBitsPerValue);
            q    = bitStream.ReadBits(3);
            m[1] = bitStream.ReadBits(numberBitsPerValue);
            q   |= bitStream.ReadBits(2) << 3;
            m[2] = bitStream.ReadBits(numberBitsPerValue);
            q   |= bitStream.ReadBits(2) << 5;

            BitArrayStream qb = new BitArrayStream(new BitArray(new int[] { q }));
            if (qb.ReadBits(1, 2) == 3 && qb.ReadBits(5, 6) == 0)
            {
                qa[0] = qa[1] = 4;
                qa[2] = (qb.ReadBit(0) << 2) | ((qb.ReadBit(4) & ~qb.ReadBit(0)) << 1) | (qb.ReadBit(3) & ~qb.ReadBit(0));
            }
            else
            {
                int c = 0;
                if (qb.ReadBits(1, 2) == 3)
                {
                    qa[2] = 4;
                    c    = (qb.ReadBits(3, 4) << 3) | ((~qb.ReadBits(5, 6) & 3) << 1) | qb.ReadBit(0);
                }
                else
                {
                    qa[2] = qb.ReadBits(5, 6);
                    c    = qb.ReadBits(0, 4);
                }

                BitArrayStream cb = new BitArrayStream(new BitArray(new int[] { c }));
                if (cb.ReadBits(0, 2) == 5)
                {
                    qa[1] = 4;
                    qa[0] = cb.ReadBits(3, 4);
                }
                else
                {
                    qa[1] = cb.ReadBits(3, 4);
                    qa[0] = cb.ReadBits(0, 2);
                }
            }

            for (int i = 0; i < 3; i++)
            {
                IntegerEncoded intEncoded = new IntegerEncoded(EIntegerEncoding.Quint, numberBitsPerValue)
                {
                    BitValue   = m[i],
                    QuintValue = qa[i]
                };
                listIntegerEncoded.Add(intEncoded);
            }
        }

        public static void DecodeIntegerSequence(
            List<IntegerEncoded> decodeIntegerSequence, 
            BitArrayStream       bitStream, 
            int                  maxRange, 
            int                  numberValues)
        {
            // Determine encoding parameters
            IntegerEncoded intEncoded = CreateEncoding(maxRange);

            // Start decoding
            int numberValuesDecoded = 0;
            while (numberValuesDecoded < numberValues)
            {
                switch (intEncoded.GetEncoding())
                {
                    case EIntegerEncoding.Quint:
                    {
                        DecodeQuintBlock(bitStream, decodeIntegerSequence, intEncoded.NumberBits);
                        numberValuesDecoded += 3;

                        break;
                    }

                    case EIntegerEncoding.Trit:
                    {
                        DecodeTritBlock(bitStream, decodeIntegerSequence, intEncoded.NumberBits);
                        numberValuesDecoded += 5;

                        break;
                    }

                    case EIntegerEncoding.JustBits:
                    {
                        intEncoded.BitValue = bitStream.ReadBits(intEncoded.NumberBits);
                        decodeIntegerSequence.Add(intEncoded);
                        numberValuesDecoded++;

                        break;
                    }
                }
            }
        }
    }
}
