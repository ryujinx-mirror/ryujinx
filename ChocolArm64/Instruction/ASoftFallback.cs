using ChocolArm64.State;
using ChocolArm64.Translation;
using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace ChocolArm64.Instruction
{
    static class ASoftFallback
    {
        public static void EmitCall(AILEmitterCtx Context, string Name64, string Name128)
        {
            bool IsSimd64 = Context.CurrOp.RegisterSize == ARegisterSize.SIMD64;

            Context.EmitCall(typeof(ASoftFallback), IsSimd64 ? Name64 : Name128);
        }

        public static void EmitCall(AILEmitterCtx Context, string MthdName)
        {
            Context.EmitCall(typeof(ASoftFallback), MthdName);
        }

        public static uint  CountLeadingZeros32(uint Value)  => (uint)CountLeadingZeros(Value, 32);
        public static ulong CountLeadingZeros64(ulong Value) => (ulong)CountLeadingZeros(Value, 64);

        private static ulong CountLeadingZeros(ulong Value, int Size)
        {
            int HighBit = Size - 1;

            for (int Bit = HighBit; Bit >= 0; Bit--)
            {
                if (((Value >> Bit) & 1) != 0)
                {
                    return (ulong)(HighBit - Bit);
                }
            }

            return (ulong)Size;
        }

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
            Crc = Crc32(Crc, Poly, (byte)(Val >> 0));
            Crc = Crc32(Crc, Poly, (byte)(Val >> 8));
            Crc = Crc32(Crc, Poly, (byte)(Val >> 16));
            Crc = Crc32(Crc, Poly, (byte)(Val >> 24));

            return Crc;
        }

        private static uint Crc32x(uint Crc, uint Poly, ulong Val)
        {
            Crc = Crc32(Crc, Poly, (byte)(Val >> 0));
            Crc = Crc32(Crc, Poly, (byte)(Val >> 8));
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
            Value = ((Value & 0xaaaaaaaaaaaaaaaa) >>  1) | ((Value & 0x5555555555555555) <<  1);
            Value = ((Value & 0xcccccccccccccccc) >>  2) | ((Value & 0x3333333333333333) <<  2);
            Value = ((Value & 0xf0f0f0f0f0f0f0f0) >>  4) | ((Value & 0x0f0f0f0f0f0f0f0f) <<  4);
            Value = ((Value & 0xff00ff00ff00ff00) >>  8) | ((Value & 0x00ff00ff00ff00ff) <<  8);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SatF32ToS32(float Value)
        {
            if (float.IsNaN(Value)) return 0;

            return Value > int.MaxValue ? int.MaxValue :
                   Value < int.MinValue ? int.MinValue : (int)Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long SatF32ToS64(float Value)
        {
            if (float.IsNaN(Value)) return 0;

            return Value > long.MaxValue ? long.MaxValue :
                   Value < long.MinValue ? long.MinValue : (long)Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint SatF32ToU32(float Value)
        {
            if (float.IsNaN(Value)) return 0;

            return Value > uint.MaxValue ? uint.MaxValue :
                   Value < uint.MinValue ? uint.MinValue : (uint)Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong SatF32ToU64(float Value)
        {
            if (float.IsNaN(Value)) return 0;

            return Value > ulong.MaxValue ? ulong.MaxValue :
                   Value < ulong.MinValue ? ulong.MinValue : (ulong)Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SatF64ToS32(double Value)
        {
            if (double.IsNaN(Value)) return 0;

            return Value > int.MaxValue ? int.MaxValue :
                   Value < int.MinValue ? int.MinValue : (int)Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long SatF64ToS64(double Value)
        {
            if (double.IsNaN(Value)) return 0;

            return Value > long.MaxValue ? long.MaxValue :
                   Value < long.MinValue ? long.MinValue : (long)Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint SatF64ToU32(double Value)
        {
            if (double.IsNaN(Value)) return 0;

            return Value > uint.MaxValue ? uint.MaxValue :
                   Value < uint.MinValue ? uint.MinValue : (uint)Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong SatF64ToU64(double Value)
        {
            if (double.IsNaN(Value)) return 0;

            return Value > ulong.MaxValue ? ulong.MaxValue :
                   Value < ulong.MinValue ? ulong.MinValue : (ulong)Value;
        }

        public static long SMulHi128(long LHS, long RHS)
        {
            return (long)(BigInteger.Multiply(LHS, RHS) >> 64);
        }

        public static ulong UMulHi128(ulong LHS, ulong RHS)
        {
            return (ulong)(BigInteger.Multiply(LHS, RHS) >> 64);
        }

        public static int CountSetBits8(byte Value)
        {
            return (Value >> 0) & 1 + (Value >> 1) & 1 +
                   (Value >> 2) & 1 + (Value >> 3) & 1 +
                   (Value >> 4) & 1 + (Value >> 5) & 1 +
                   (Value >> 6) & 1 + (Value >> 7);
        }

        public static float RoundF(float Value, int Fpcr)
        {
            switch ((ARoundMode)((Fpcr >> 22) & 3))
            {
                case ARoundMode.ToNearest:            return MathF.Round   (Value);
                case ARoundMode.TowardsPlusInfinity:  return MathF.Ceiling (Value);
                case ARoundMode.TowardsMinusInfinity: return MathF.Floor   (Value);
                case ARoundMode.TowardsZero:          return MathF.Truncate(Value);
            }

            throw new InvalidOperationException();
        }

        public static double Round(double Value, int Fpcr)
        {
            switch ((ARoundMode)((Fpcr >> 22) & 3))
            {
                case ARoundMode.ToNearest:            return Math.Round   (Value);
                case ARoundMode.TowardsPlusInfinity:  return Math.Ceiling (Value);
                case ARoundMode.TowardsMinusInfinity: return Math.Floor   (Value);
                case ARoundMode.TowardsZero:          return Math.Truncate(Value);
            }

            throw new InvalidOperationException();
        }

        public static AVec Tbl1_V64(AVec Vector, AVec Tb0)
        {
            return Tbl(Vector, 8, Tb0);
        }

        public static AVec Tbl1_V128(AVec Vector, AVec Tb0)
        {
            return Tbl(Vector, 16, Tb0);
        }

        public static AVec Tbl2_V64(AVec Vector, AVec Tb0, AVec Tb1)
        {
            return Tbl(Vector, 8, Tb0, Tb1);
        }

        public static AVec Tbl2_V128(AVec Vector, AVec Tb0, AVec Tb1)
        {
            return Tbl(Vector, 16, Tb0, Tb1);
        }

        public static AVec Tbl3_V64(AVec Vector, AVec Tb0, AVec Tb1, AVec Tb2)
        {
            return Tbl(Vector, 8, Tb0, Tb1, Tb2);
        }

        public static AVec Tbl3_V128(AVec Vector, AVec Tb0, AVec Tb1, AVec Tb2)
        {
            return Tbl(Vector, 16, Tb0, Tb1, Tb2);
        }

        public static AVec Tbl4_V64(AVec Vector, AVec Tb0, AVec Tb1, AVec Tb2, AVec Tb3)
        {
            return Tbl(Vector, 8, Tb0, Tb1, Tb2, Tb3);
        }

        public static AVec Tbl4_V128(AVec Vector, AVec Tb0, AVec Tb1, AVec Tb2, AVec Tb3)
        {
            return Tbl(Vector, 16, Tb0, Tb1, Tb2, Tb3);
        }

        private static AVec Tbl(AVec Vector, int Bytes, params AVec[] Tb)
        {
            AVec Res = new AVec();

            byte[] Table = new byte[Tb.Length * 16];

            for (int Index  = 0; Index  < Tb.Length; Index++)
            for (int Index2 = 0; Index2 < 16;        Index2++)
            {
                Table[Index * 16 + Index2] = (byte)VectorExtractIntZx(Tb[Index], Index2, 0);
            }

            for (int Index = 0; Index < Bytes; Index++)
            {
                byte TblIdx = (byte)VectorExtractIntZx(Vector, Index, 0);

                if (TblIdx < Table.Length)
                {
                    Res = VectorInsertInt(Table[TblIdx], Res, Index, 0);
                }
            }

            return Res;
        }

        public static ulong VectorExtractIntZx(AVec Vector, int Index, int Size)
        {
            switch (Size)
            {
                case 0: return Vector.ExtractByte  (Index);
                case 1: return Vector.ExtractUInt16(Index);
                case 2: return Vector.ExtractUInt32(Index);
                case 3: return Vector.ExtractUInt64(Index);
            }

            throw new ArgumentOutOfRangeException(nameof(Size));
        }

        public static long VectorExtractIntSx(AVec Vector, int Index, int Size)
        {
            switch (Size)
            {
                case 0: return (sbyte)Vector.ExtractByte  (Index);
                case 1: return (short)Vector.ExtractUInt16(Index);
                case 2: return   (int)Vector.ExtractUInt32(Index);
                case 3: return  (long)Vector.ExtractUInt64(Index);
            }

            throw new ArgumentOutOfRangeException(nameof(Size));
        }

        public static float VectorExtractSingle(AVec Vector, int Index)
        {
            return Vector.ExtractSingle(Index);
        }

        public static double VectorExtractDouble(AVec Vector, int Index)
        {
            return Vector.ExtractDouble(Index);
        }

        public static AVec VectorInsertSingle(float Value, AVec Vector, int Index)
        {
            return AVec.InsertSingle(Vector, Index, Value);
        }

        public static AVec VectorInsertDouble(double Value, AVec Vector, int Index)
        {
            return AVec.InsertDouble(Vector, Index, Value);
        }

        public static AVec VectorInsertInt(ulong Value, AVec Vector, int Index, int Size)
        {
            switch (Size)
            {
                case 0: return AVec.InsertByte  (Vector, Index,   (byte)Value);
                case 1: return AVec.InsertUInt16(Vector, Index, (ushort)Value);
                case 2: return AVec.InsertUInt32(Vector, Index,   (uint)Value);
                case 3: return AVec.InsertUInt64(Vector, Index,  (ulong)Value);
            }

            throw new ArgumentOutOfRangeException(nameof(Size));
        }
    }
}