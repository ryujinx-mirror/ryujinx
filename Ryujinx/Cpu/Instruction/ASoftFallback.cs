using ChocolArm64.State;
using ChocolArm64.Translation;
using System;

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
            Value = ((Value & 0xff00ff00ff00ff00) >>  8) | ((Value & 0x00ff00ff00ff00ff) <<  8);

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

        public static int SatSingleToInt32(float Value, int FBits)
        {
            if (FBits != 0) Value *= MathF.Pow(2, FBits);

            return Value > int.MaxValue ? int.MaxValue :
                   Value < int.MinValue ? int.MinValue : (int)Value;
        }

        public static long SatSingleToInt64(float Value, int FBits)
        {
            if (FBits != 0) Value *= MathF.Pow(2, FBits);

            return Value > long.MaxValue ? long.MaxValue :
                   Value < long.MinValue ? long.MinValue : (long)Value;
        }

        public static uint SatSingleToUInt32(float Value, int FBits)
        {
            if (FBits != 0) Value *= MathF.Pow(2, FBits);

            return Value > uint.MaxValue ? uint.MaxValue :
                   Value < uint.MinValue ? uint.MinValue : (uint)Value;
        }

        public static ulong SatSingleToUInt64(float Value, int FBits)
        {
            if (FBits != 0) Value *= MathF.Pow(2, FBits);

            return Value > ulong.MaxValue ? ulong.MaxValue :
                   Value < ulong.MinValue ? ulong.MinValue : (ulong)Value;
        }

        public static int SatDoubleToInt32(double Value, int FBits)
        {
            if (FBits != 0) Value *= Math.Pow(2, FBits);

            return Value > int.MaxValue ? int.MaxValue :
                   Value < int.MinValue ? int.MinValue : (int)Value;
        }

        public static long SatDoubleToInt64(double Value, int FBits)
        {
            if (FBits != 0) Value *= Math.Pow(2, FBits);

            return Value > long.MaxValue ? long.MaxValue :
                   Value < long.MinValue ? long.MinValue : (long)Value;
        }

        public static uint SatDoubleToUInt32(double Value, int FBits)
        {
            if (FBits != 0) Value *= Math.Pow(2, FBits);

            return Value > uint.MaxValue ? uint.MaxValue :
                   Value < uint.MinValue ? uint.MinValue : (uint)Value;
        }

        public static ulong SatDoubleToUInt64(double Value, int FBits)
        {
            if (FBits != 0) Value *= Math.Pow(2, FBits);

            return Value > ulong.MaxValue ? ulong.MaxValue :
                   Value < ulong.MinValue ? ulong.MinValue : (ulong)Value;
        }

        public static float Int32ToSingle(int Value, int FBits)
        {
            float ValueF = Value;

            if (FBits != 0) ValueF *= 1 / MathF.Pow(2, FBits);

            return ValueF;
        }

        public static float Int64ToSingle(long Value, int FBits)
        {
            float ValueF = Value;

            if (FBits != 0) ValueF *= 1 / MathF.Pow(2, FBits);

            return ValueF;
        }

        public static float UInt32ToSingle(uint Value, int FBits)
        {
            float ValueF = Value;

            if (FBits != 0) ValueF *= 1 / MathF.Pow(2, FBits);

            return ValueF;
        }

        public static float UInt64ToSingle(ulong Value, int FBits)
        {
            float ValueF = Value;

            if (FBits != 0) ValueF *= 1 / MathF.Pow(2, FBits);

            return ValueF;
        }

        public static double Int32ToDouble(int Value, int FBits)
        {
            double ValueF = Value;

            if (FBits != 0) ValueF *= 1 / Math.Pow(2, FBits);

            return ValueF;
        }

        public static double Int64ToDouble(long Value, int FBits)
        {
            double ValueF = Value;

            if (FBits != 0) ValueF *= 1 / Math.Pow(2, FBits);

            return ValueF;
        }

        public static double UInt32ToDouble(uint Value, int FBits)
        {
            double ValueF = Value;

            if (FBits != 0) ValueF *= 1 / Math.Pow(2, FBits);

            return ValueF;
        }

        public static double UInt64ToDouble(ulong Value, int FBits)
        {
            double ValueF = Value;

            if (FBits != 0) ValueF *= 1 / Math.Pow(2, FBits);

            return ValueF;
        }

        public static ulong SMulHi128(ulong LHS, ulong RHS)
        {
            long LLo = (uint)(LHS >>  0);
            long LHi =  (int)(LHS >> 32);
            long RLo = (uint)(RHS >>  0);
            long RHi =  (int)(RHS >> 32);

            long LHiRHi = LHi * RHi;
            long LHiRLo = LHi * RLo;
            long LLoRHi = LLo * RHi;
            long LLoRLo = LLo * RLo;

            long Carry = ((uint)LHiRLo + ((uint)LLoRHi + (LLoRLo >> 32))) >> 32;

            long ResHi = LHiRHi + (LHiRLo >> 32) + (LLoRHi >> 32) + Carry;

            return (ulong)ResHi;
        }

        public static ulong UMulHi128(ulong LHS, ulong RHS)
        {
            ulong LLo = (uint)(LHS >>  0);
            ulong LHi = (uint)(LHS >> 32);
            ulong RLo = (uint)(RHS >>  0);
            ulong RHi = (uint)(RHS >> 32);

            ulong LHiRHi = LHi * RHi;
            ulong LHiRLo = LHi * RLo;
            ulong LLoRHi = LLo * RHi;
            ulong LLoRLo = LLo * RLo;

            ulong Carry = ((uint)LHiRLo + ((uint)LLoRHi + (LLoRLo >> 32))) >> 32;

            ulong ResHi = LHiRHi + (LHiRLo >> 32) + (LLoRHi >> 32) + Carry;

            return ResHi;
        }    

        public static AVec Addp_S(AVec Vector, int Size)
        {
            ulong Low  = ExtractVec(Vector, 0, Size);
            ulong High = ExtractVec(Vector, 1, Size);

            return InsertVec(new AVec(), 0, Size, Low + High);
        }

        public static int CountSetBits8(byte Value)
        {
            return (Value >> 0) & 1 + (Value >> 1) & 1 +
                   (Value >> 2) & 1 + (Value >> 3) & 1 +
                   (Value >> 4) & 1 + (Value >> 5) & 1 +
                   (Value >> 6) & 1 + (Value >> 7);
        }

        public static AVec Dup_Gp64(ulong Value, int Size)
        {
            return Dup_Gp(Value, Size, 8);
        }

        public static AVec Dup_Gp128(ulong Value, int Size)
        {
            return Dup_Gp(Value, Size, 16);
        }

        private static AVec Dup_Gp(ulong Value, int Size, int Bytes)
        {
            AVec Res = new AVec();

            for (int Index = 0; Index < (Bytes >> Size); Index++)
            {
                Res = InsertVec(Res, Index, Size, Value);
            }

            return Res;
        }

        public static AVec Dup_S(AVec Vector, int Elem, int Size)
        {
            return InsertVec(new AVec(), 0, Size, ExtractVec(Vector, Elem, Size));
        }

        public static AVec Fmov_S(ulong Value, int Elem, int Size)
        {
            return InsertVec(new AVec(), Elem, Size, Value);
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
                Table[Index * 16 + Index2] = (byte)ExtractVec(Tb[Index], Index2, 0);
            }

            for (int Index = 0; Index < Bytes; Index++)
            {
                byte TblIdx = (byte)ExtractVec(Vector, Index, 0);

                if (TblIdx < Table.Length)
                {
                    Res = InsertVec(Res, Index, 0, Table[TblIdx]);
                }
            }

            return Res;
        }

        public static ulong ExtractVec(AVec Vector, int Index, int Size)
        {
            switch (Size)
            {
                case 0: return Vector.ExtractByte(Index);
                case 1: return Vector.ExtractUInt16(Index);
                case 2: return Vector.ExtractUInt32(Index);
                case 3: return Vector.ExtractUInt64(Index);
            }

            throw new ArgumentOutOfRangeException(nameof(Size));
        }

        public static long ExtractSVec(AVec Vector, int Index, int Size)
        {
            switch (Size)
            {
                case 0: return (sbyte)Vector.ExtractByte(Index);
                case 1: return (short)Vector.ExtractUInt16(Index);
                case 2: return (int)Vector.ExtractUInt32(Index);
                case 3: return (long)Vector.ExtractUInt64(Index);
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

        public static AVec InsertVec(AVec Vector, int Index, int Size, ulong Value)
        {
            switch (Size)
            {
                case 0: return AVec.InsertByte(Vector, Index, (byte)Value);
                case 1: return AVec.InsertUInt16(Vector, Index, (ushort)Value);
                case 2: return AVec.InsertUInt32(Vector, Index, (uint)Value);
                case 3: return AVec.InsertUInt64(Vector, Index, (ulong)Value);
            }

            throw new ArgumentOutOfRangeException(nameof(Size));
        }

        public static AVec InsertSVec(AVec Vector, int Index, int Size, long Value)
        {
            switch (Size)
            {
                case 0: return AVec.InsertByte(Vector, Index, (byte)Value);
                case 1: return AVec.InsertUInt16(Vector, Index, (ushort)Value);
                case 2: return AVec.InsertUInt32(Vector, Index, (uint)Value);
                case 3: return AVec.InsertUInt64(Vector, Index, (ulong)Value);
            }

            throw new ArgumentOutOfRangeException(nameof(Size));
        }
    }
}