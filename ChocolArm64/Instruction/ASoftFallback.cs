using ChocolArm64.State;
using ChocolArm64.Translation;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace ChocolArm64.Instruction
{
    using static AVectorHelper;

    static class ASoftFallback
    {
        public static void EmitCall(AILEmitterCtx Context, string MthdName)
        {
            Context.EmitCall(typeof(ASoftFallback), MthdName);
        }

#region "Saturating"
        public static long SignedSrcSignedDstSatQ(long op, int Size, AThreadState State)
        {
            int ESize = 8 << Size;

            long TMaxValue =  (1L << (ESize - 1)) - 1L;
            long TMinValue = -(1L << (ESize - 1));

            if (op > TMaxValue)
            {
                SetFpsrQCFlag(State);

                return TMaxValue;
            }
            else if (op < TMinValue)
            {
                SetFpsrQCFlag(State);

                return TMinValue;
            }
            else
            {
                return op;
            }
        }

        public static ulong SignedSrcUnsignedDstSatQ(long op, int Size, AThreadState State)
        {
            int ESize = 8 << Size;

            ulong TMaxValue = (1UL << ESize) - 1UL;
            ulong TMinValue =  0UL;

            if (op > (long)TMaxValue)
            {
                SetFpsrQCFlag(State);

                return TMaxValue;
            }
            else if (op < (long)TMinValue)
            {
                SetFpsrQCFlag(State);

                return TMinValue;
            }
            else
            {
                return (ulong)op;
            }
        }

        public static long UnsignedSrcSignedDstSatQ(ulong op, int Size, AThreadState State)
        {
            int ESize = 8 << Size;

            long TMaxValue = (1L << (ESize - 1)) - 1L;

            if (op > (ulong)TMaxValue)
            {
                SetFpsrQCFlag(State);

                return TMaxValue;
            }
            else
            {
                return (long)op;
            }
        }

        public static ulong UnsignedSrcUnsignedDstSatQ(ulong op, int Size, AThreadState State)
        {
            int ESize = 8 << Size;

            ulong TMaxValue = (1UL << ESize) - 1UL;

            if (op > TMaxValue)
            {
                SetFpsrQCFlag(State);

                return TMaxValue;
            }
            else
            {
                return op;
            }
        }

        public static long UnarySignedSatQAbsOrNeg(long op, AThreadState State)
        {
            if (op == long.MinValue)
            {
                SetFpsrQCFlag(State);

                return long.MaxValue;
            }
            else
            {
                return op;
            }
        }

        public static long BinarySignedSatQAdd(long op1, long op2, AThreadState State)
        {
            long Add = op1 + op2;

            if ((~(op1 ^ op2) & (op1 ^ Add)) < 0L)
            {
                SetFpsrQCFlag(State);

                if (op1 < 0L)
                {
                    return long.MinValue;
                }
                else
                {
                    return long.MaxValue;
                }
            }
            else
            {
                return Add;
            }
        }

        public static ulong BinaryUnsignedSatQAdd(ulong op1, ulong op2, AThreadState State)
        {
            ulong Add = op1 + op2;

            if ((Add < op1) && (Add < op2))
            {
                SetFpsrQCFlag(State);

                return ulong.MaxValue;
            }
            else
            {
                return Add;
            }
        }

        public static long BinarySignedSatQSub(long op1, long op2, AThreadState State)
        {
            long Sub = op1 - op2;

            if (((op1 ^ op2) & (op1 ^ Sub)) < 0L)
            {
                SetFpsrQCFlag(State);

                if (op1 < 0L)
                {
                    return long.MinValue;
                }
                else
                {
                    return long.MaxValue;
                }
            }
            else
            {
                return Sub;
            }
        }

        public static ulong BinaryUnsignedSatQSub(ulong op1, ulong op2, AThreadState State)
        {
            ulong Sub = op1 - op2;

            if (op1 < op2)
            {
                SetFpsrQCFlag(State);

                return ulong.MinValue;
            }
            else
            {
                return Sub;
            }
        }

        public static long BinarySignedSatQAcc(ulong op1, long op2, AThreadState State)
        {
            if (op1 <= (ulong)long.MaxValue)
            {
                // op1 from ulong.MinValue to (ulong)long.MaxValue
                // op2 from long.MinValue to long.MaxValue

                long Add = (long)op1 + op2;

                if ((~op2 & Add) < 0L)
                {
                    SetFpsrQCFlag(State);

                    return long.MaxValue;
                }
                else
                {
                    return Add;
                }
            }
            else if (op2 >= 0L)
            {
                // op1 from (ulong)long.MaxValue + 1UL to ulong.MaxValue
                // op2 from (long)ulong.MinValue to long.MaxValue

                SetFpsrQCFlag(State);

                return long.MaxValue;
            }
            else
            {
                // op1 from (ulong)long.MaxValue + 1UL to ulong.MaxValue
                // op2 from long.MinValue to (long)ulong.MinValue - 1L

                ulong Add = op1 + (ulong)op2;

                if (Add > (ulong)long.MaxValue)
                {
                    SetFpsrQCFlag(State);

                    return long.MaxValue;
                }
                else
                {
                    return (long)Add;
                }
            }
        }

        public static ulong BinaryUnsignedSatQAcc(long op1, ulong op2, AThreadState State)
        {
            if (op1 >= 0L)
            {
                // op1 from (long)ulong.MinValue to long.MaxValue
                // op2 from ulong.MinValue to ulong.MaxValue

                ulong Add = (ulong)op1 + op2;

                if ((Add < (ulong)op1) && (Add < op2))
                {
                    SetFpsrQCFlag(State);

                    return ulong.MaxValue;
                }
                else
                {
                    return Add;
                }
            }
            else if (op2 > (ulong)long.MaxValue)
            {
                // op1 from long.MinValue to (long)ulong.MinValue - 1L
                // op2 from (ulong)long.MaxValue + 1UL to ulong.MaxValue

                return (ulong)op1 + op2;
            }
            else
            {
                // op1 from long.MinValue to (long)ulong.MinValue - 1L
                // op2 from ulong.MinValue to (ulong)long.MaxValue

                long Add = op1 + (long)op2;

                if (Add < (long)ulong.MinValue)
                {
                    SetFpsrQCFlag(State);

                    return ulong.MinValue;
                }
                else
                {
                    return (ulong)Add;
                }
            }
        }

        private static void SetFpsrQCFlag(AThreadState State)
        {
            const int QCFlagBit = 27;

            State.Fpsr |= 1 << QCFlagBit;
        }
#endregion

#region "Count"
        public static ulong CountLeadingSigns(ulong Value, int Size)
        {
            Value ^= Value >> 1;

            int HighBit = Size - 2;

            for (int Bit = HighBit; Bit >= 0; Bit--)
            {
                if (((Value >> Bit) & 0b1) != 0)
                {
                    return (ulong)(HighBit - Bit);
                }
            }

            return (ulong)(Size - 1);
        }

        private static readonly byte[] ClzNibbleTbl = { 4, 3, 2, 2, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0 };

        public static ulong CountLeadingZeros(ulong Value, int Size)
        {
            if (Value == 0)
            {
                return (ulong)Size;
            }

            int NibbleIdx = Size;
            int PreCount, Count = 0;

            do
            {
                NibbleIdx -= 4;
                PreCount = ClzNibbleTbl[(Value >> NibbleIdx) & 0b1111];
                Count += PreCount;
            }
            while (PreCount == 4);

            return (ulong)Count;
        }

        public static uint CountSetBits8(uint Value)
        {
            Value = ((Value >> 1) & 0x55) + (Value & 0x55);
            Value = ((Value >> 2) & 0x33) + (Value & 0x33);

            return (Value >> 4) + (Value & 0x0f);
        }
#endregion

#region "Crc32"
        private const uint Crc32RevPoly  = 0xedb88320;
        private const uint Crc32cRevPoly = 0x82f63b78;

        public static uint Crc32b(uint Crc, byte   Val) => Crc32 (Crc, Crc32RevPoly, Val);
        public static uint Crc32h(uint Crc, ushort Val) => Crc32h(Crc, Crc32RevPoly, Val);
        public static uint Crc32w(uint Crc, uint   Val) => Crc32w(Crc, Crc32RevPoly, Val);
        public static uint Crc32x(uint Crc, ulong  Val) => Crc32x(Crc, Crc32RevPoly, Val);

        public static uint Crc32cb(uint Crc, byte   Val) => Crc32 (Crc, Crc32cRevPoly, Val);
        public static uint Crc32ch(uint Crc, ushort Val) => Crc32h(Crc, Crc32cRevPoly, Val);
        public static uint Crc32cw(uint Crc, uint   Val) => Crc32w(Crc, Crc32cRevPoly, Val);
        public static uint Crc32cx(uint Crc, ulong  Val) => Crc32x(Crc, Crc32cRevPoly, Val);

        private static uint Crc32h(uint Crc, uint Poly, ushort Val)
        {
            Crc = Crc32(Crc, Poly, (byte)(Val >> 0));
            Crc = Crc32(Crc, Poly, (byte)(Val >> 8));

            return Crc;
        }

        private static uint Crc32w(uint Crc, uint Poly, uint Val)
        {
            Crc = Crc32(Crc, Poly, (byte)(Val >> 0 ));
            Crc = Crc32(Crc, Poly, (byte)(Val >> 8 ));
            Crc = Crc32(Crc, Poly, (byte)(Val >> 16));
            Crc = Crc32(Crc, Poly, (byte)(Val >> 24));

            return Crc;
        }

        private static uint Crc32x(uint Crc, uint Poly, ulong Val)
        {
            Crc = Crc32(Crc, Poly, (byte)(Val >> 0 ));
            Crc = Crc32(Crc, Poly, (byte)(Val >> 8 ));
            Crc = Crc32(Crc, Poly, (byte)(Val >> 16));
            Crc = Crc32(Crc, Poly, (byte)(Val >> 24));
            Crc = Crc32(Crc, Poly, (byte)(Val >> 32));
            Crc = Crc32(Crc, Poly, (byte)(Val >> 40));
            Crc = Crc32(Crc, Poly, (byte)(Val >> 48));
            Crc = Crc32(Crc, Poly, (byte)(Val >> 56));

            return Crc;
        }

        private static uint Crc32(uint Crc, uint Poly, byte Val)
        {
            Crc ^= Val;

            for (int Bit = 7; Bit >= 0; Bit--)
            {
                uint Mask = (uint)(-(int)(Crc & 1));

                Crc = (Crc >> 1) ^ (Poly & Mask);
            }

            return Crc;
        }
#endregion

#region "Sha256"
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<float> HashLower(Vector128<float> hash_abcd, Vector128<float> hash_efgh, Vector128<float> wk)
        {
            return SHA256hash(hash_abcd, hash_efgh, wk, true);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<float> HashUpper(Vector128<float> hash_efgh, Vector128<float> hash_abcd, Vector128<float> wk)
        {
            return SHA256hash(hash_abcd, hash_efgh, wk, false);
        }

        public static Vector128<float> SchedulePart1(Vector128<float> w0_3, Vector128<float> w4_7)
        {
            Vector128<float> result = new Vector128<float>();

            for (int e = 0; e <= 3; e++)
            {
                uint elt = (uint)VectorExtractIntZx(e <= 2 ? w0_3 : w4_7, (byte)(e <= 2 ? e + 1 : 0), 2);

                elt = elt.Ror(7) ^ elt.Ror(18) ^ elt.Lsr(3);

                elt += (uint)VectorExtractIntZx(w0_3, (byte)e, 2);

                result = VectorInsertInt((ulong)elt, result, (byte)e, 2);
            }

            return result;
        }

        public static Vector128<float> SchedulePart2(Vector128<float> w0_3, Vector128<float> w8_11, Vector128<float> w12_15)
        {
            Vector128<float> result = new Vector128<float>();

            ulong T1 = VectorExtractIntZx(w12_15, (byte)1, 3);

            for (int e = 0; e <= 1; e++)
            {
                uint elt = T1.ULongPart(e);

                elt = elt.Ror(17) ^ elt.Ror(19) ^ elt.Lsr(10);

                elt += (uint)VectorExtractIntZx(w0_3, (byte)e, 2);
                elt += (uint)VectorExtractIntZx(w8_11, (byte)(e + 1), 2);

                result = VectorInsertInt((ulong)elt, result, (byte)e, 2);
            }

            T1 = VectorExtractIntZx(result, (byte)0, 3);

            for (int e = 2; e <= 3; e++)
            {
                uint elt = T1.ULongPart(e - 2);

                elt = elt.Ror(17) ^ elt.Ror(19) ^ elt.Lsr(10);

                elt += (uint)VectorExtractIntZx(w0_3, (byte)e, 2);
                elt += (uint)VectorExtractIntZx(e == 2 ? w8_11 : w12_15, (byte)(e == 2 ? 3 : 0), 2);

                result = VectorInsertInt((ulong)elt, result, (byte)e, 2);
            }

            return result;
        }

        private static Vector128<float> SHA256hash(Vector128<float> X, Vector128<float> Y, Vector128<float> W, bool part1)
        {
            for (int e = 0; e <= 3; e++)
            {
                uint chs = SHAchoose((uint)VectorExtractIntZx(Y, (byte)0, 2),
                                     (uint)VectorExtractIntZx(Y, (byte)1, 2),
                                     (uint)VectorExtractIntZx(Y, (byte)2, 2));

                uint maj = SHAmajority((uint)VectorExtractIntZx(X, (byte)0, 2),
                                       (uint)VectorExtractIntZx(X, (byte)1, 2),
                                       (uint)VectorExtractIntZx(X, (byte)2, 2));

                uint t1 = (uint)VectorExtractIntZx(Y, (byte)3, 2);
                t1 += SHAhashSIGMA1((uint)VectorExtractIntZx(Y, (byte)0, 2)) + chs;
                t1 += (uint)VectorExtractIntZx(W, (byte)e, 2);

                uint t2 = t1 + (uint)VectorExtractIntZx(X, (byte)3, 2);
                X = VectorInsertInt((ulong)t2, X, (byte)3, 2);
                t2 = t1 + SHAhashSIGMA0((uint)VectorExtractIntZx(X, (byte)0, 2)) + maj;
                Y = VectorInsertInt((ulong)t2, Y, (byte)3, 2);

                Rol32_256(ref Y, ref X);
            }

            return part1 ? X : Y;
        }

        private static void Rol32_256(ref Vector128<float> Y, ref Vector128<float> X)
        {
            if (!Sse2.IsSupported)
            {
                throw new PlatformNotSupportedException();
            }

            uint yE3 = (uint)VectorExtractIntZx(Y, (byte)3, 2);
            uint xE3 = (uint)VectorExtractIntZx(X, (byte)3, 2);

            Y = Sse.StaticCast<uint, float>(Sse2.ShiftLeftLogical128BitLane(Sse.StaticCast<float, uint>(Y), (byte)4));
            X = Sse.StaticCast<uint, float>(Sse2.ShiftLeftLogical128BitLane(Sse.StaticCast<float, uint>(X), (byte)4));

            Y = VectorInsertInt((ulong)xE3, Y, (byte)0, 2);
            X = VectorInsertInt((ulong)yE3, X, (byte)0, 2);
        }

        private static uint SHAhashSIGMA0(uint x)
        {
            return x.Ror(2) ^ x.Ror(13) ^ x.Ror(22);
        }

        private static uint SHAhashSIGMA1(uint x)
        {
            return x.Ror(6) ^ x.Ror(11) ^ x.Ror(25);
        }

        private static uint SHAmajority(uint x, uint y, uint z)
        {
            return (x & y) | ((x | y) & z);
        }

        private static uint SHAchoose(uint x, uint y, uint z)
        {
            return ((y ^ z) & x) ^ z;
        }

        private static uint Ror(this uint value, int count)
        {
            return (value >> count) | (value << (32 - count));
        }

        private static uint Lsr(this uint value, int count)
        {
            return value >> count;
        }

        private static uint ULongPart(this ulong value, int part)
        {
            return part == 0
                ? (uint)(value & 0xFFFFFFFFUL)
                : (uint)(value >> 32);
        }
#endregion

#region "Reverse"
        public static uint ReverseBits8(uint Value)
        {
            Value = ((Value & 0xaa) >> 1) | ((Value & 0x55) << 1);
            Value = ((Value & 0xcc) >> 2) | ((Value & 0x33) << 2);

            return (Value >> 4) | ((Value & 0x0f) << 4);
        }

        public static uint ReverseBits32(uint Value)
        {
            Value = ((Value & 0xaaaaaaaa) >> 1) | ((Value & 0x55555555) << 1);
            Value = ((Value & 0xcccccccc) >> 2) | ((Value & 0x33333333) << 2);
            Value = ((Value & 0xf0f0f0f0) >> 4) | ((Value & 0x0f0f0f0f) << 4);
            Value = ((Value & 0xff00ff00) >> 8) | ((Value & 0x00ff00ff) << 8);

            return (Value >> 16) | (Value << 16);
        }

        public static ulong ReverseBits64(ulong Value)
        {
            Value = ((Value & 0xaaaaaaaaaaaaaaaa) >> 1 ) | ((Value & 0x5555555555555555) << 1 );
            Value = ((Value & 0xcccccccccccccccc) >> 2 ) | ((Value & 0x3333333333333333) << 2 );
            Value = ((Value & 0xf0f0f0f0f0f0f0f0) >> 4 ) | ((Value & 0x0f0f0f0f0f0f0f0f) << 4 );
            Value = ((Value & 0xff00ff00ff00ff00) >> 8 ) | ((Value & 0x00ff00ff00ff00ff) << 8 );
            Value = ((Value & 0xffff0000ffff0000) >> 16) | ((Value & 0x0000ffff0000ffff) << 16);

            return (Value >> 32) | (Value << 32);
        }

        public static uint ReverseBytes16_32(uint Value) => (uint)ReverseBytes16_64(Value);
        public static uint ReverseBytes32_32(uint Value) => (uint)ReverseBytes32_64(Value);

        public static ulong ReverseBytes16_64(ulong Value) => ReverseBytes(Value, RevSize.Rev16);
        public static ulong ReverseBytes32_64(ulong Value) => ReverseBytes(Value, RevSize.Rev32);
        public static ulong ReverseBytes64(ulong Value)    => ReverseBytes(Value, RevSize.Rev64);

        private enum RevSize
        {
            Rev16,
            Rev32,
            Rev64
        }

        private static ulong ReverseBytes(ulong Value, RevSize Size)
        {
            Value = ((Value & 0xff00ff00ff00ff00) >> 8) | ((Value & 0x00ff00ff00ff00ff) << 8);

            if (Size == RevSize.Rev16)
            {
                return Value;
            }

            Value = ((Value & 0xffff0000ffff0000) >> 16) | ((Value & 0x0000ffff0000ffff) << 16);

            if (Size == RevSize.Rev32)
            {
                return Value;
            }

            Value = ((Value & 0xffffffff00000000) >> 32) | ((Value & 0x00000000ffffffff) << 32);

            if (Size == RevSize.Rev64)
            {
                return Value;
            }

            throw new ArgumentException(nameof(Size));
        }
#endregion

#region "MultiplyHigh"
        public static long SMulHi128(long LHS, long RHS)
        {
            long Result = (long)UMulHi128((ulong)LHS, (ulong)RHS);
            if (LHS < 0) Result -= RHS;
            if (RHS < 0) Result -= LHS;

            return Result;
        }

        public static ulong UMulHi128(ulong LHS, ulong RHS)
        {
            //long multiplication
            //multiply 32 bits at a time in 64 bit, the result is what's carried over 64 bits.
            ulong LHigh = LHS >> 32;
            ulong LLow = LHS & 0xFFFFFFFF;
            ulong RHigh = RHS >> 32;
            ulong RLow = RHS & 0xFFFFFFFF;
            ulong Z2 = LLow * RLow;
            ulong T = LHigh * RLow + (Z2 >> 32);
            ulong Z1 = T & 0xFFFFFFFF;
            ulong Z0 = T >> 32;
            Z1 += LLow * RHigh;

            return LHigh * RHigh + Z0 + (Z1 >> 32);
        }
#endregion
    }
}
