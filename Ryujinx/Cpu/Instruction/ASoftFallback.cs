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

        public static int SatDoubleToInt32(double Value, int FBits = 0)
        {
            if (FBits != 0) Value *= Math.Pow(2, FBits);

            return Value > int.MaxValue ? int.MaxValue :
                   Value < int.MinValue ? int.MinValue : (int)Value;
        }

        public static long SatDoubleToInt64(double Value, int FBits = 0)
        {
            if (FBits != 0) Value *= Math.Pow(2, FBits);

            return Value > long.MaxValue ? long.MaxValue :
                   Value < long.MinValue ? long.MinValue : (long)Value;
        }

        public static uint SatDoubleToUInt32(double Value, int FBits = 0)
        {
            if (FBits != 0) Value *= Math.Pow(2, FBits);

            return Value > uint.MaxValue ? uint.MaxValue :
                   Value < uint.MinValue ? uint.MinValue : (uint)Value;
        }

        public static ulong SatDoubleToUInt64(double Value, int FBits = 0)
        {
            if (FBits != 0) Value *= Math.Pow(2, FBits);

            return Value > ulong.MaxValue ? ulong.MaxValue :
                   Value < ulong.MinValue ? ulong.MinValue : (ulong)Value;
        }

        public static int SatSingleToInt32(float Value, int FBits = 0)
        {
            if (FBits != 0) Value *= MathF.Pow(2, FBits);

            return Value > int.MaxValue ? int.MaxValue :
                   Value < int.MinValue ? int.MinValue : (int)Value;
        }

        public static long SatSingleToInt64(float Value, int FBits = 0)
        {
            if (FBits != 0) Value *= MathF.Pow(2, FBits);

            return Value > long.MaxValue ? long.MaxValue :
                   Value < long.MinValue ? long.MinValue : (long)Value;
        }

        public static uint SatSingleToUInt32(float Value, int FBits = 0)
        {
            if (FBits != 0) Value *= MathF.Pow(2, FBits);

            return Value > uint.MaxValue ? uint.MaxValue :
                   Value < uint.MinValue ? uint.MinValue : (uint)Value;
        }

        public static ulong SatSingleToUInt64(float Value, int FBits = 0)
        {
            if (FBits != 0) Value *= MathF.Pow(2, FBits);

            return Value > ulong.MaxValue ? ulong.MaxValue :
                   Value < ulong.MinValue ? ulong.MinValue : (ulong)Value;
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

        public static AVec Addp64(AVec LHS, AVec RHS, int Size)
        {
            return Addp(LHS, RHS, Size, 8);
        }

        public static AVec Addp128(AVec LHS, AVec RHS, int Size)
        {
            return Addp(LHS, RHS, Size, 16);
        }

        private static AVec Addp(AVec LHS, AVec RHS, int Size, int Bytes)
        {
            AVec Res = new AVec();

            int Elems = Bytes >> Size;
            int Half  = Elems >> 1;

            for (int Index = 0; Index < Elems; Index++)
            {
                int Elem = (Index & (Half - 1)) << 1;

                ulong L = Index < Half
                    ? ExtractVec(LHS, Elem + 0, Size)
                    : ExtractVec(RHS, Elem + 0, Size);
                
                ulong R = Index < Half
                    ? ExtractVec(LHS, Elem + 1, Size)
                    : ExtractVec(RHS, Elem + 1, Size);

                Res = InsertVec(Res, Index, Size, L + R);
            }

            return Res;
        }

        public static AVec Bic_Vi64(AVec Res, ulong Imm, int Size)
        {
            return Bic_Vi(Res, Imm, Size, 8);
        }

        public static AVec Bic_Vi128(AVec Res, ulong Imm, int Size)
        {
            return Bic_Vi(Res, Imm, Size, 16);
        }

        private static AVec Bic_Vi(AVec Res, ulong Imm, int Size, int Bytes)
        {
            int Elems = Bytes >> Size;

            for (int Index = 0; Index < Elems; Index++)
            {
                ulong Value = ExtractVec(Res, Index, Size);

                Res = InsertVec(Res, Index, Size, Value & ~Imm);
            }

            return Res;
        }

        public static AVec Cnt64(AVec Vector)
        {
            AVec Res = new AVec();

            Res.B0 = (byte)CountSetBits8(Vector.B0);
            Res.B1 = (byte)CountSetBits8(Vector.B1);
            Res.B2 = (byte)CountSetBits8(Vector.B2);
            Res.B3 = (byte)CountSetBits8(Vector.B3);
            Res.B4 = (byte)CountSetBits8(Vector.B4);
            Res.B5 = (byte)CountSetBits8(Vector.B5);
            Res.B6 = (byte)CountSetBits8(Vector.B6);
            Res.B7 = (byte)CountSetBits8(Vector.B7);

            return Res;
        }

        public static AVec Cnt128(AVec Vector)
        {
            AVec Res = new AVec();

            Res.B0  = (byte)CountSetBits8(Vector.B0);
            Res.B1  = (byte)CountSetBits8(Vector.B1);
            Res.B2  = (byte)CountSetBits8(Vector.B2);
            Res.B3  = (byte)CountSetBits8(Vector.B3);
            Res.B4  = (byte)CountSetBits8(Vector.B4);
            Res.B5  = (byte)CountSetBits8(Vector.B5);
            Res.B6  = (byte)CountSetBits8(Vector.B6);
            Res.B7  = (byte)CountSetBits8(Vector.B7);
            Res.B8  = (byte)CountSetBits8(Vector.B8);
            Res.B9  = (byte)CountSetBits8(Vector.B9);
            Res.B10 = (byte)CountSetBits8(Vector.B10);
            Res.B11 = (byte)CountSetBits8(Vector.B11);
            Res.B12 = (byte)CountSetBits8(Vector.B12);
            Res.B13 = (byte)CountSetBits8(Vector.B13);
            Res.B14 = (byte)CountSetBits8(Vector.B14);
            Res.B15 = (byte)CountSetBits8(Vector.B15);

            return Res;
        }

        private static int CountSetBits8(byte Value)
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

        public static AVec Dup_V64(AVec Vector, int Elem, int Size)
        {
            return Dup_V(Vector, Elem, Size, 8);
        }

        public static AVec Dup_V128(AVec Vector, int Elem, int Size)
        {
            return Dup_V(Vector, Elem, Size, 16);
        }

        private static AVec Dup_V(AVec Vector, int Elem, int Size, int Bytes)
        {
            AVec Res = new AVec();

            ulong Value = ExtractVec(Vector, Elem, Size);

            for (Elem = 0; Elem < (Bytes >> Size); Elem++)
            {
                Res = InsertVec(Res, Elem, Size, Value);
            }

            return Res;
        }

        public static AVec Fadd64(AVec LHS, AVec RHS, int Size)
        {
            return Fadd(LHS, RHS, Size, 2);
        }

        public static AVec Fadd128(AVec LHS, AVec RHS, int Size)
        {
            return Fadd(LHS, RHS, Size, 4);
        }

        private static AVec Fadd(AVec LHS, AVec RHS, int Size, int Bytes)
        {
            AVec Res = new AVec();

            int Elems = Bytes >> Size;

            if (Size == 0)
            {
                for (int Index = 0; Index < Elems; Index++)
                {
                    float L = LHS.ExtractSingle(Index);
                    float R = RHS.ExtractSingle(Index);

                    Res = AVec.InsertSingle(Res, Index, L + R);
                }
            }
            else
            {
                for (int Index = 0; Index < Elems; Index++)
                {
                    double L = LHS.ExtractDouble(Index);
                    double R = RHS.ExtractDouble(Index);

                    Res = AVec.InsertDouble(Res, Index, L + R);
                }
            }

            return Res;
        }

        public static AVec Fcvtzs_V64(AVec Vector, int Size)
        {
            return Fcvtzs_V(Vector, Size, 2);
        }

        public static AVec Fcvtzs_V128(AVec Vector, int Size)
        {
            return Fcvtzs_V(Vector, Size, 4);
        }

        private static AVec Fcvtzs_V(AVec Vector, int Size, int Bytes)
        {
            AVec Res = new AVec();

            int Elems = Bytes >> Size;

            if (Size == 0)
            {
                for (int Index = 0; Index < Elems; Index++)
                {
                    float Value = Vector.ExtractSingle(Index);

                    Res = InsertSVec(Res, Index, Size + 2, SatSingleToInt32(Value));
                }
            }
            else
            {
                for (int Index = 0; Index < Elems; Index++)
                {
                    double Value = Vector.ExtractDouble(Index);

                    Res = InsertSVec(Res, Index, Size + 2, SatDoubleToInt64(Value));
                }
            }

            return Res;
        }

        public static AVec Fcvtzu_V_64(AVec Vector, int FBits, int Size)
        {
            return Fcvtzu_V(Vector, FBits, Size, 2);
        }

        public static AVec Fcvtzu_V_128(AVec Vector, int FBits, int Size)
        {
            return Fcvtzu_V(Vector, FBits, Size, 4);
        }

        private static AVec Fcvtzu_V(AVec Vector, int FBits, int Size, int Bytes)
        {
            AVec Res = new AVec();

            int Elems = Bytes >> Size;

            if (Size == 0)
            {
                for (int Index = 0; Index < Elems; Index++)
                {
                    float Value = Vector.ExtractSingle(Index);

                    Res = InsertVec(Res, Index, Size + 2, SatSingleToUInt32(Value, FBits));
                }
            }
            else
            {
                for (int Index = 0; Index < Elems; Index++)
                {
                    double Value = Vector.ExtractDouble(Index);

                    Res = InsertVec(Res, Index, Size + 2, SatDoubleToUInt64(Value, FBits));
                }
            }

            return Res;
        }

        public static AVec Fmla64(AVec Res, AVec LHS, AVec RHS, int Size)
        {
            return Fmla(Res, LHS, RHS, Size, 2);
        }

        public static AVec Fmla128(AVec Res, AVec LHS, AVec RHS, int Size)
        {
            return Fmla(Res, LHS, RHS, Size, 4);
        }

        private static AVec Fmla(AVec Res, AVec LHS, AVec RHS, int Size, int Bytes)
        {
            int Elems = Bytes >> Size;

            if (Size == 0)
            {
                for (int Index = 0; Index < Elems; Index++)
                {
                    float L      = LHS.ExtractSingle(Index);
                    float R      = RHS.ExtractSingle(Index);
                    float Addend = Res.ExtractSingle(Index);

                    Res = AVec.InsertSingle(Res, Index, Addend + L * R);
                }
            }
            else
            {
                for (int Index = 0; Index < Elems; Index++)
                {
                    double L      = LHS.ExtractDouble(Index);
                    double R      = RHS.ExtractDouble(Index);
                    double Addend = Res.ExtractDouble(Index);

                    Res = AVec.InsertDouble(Res, Index, Addend + L * R);
                }
            }

            return Res;
        }

        public static AVec Fmla_Ve64(AVec Res, AVec LHS, AVec RHS, int SIdx, int Size)
        {
            return Fmla_Ve(Res, LHS, RHS, SIdx, Size, 2);
        }

        public static AVec Fmla_Ve128(AVec Res, AVec LHS, AVec RHS, int SIdx, int Size)
        {
            return Fmla_Ve(Res, LHS, RHS, SIdx, Size, 4);
        }

        private static AVec Fmla_Ve(AVec Res, AVec LHS, AVec RHS, int SIdx, int Size, int Bytes)
        {
            int Elems = Bytes >> Size;

            if (Size == 0)
            {
                float R = RHS.ExtractSingle(SIdx);

                for (int Index = 0; Index < Elems; Index++)
                {
                    float L      = LHS.ExtractSingle(Index);
                    float Addend = Res.ExtractSingle(Index);

                    Res = AVec.InsertSingle(Res, Index, Addend + L * R);
                }
            }
            else
            {
                double R = RHS.ExtractDouble(SIdx);

                for (int Index = 0; Index < Elems; Index++)
                {
                    double L      = LHS.ExtractDouble(Index);
                    double Addend = Res.ExtractDouble(Index);

                    Res = AVec.InsertDouble(Res, Index, Addend + L * R);
                }
            }

            return Res;
        }

        public static AVec Fmov_S(ulong Value, int Elem, int Size)
        {
            return InsertVec(new AVec(), Elem, Size, Value);
        }

        public static AVec Fmul64(AVec LHS, AVec RHS, int Size)
        {
            return Fmul(LHS, RHS, Size, 2);
        }

        public static AVec Fmul128(AVec LHS, AVec RHS, int Size)
        {
            return Fmul(LHS, RHS, Size, 4);
        }

        private static AVec Fmul(AVec LHS, AVec RHS, int Size, int Bytes)
        {
            AVec Res = new AVec();

            int Elems = Bytes >> Size;

            if (Size == 0)
            {
                for (int Index = 0; Index < Elems; Index++)
                {
                    float L = LHS.ExtractSingle(Index);
                    float R = RHS.ExtractSingle(Index);

                    Res = AVec.InsertSingle(Res, Index, L * R);
                }
            }
            else
            {
                for (int Index = 0; Index < Elems; Index++)
                {
                    double L = LHS.ExtractDouble(Index);
                    double R = RHS.ExtractDouble(Index);

                    Res = AVec.InsertDouble(Res, Index, L * R);
                }
            }

            return Res;
        }

        public static AVec Fmul_Ve64(AVec LHS, AVec RHS, int SIdx, int Size)
        {
            return Fmul_Ve(LHS, RHS, SIdx, Size, 2);
        }

        public static AVec Fmul_Ve128(AVec LHS, AVec RHS, int SIdx, int Size)
        {
            return Fmul_Ve(LHS, RHS, SIdx, Size, 4);
        }

        private static AVec Fmul_Ve(AVec LHS, AVec RHS, int SIdx, int Size, int Bytes)
        {
            AVec Res = new AVec();

            int Elems = Bytes >> Size;

            if (Size == 0)
            {
                float R = RHS.ExtractSingle(SIdx);

                for (int Index = 0; Index < Elems; Index++)
                {
                    float L = LHS.ExtractSingle(Index);

                    Res = AVec.InsertSingle(Res, Index, L * R);
                }
            }
            else
            {
                double R = RHS.ExtractDouble(SIdx);

                for (int Index = 0; Index < Elems; Index++)
                {
                    double L = LHS.ExtractDouble(Index);

                    Res = AVec.InsertDouble(Res, Index, L * R);
                }
            }

            return Res;
        }

        public static AVec Fsub64(AVec LHS, AVec RHS, int Size)
        {
            return Fsub(LHS, RHS, Size, 2);
        }

        public static AVec Fsub128(AVec LHS, AVec RHS, int Size)
        {
            return Fsub(LHS, RHS, Size, 4);
        }

        private static AVec Fsub(AVec LHS, AVec RHS, int Size, int Bytes)
        {
            AVec Res = new AVec();

            int Elems = Bytes >> Size;

            if (Size == 0)
            {
                for (int Index = 0; Index < Elems; Index++)
                {
                    float L = LHS.ExtractSingle(Index);
                    float R = RHS.ExtractSingle(Index);

                    Res = AVec.InsertSingle(Res, Index, L - R);
                }
            }
            else
            {
                for (int Index = 0; Index < Elems; Index++)
                {
                    double L = LHS.ExtractDouble(Index);
                    double R = RHS.ExtractDouble(Index);

                    Res = AVec.InsertDouble(Res, Index, L - R);
                }
            }

            return Res;
        }

        public static AVec Ins_Gp(AVec Res, ulong Value, int Elem, int Size)
        {
            return InsertVec(Res, Elem, Size, Value);
        }

        public static AVec Ins_V(AVec Res, AVec Value, int Src, int Dst, int Size)
        {
            return InsertVec(Res, Dst, Size, ExtractVec(Value, Src, Size));;
        }

        public static AVec Orr_Vi64(AVec Res, ulong Imm, int Size)
        {
            return Orr_Vi(Res, Imm, Size, 8);
        }

        public static AVec Orr_Vi128(AVec Res, ulong Imm, int Size)
        {
            return Orr_Vi(Res, Imm, Size, 16);
        }

        private static AVec Orr_Vi(AVec Res, ulong Imm, int Size, int Bytes)
        {
            int Elems = Bytes >> Size;

            for (int Index = 0; Index < Elems; Index++)
            {
                ulong Value = ExtractVec(Res, Index, Size);

                Res = InsertVec(Res, Index, Size, Value | Imm);
            }

            return Res;
        }

        public static AVec Saddw(AVec LHS, AVec RHS, int Size)
        {
            return Saddw_(LHS, RHS, Size, false);
        }

        public static AVec Saddw2(AVec LHS, AVec RHS, int Size)
        {
            return Saddw_(LHS, RHS, Size, true);
        }

        private static AVec Saddw_(AVec LHS, AVec RHS, int Size, bool High)
        {
            AVec Res = new AVec();

            int Elems = 8 >> Size;
            int Part  = High ? Elems : 0;

            for (int Index = 0; Index < Elems; Index++)
            {
                long L = ExtractSVec(LHS, Index,        Size + 1);
                long R = ExtractSVec(RHS, Index + Part, Size);

                Res = InsertSVec(Res, Index, Size + 1, L + R);
            }

            return Res;
        }

        public static AVec Scvtf_V64(AVec Vector, int Size)
        {
            return Scvtf_V(Vector, Size, 2);
        }

        public static AVec Scvtf_V128(AVec Vector, int Size)
        {
            return Scvtf_V(Vector, Size, 4);
        }

        private static AVec Scvtf_V(AVec Vector, int Size, int Bytes)
        {
            AVec Res = new AVec();

            int Elems = Bytes >> Size;

            if (Size == 0)
            {
                for (int Index = 0; Index < Elems; Index++)
                {
                    int Value = (int)ExtractSVec(Vector, Index, Size + 2);

                    Res = AVec.InsertSingle(Res, Index, Value);
                }
            }
            else
            {
                for (int Index = 0; Index < Elems; Index++)
                {
                    long Value = ExtractSVec(Vector, Index, Size + 2);

                    Res = AVec.InsertDouble(Res, Index, Value);
                }
            }

            return Res;
        }

        public static AVec Shl64(AVec Vector, int Shift, int Size)
        {
            return Shl(Vector, Shift, Size, 8);
        }

        public static AVec Shl128(AVec Vector, int Shift, int Size)
        {
            return Shl(Vector, Shift, Size, 16);
        }

        private static AVec Shl(AVec Vector, int Shift, int Size, int Bytes)
        {
            AVec Res = new AVec();

            int Elems = Bytes >> Size;

            for (int Index = 0; Index < Elems; Index++)
            {
                ulong Value = ExtractVec(Vector, Index, Size);

                Res = InsertVec(Res, Index, Size, Value << Shift);
            }

            return Res;
        }

        public static AVec Sshll(AVec Vector, int Shift, int Size)
        {
            return Sshll_(Vector, Shift, Size, false);
        }

        public static AVec Sshll2(AVec Vector, int Shift, int Size)
        {
            return Sshll_(Vector, Shift, Size, true);
        }

        private static AVec Sshll_(AVec Vector, int Shift, int Size, bool High)
        {
            AVec Res = new AVec();

            int Elems = 8 >> Size;
            int Part  = High ? Elems : 0;

            for (int Index = 0; Index < Elems; Index++)
            {
                long Value = ExtractSVec(Vector, Index + Part, Size);

                Res = InsertSVec(Res, Index, Size + 1, Value << Shift);
            }

            return Res;
        }

        public static AVec Sshr64(AVec Vector, int Shift, int Size)
        {
            return Sshr(Vector, Shift, Size, 8);
        }

        public static AVec Sshr128(AVec Vector, int Shift, int Size)
        {
            return Sshr(Vector, Shift, Size, 16);
        }

        private static AVec Sshr(AVec Vector, int Shift, int Size, int Bytes)
        {
            AVec Res = new AVec();

            int Elems = Bytes >> Size;

            for (int Index = 0; Index < Elems; Index++)
            {
                long Value = ExtractSVec(Vector, Index, Size);

                Res = InsertSVec(Res, Index, Size, Value >> Shift);
            }

            return Res;
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

        public static AVec Uaddlv64(AVec Vector, int Size)
        {
            return Uaddlv(Vector, Size, 8);
        }

        public static AVec Uaddlv128(AVec Vector, int Size)
        {
            return Uaddlv(Vector, Size, 16);
        }

        private static AVec Uaddlv(AVec Vector, int Size, int Bytes)
        {
            int Elems = Bytes >> Size;

            ulong Sum = 0;

            for (int Index = 0; Index < Elems; Index++)
            {
                Sum += ExtractVec(Vector, Index, Size);
            }

            return InsertVec(new AVec(), 0, 3, Sum);
        }

        public static AVec Uaddw(AVec LHS, AVec RHS, int Size)
        {
            return Uaddw_(LHS, RHS, Size, false);
        }

        public static AVec Uaddw2(AVec LHS, AVec RHS, int Size)
        {
            return Uaddw_(LHS, RHS, Size, true);
        }

        private static AVec Uaddw_(AVec LHS, AVec RHS, int Size, bool High)
        {
            AVec Res = new AVec();

            int Elems = 8 >> Size;
            int Part  = High ? Elems : 0;

            for (int Index = 0; Index < Elems; Index++)
            {
                ulong L = ExtractVec(LHS, Index,        Size + 1);
                ulong R = ExtractVec(RHS, Index + Part, Size);

                Res = InsertVec(Res, Index, Size + 1, L + R);
            }

            return Res;
        }

        public static AVec Ucvtf_V_F(AVec Vector)
        {
            return new AVec()
            {
                S0 = (uint)Vector.W0,
                S1 = (uint)Vector.W1,
                S2 = (uint)Vector.W2,
                S3 = (uint)Vector.W3
            };
        }

        public static AVec Ucvtf_V_D(AVec Vector)
        {
            return new AVec()
            {
                D0 = (ulong)Vector.X0,
                D1 = (ulong)Vector.X1
            };
        }

        public static AVec Ushll(AVec Vector, int Shift, int Size)
        {
            return Ushll_(Vector, Shift, Size, false);
        }

        public static AVec Ushll2(AVec Vector, int Shift, int Size)
        {
            return Ushll_(Vector, Shift, Size, true);
        }

        private static AVec Ushll_(AVec Vector, int Shift, int Size, bool High)
        {
            AVec Res = new AVec();

            int Elems = 8 >> Size;
            int Part  = High ? Elems : 0;

            for (int Index = 0; Index < Elems; Index++)
            {
                ulong Value = ExtractVec(Vector, Index + Part, Size);

                Res = InsertVec(Res, Index, Size + 1, Value << Shift);
            }

            return Res;
        }

        public static AVec Ushr64(AVec Vector, int Shift, int Size)
        {
            return Ushr(Vector, Shift, Size, 8);
        }

        public static AVec Ushr128(AVec Vector, int Shift, int Size)
        {
            return Ushr(Vector, Shift, Size, 16);
        }

        private static AVec Ushr(AVec Vector, int Shift, int Size, int Bytes)
        {
            AVec Res = new AVec();

            int Elems = Bytes >> Size;

            for (int Index = 0; Index < Elems; Index++)
            {
                ulong Value = ExtractVec(Vector, Index, Size);

                Res = InsertVec(Res, Index, Size, Value >> Shift);
            }

            return Res;
        }

        public static AVec Usra64(AVec Res, AVec Vector, int Shift, int Size)
        {
            return Usra(Res, Vector, Shift, Size, 8);
        }

        public static AVec Usra128(AVec Res, AVec Vector, int Shift, int Size)
        {
            return Usra(Res, Vector, Shift, Size, 16);
        }

        private static AVec Usra(AVec Res, AVec Vector, int Shift, int Size, int Bytes)
        {
            int Elems = Bytes >> Size;

            for (int Index = 0; Index < Elems; Index++)
            {
                ulong Value  = ExtractVec(Vector, Index, Size);
                ulong Addend = ExtractVec(Res,    Index, Size);

                Res = InsertVec(Res, Index, Size, Addend + (Value >> Shift));
            }

            return Res;
        }

        public static AVec Uzp1_V64(AVec LHS, AVec RHS, int Size)
        {
            return Uzp(LHS, RHS, Size, 0, 8);
        }

        public static AVec Uzp1_V128(AVec LHS, AVec RHS, int Size)
        {
            return Uzp(LHS, RHS, Size, 0, 16);
        }

        public static AVec Uzp2_V64(AVec LHS, AVec RHS, int Size)
        {
            return Uzp(LHS, RHS, Size, 1, 8);
        }

        public static AVec Uzp2_V128(AVec LHS, AVec RHS, int Size)
        {
            return Uzp(LHS, RHS, Size, 1, 16);
        }

        private static AVec Uzp(AVec LHS, AVec RHS, int Size, int Part, int Bytes)
        {
            AVec Res = new AVec();

            int Elems = Bytes >> Size;
            int Half  = Elems >> 1;

            for (int Index = 0; Index < Elems; Index++)
            {
                int Elem = (Index & (Half - 1)) << 1;

                ulong Value = Index < Half
                    ? ExtractVec(LHS, Elem + Part, Size)
                    : ExtractVec(RHS, Elem + Part, Size);
 
                Res = InsertVec(Res, Index, Size, Value);
            }

            return Res;
        }

        public static AVec Xtn(AVec Vector, int Size)
        {
            return Xtn_(Vector, Size, false);
        }

        public static AVec Xtn2(AVec Vector, int Size)
        {
            return Xtn_(Vector, Size, true);
        }

        private static AVec Xtn_(AVec Vector, int Size, bool High)
        {
            AVec Res = new AVec();

            int Elems = 8 >> Size;
            int Part  = High ? Elems : 0;

            for (int Index = 0; Index < Elems; Index++)
            {
                ulong Value = ExtractVec(Vector, Index, Size + 1);

                Res = InsertVec(Res, Index + Part, Size, Value);
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