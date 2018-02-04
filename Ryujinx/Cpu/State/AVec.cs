using System;
using System.Runtime.InteropServices;

namespace ChocolArm64.State
{
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    public struct AVec
    {
        [FieldOffset(0x0)] public byte B0;
        [FieldOffset(0x1)] public byte B1;
        [FieldOffset(0x2)] public byte B2;
        [FieldOffset(0x3)] public byte B3;
        [FieldOffset(0x4)] public byte B4;
        [FieldOffset(0x5)] public byte B5;
        [FieldOffset(0x6)] public byte B6;
        [FieldOffset(0x7)] public byte B7;
        [FieldOffset(0x8)] public byte B8;
        [FieldOffset(0x9)] public byte B9;
        [FieldOffset(0xa)] public byte B10;
        [FieldOffset(0xb)] public byte B11;
        [FieldOffset(0xc)] public byte B12;
        [FieldOffset(0xd)] public byte B13;
        [FieldOffset(0xe)] public byte B14;
        [FieldOffset(0xf)] public byte B15;

        [FieldOffset(0x0)] public ushort H0;
        [FieldOffset(0x2)] public ushort H1;
        [FieldOffset(0x4)] public ushort H2;
        [FieldOffset(0x6)] public ushort H3;
        [FieldOffset(0x8)] public ushort H4;
        [FieldOffset(0xa)] public ushort H5;
        [FieldOffset(0xc)] public ushort H6;
        [FieldOffset(0xe)] public ushort H7;

        [FieldOffset(0x0)] public uint W0;
        [FieldOffset(0x4)] public uint W1;
        [FieldOffset(0x8)] public uint W2;
        [FieldOffset(0xc)] public uint W3;

        [FieldOffset(0x0)] public float S0;
        [FieldOffset(0x4)] public float S1;
        [FieldOffset(0x8)] public float S2;
        [FieldOffset(0xc)] public float S3;

        [FieldOffset(0x0)] public ulong X0;
        [FieldOffset(0x8)] public ulong X1;

        [FieldOffset(0x0)] public double D0;
        [FieldOffset(0x8)] public double D1;

        public byte ExtractByte(int Index)
        {
            switch (Index)
            {
                case 0:  return B0;
                case 1:  return B1;
                case 2:  return B2;
                case 3:  return B3;
                case 4:  return B4;
                case 5:  return B5;
                case 6:  return B6;
                case 7:  return B7;
                case 8:  return B8;
                case 9:  return B9;
                case 10: return B10;
                case 11: return B11;
                case 12: return B12;
                case 13: return B13;
                case 14: return B14;
                case 15: return B15;
            }

            throw new ArgumentOutOfRangeException(nameof(Index));
        }

        public ushort ExtractUInt16(int Index)
        {
            switch (Index)
            {
                case 0: return H0;
                case 1: return H1;
                case 2: return H2;
                case 3: return H3;
                case 4: return H4;
                case 5: return H5;
                case 6: return H6;
                case 7: return H7;
            }

            throw new ArgumentOutOfRangeException(nameof(Index));
        }

        public uint ExtractUInt32(int Index)
        {
            switch (Index)
            {
                case 0: return W0;
                case 1: return W1;
                case 2: return W2;
                case 3: return W3;
            }

            throw new ArgumentOutOfRangeException(nameof(Index));
        }

        public float ExtractSingle(int Index)
        {
            switch (Index)
            {
                case 0: return S0;
                case 1: return S1;
                case 2: return S2;
                case 3: return S3;
            }

            throw new ArgumentOutOfRangeException(nameof(Index));
        }

        public ulong ExtractUInt64(int Index)
        {
            switch (Index)
            {
                case 0: return X0;
                case 1: return X1;
            }

            throw new ArgumentOutOfRangeException(nameof(Index));
        }

        public double ExtractDouble(int Index)
        {
            switch (Index)
            {
                case 0: return D0;
                case 1: return D1;
            }

            throw new ArgumentOutOfRangeException(nameof(Index));
        }

        public static AVec InsertByte(AVec Vec, int Index, byte Value)
        {
            switch (Index)
            {
                case 0:  Vec.B0  = Value; break;
                case 1:  Vec.B1  = Value; break;
                case 2:  Vec.B2  = Value; break;
                case 3:  Vec.B3  = Value; break;
                case 4:  Vec.B4  = Value; break;
                case 5:  Vec.B5  = Value; break;
                case 6:  Vec.B6  = Value; break;
                case 7:  Vec.B7  = Value; break;
                case 8:  Vec.B8  = Value; break;
                case 9:  Vec.B9  = Value; break;
                case 10: Vec.B10 = Value; break;
                case 11: Vec.B11 = Value; break;
                case 12: Vec.B12 = Value; break;
                case 13: Vec.B13 = Value; break;
                case 14: Vec.B14 = Value; break;
                case 15: Vec.B15 = Value; break;

                default: throw new ArgumentOutOfRangeException(nameof(Index));
            }

            return Vec;
        }

        public static AVec InsertUInt16(AVec Vec, int Index, ushort Value)
        {
            switch (Index)
            {
                case 0: Vec.H0 = Value; break;
                case 1: Vec.H1 = Value; break;
                case 2: Vec.H2 = Value; break;
                case 3: Vec.H3 = Value; break;
                case 4: Vec.H4 = Value; break;
                case 5: Vec.H5 = Value; break;
                case 6: Vec.H6 = Value; break;
                case 7: Vec.H7 = Value; break;

                default: throw new ArgumentOutOfRangeException(nameof(Index));
            }

            return Vec;
        }

        public static AVec InsertUInt32(AVec Vec, int Index, uint Value)
        {
            switch (Index)
            {
                case 0: Vec.W0 = Value; break;
                case 1: Vec.W1 = Value; break;
                case 2: Vec.W2 = Value; break;
                case 3: Vec.W3 = Value; break;

                default: throw new ArgumentOutOfRangeException(nameof(Index));
            }

            return Vec;
        }

        public static AVec InsertSingle(AVec Vec, int Index, float Value)
        {
            switch (Index)
            {
                case 0: Vec.S0 = Value; break;
                case 1: Vec.S1 = Value; break;
                case 2: Vec.S2 = Value; break;
                case 3: Vec.S3 = Value; break;

                default: throw new ArgumentOutOfRangeException(nameof(Index));
            }

            return Vec;
        }

        public static AVec InsertUInt64(AVec Vec, int Index, ulong Value)
        {
            switch (Index)
            {
                case 0: Vec.X0 = Value; break;
                case 1: Vec.X1 = Value; break;

                default: throw new ArgumentOutOfRangeException(nameof(Index));
            }

            return Vec;
        }

        public static AVec InsertDouble(AVec Vec, int Index, double Value)
        {
            switch (Index)
            {
                case 0: Vec.D0 = Value; break;
                case 1: Vec.D1 = Value; break;

                default: throw new ArgumentOutOfRangeException(nameof(Index));
            }

            return Vec;
        }
    }
}