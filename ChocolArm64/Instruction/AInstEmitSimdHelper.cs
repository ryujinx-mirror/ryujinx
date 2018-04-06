using ChocolArm64.Decoder;
using ChocolArm64.State;
using ChocolArm64.Translation;
using System;
using System.Reflection;

namespace ChocolArm64.Instruction
{
    static class AInstEmitSimdHelper
    {
        [Flags]
        public enum OperFlags
        {
            Rd = 1 << 0,
            Rn = 1 << 1,
            Rm = 1 << 2,
            Ra = 1 << 3,

            RnRm   = Rn | Rm,
            RdRn   = Rd | Rn,
            RaRnRm = Ra | Rn | Rm,
            RdRnRm = Rd | Rn | Rm
        }

        public static int GetImmShl(AOpCodeSimdShImm Op)
        {
            return Op.Imm - (8 << Op.Size);
        }

        public static int GetImmShr(AOpCodeSimdShImm Op)
        {
            return (8 << (Op.Size + 1)) - Op.Imm;
        }

        public static void EmitUnaryMathCall(AILEmitterCtx Context, string Name)
        {
            IAOpCodeSimd Op = (IAOpCodeSimd)Context.CurrOp;

            int SizeF = Op.Size & 1;

            MethodInfo MthdInfo;

            if (SizeF == 0)
            {
                MthdInfo = typeof(MathF).GetMethod(Name, new Type[] { typeof(float) });
            }
            else /* if (SizeF == 1) */
            {
                MthdInfo = typeof(Math).GetMethod(Name, new Type[] { typeof(double) });
            }

            Context.EmitCall(MthdInfo);
        }

        public static void EmitBinaryMathCall(AILEmitterCtx Context, string Name)
        {
            IAOpCodeSimd Op = (IAOpCodeSimd)Context.CurrOp;

            int SizeF = Op.Size & 1;

            MethodInfo MthdInfo;

            if (SizeF == 0)
            {
                MthdInfo = typeof(MathF).GetMethod(Name, new Type[] { typeof(float), typeof(float) });
            }
            else /* if (SizeF == 1) */
            {
                MthdInfo = typeof(Math).GetMethod(Name, new Type[] { typeof(double), typeof(double) });
            }

            Context.EmitCall(MthdInfo);
        }

        public static void EmitRoundMathCall(AILEmitterCtx Context, MidpointRounding RoundMode)
        {
            IAOpCodeSimd Op = (IAOpCodeSimd)Context.CurrOp;

            int SizeF = Op.Size & 1;

            Context.EmitLdc_I4((int)RoundMode);

            MethodInfo MthdInfo;

            Type[] Types = new Type[] { null, typeof(MidpointRounding) };

            Types[0] = SizeF == 0
                ? typeof(float)
                : typeof(double);

            if (SizeF == 0)
            {
                MthdInfo = typeof(MathF).GetMethod(nameof(MathF.Round), Types);
            }
            else /* if (SizeF == 1) */
            {
                MthdInfo = typeof(Math).GetMethod(nameof(Math.Round), Types);
            }

            Context.EmitCall(MthdInfo);
        }

        public static void EmitUnarySoftFloatCall(AILEmitterCtx Context, string Name)
        {
            IAOpCodeSimd Op = (IAOpCodeSimd)Context.CurrOp;

            int SizeF = Op.Size & 1;

            MethodInfo MthdInfo;

            if (SizeF == 0)
            {
                MthdInfo = typeof(ASoftFloat).GetMethod(Name, new Type[] { typeof(float) });
            }
            else /* if (SizeF == 1) */
            {
                MthdInfo = typeof(ASoftFloat).GetMethod(Name, new Type[] { typeof(double) });
            }

            Context.EmitCall(MthdInfo);
        }

        public static void EmitScalarUnaryOpSx(AILEmitterCtx Context, Action Emit)
        {
            EmitScalarOp(Context, Emit, OperFlags.Rn, true);
        }

        public static void EmitScalarBinaryOpSx(AILEmitterCtx Context, Action Emit)
        {
            EmitScalarOp(Context, Emit, OperFlags.RnRm, true);
        }

        public static void EmitScalarUnaryOpZx(AILEmitterCtx Context, Action Emit)
        {
            EmitScalarOp(Context, Emit, OperFlags.Rn, false);
        }

        public static void EmitScalarBinaryOpZx(AILEmitterCtx Context, Action Emit)
        {
            EmitScalarOp(Context, Emit, OperFlags.RnRm, false);
        }

        public static void EmitScalarTernaryOpZx(AILEmitterCtx Context, Action Emit)
        {
            EmitScalarOp(Context, Emit, OperFlags.RdRnRm, false);
        }

        public static void EmitScalarOp(AILEmitterCtx Context, Action Emit, OperFlags Opers, bool Signed)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            if (Opers.HasFlag(OperFlags.Rd))
            {
                EmitVectorExtract(Context, Op.Rd, 0, Op.Size, Signed);
            }

             if (Opers.HasFlag(OperFlags.Rn))
            {
                EmitVectorExtract(Context, Op.Rn, 0, Op.Size, Signed);
            }

            if (Opers.HasFlag(OperFlags.Rm))
            {
                EmitVectorExtract(Context, ((AOpCodeSimdReg)Op).Rm, 0, Op.Size, Signed);
            }

            Emit();

            EmitScalarSet(Context, Op.Rd, Op.Size);
        }

        public static void EmitScalarUnaryOpF(AILEmitterCtx Context, Action Emit)
        {
            EmitScalarOpF(Context, Emit, OperFlags.Rn);
        }

        public static void EmitScalarBinaryOpF(AILEmitterCtx Context, Action Emit)
        {
            EmitScalarOpF(Context, Emit, OperFlags.RnRm);
        }

        public static void EmitScalarTernaryRaOpF(AILEmitterCtx Context, Action Emit)
        {
            EmitScalarOpF(Context, Emit, OperFlags.RaRnRm);
        }

        public static void EmitScalarOpF(AILEmitterCtx Context, Action Emit, OperFlags Opers)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            int SizeF = Op.Size & 1;

            if (Opers.HasFlag(OperFlags.Ra))
            {
                EmitVectorExtractF(Context, ((AOpCodeSimdReg)Op).Ra, 0, SizeF);
            }

            if (Opers.HasFlag(OperFlags.Rn))
            {
                EmitVectorExtractF(Context, Op.Rn, 0, SizeF);
            }

            if (Opers.HasFlag(OperFlags.Rm))
            {
                EmitVectorExtractF(Context, ((AOpCodeSimdReg)Op).Rm, 0, SizeF);
            }

            Emit();

            EmitScalarSetF(Context, Op.Rd, SizeF);
        }

        public static void EmitVectorUnaryOpF(AILEmitterCtx Context, Action Emit)
        {
            EmitVectorOpF(Context, Emit, OperFlags.Rn);
        }

        public static void EmitVectorBinaryOpF(AILEmitterCtx Context, Action Emit)
        {
            EmitVectorOpF(Context, Emit, OperFlags.RnRm);
        }

        public static void EmitVectorTernaryOpF(AILEmitterCtx Context, Action Emit)
        {
            EmitVectorOpF(Context, Emit, OperFlags.RdRnRm);
        }

        public static void EmitVectorOpF(AILEmitterCtx Context, Action Emit, OperFlags Opers)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            int SizeF = Op.Size & 1;

            int Bytes = Context.CurrOp.GetBitsCount() >> 3;

            for (int Index = 0; Index < (Bytes >> SizeF + 2); Index++)
            {
                if (Opers.HasFlag(OperFlags.Rd))
                {
                    EmitVectorExtractF(Context, Op.Rd, Index, SizeF);
                }

                if (Opers.HasFlag(OperFlags.Rn))
                {
                    EmitVectorExtractF(Context, Op.Rn, Index, SizeF);
                }

                if (Opers.HasFlag(OperFlags.Rm))
                {
                    EmitVectorExtractF(Context, ((AOpCodeSimdReg)Op).Rm, Index, SizeF);
                }

                Emit();

                EmitVectorInsertF(Context, Op.Rd, Index, SizeF);
            }

            if (Op.RegisterSize == ARegisterSize.SIMD64)
            {
                EmitVectorZeroUpper(Context, Op.Rd);
            }
        }

        public static void EmitVectorBinaryOpByElemF(AILEmitterCtx Context, Action Emit)
        {
            AOpCodeSimdRegElemF Op = (AOpCodeSimdRegElemF)Context.CurrOp;

            EmitVectorOpByElemF(Context, Emit, Op.Index, Ternary: false);
        }

        public static void EmitVectorTernaryOpByElemF(AILEmitterCtx Context, Action Emit)
        {
            AOpCodeSimdRegElemF Op = (AOpCodeSimdRegElemF)Context.CurrOp;

            EmitVectorOpByElemF(Context, Emit, Op.Index, Ternary: true);
        }

        public static void EmitVectorOpByElemF(AILEmitterCtx Context, Action Emit, int Elem, bool Ternary)
        {
            AOpCodeSimdReg Op = (AOpCodeSimdReg)Context.CurrOp;

            int SizeF = Op.Size & 1;

            int Bytes = Context.CurrOp.GetBitsCount() >> 3;

            for (int Index = 0; Index < (Bytes >> SizeF + 2); Index++)
            {
                if (Ternary)
                {
                    EmitVectorExtractF(Context, Op.Rd, Index, SizeF);
                }

                EmitVectorExtractF(Context, Op.Rn, Index, SizeF);
                EmitVectorExtractF(Context, Op.Rm, Elem,  SizeF);

                Emit();

                EmitVectorInsertTmpF(Context, Index, SizeF);
            }

            Context.EmitLdvectmp();
            Context.EmitStvec(Op.Rd);

            if (Op.RegisterSize == ARegisterSize.SIMD64)
            {
                EmitVectorZeroUpper(Context, Op.Rd);
            }
        }

        public static void EmitVectorUnaryOpSx(AILEmitterCtx Context, Action Emit)
        {
            EmitVectorOp(Context, Emit, OperFlags.Rn, true);
        }

        public static void EmitVectorBinaryOpSx(AILEmitterCtx Context, Action Emit)
        {
            EmitVectorOp(Context, Emit, OperFlags.RnRm, true);
        }

        public static void EmitVectorUnaryOpZx(AILEmitterCtx Context, Action Emit)
        {
            EmitVectorOp(Context, Emit, OperFlags.Rn, false);
        }

        public static void EmitVectorBinaryOpZx(AILEmitterCtx Context, Action Emit)
        {
            EmitVectorOp(Context, Emit, OperFlags.RnRm, false);
        }

        public static void EmitVectorTernaryOpZx(AILEmitterCtx Context, Action Emit)
        {
            EmitVectorOp(Context, Emit, OperFlags.RdRnRm, false);
        }

        public static void EmitVectorOp(AILEmitterCtx Context, Action Emit, OperFlags Opers, bool Signed)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            int Bytes = Context.CurrOp.GetBitsCount() >> 3;

            for (int Index = 0; Index < (Bytes >> Op.Size); Index++)
            {
                if (Opers.HasFlag(OperFlags.Rd))
                {
                    EmitVectorExtract(Context, Op.Rd, Index, Op.Size, Signed);
                }

                if (Opers.HasFlag(OperFlags.Rn))
                {
                    EmitVectorExtract(Context, Op.Rn, Index, Op.Size, Signed);
                }

                if (Opers.HasFlag(OperFlags.Rm))
                {
                    EmitVectorExtract(Context, ((AOpCodeSimdReg)Op).Rm, Index, Op.Size, Signed);
                }

                Emit();

                EmitVectorInsert(Context, Op.Rd, Index, Op.Size);
            }

            if (Op.RegisterSize == ARegisterSize.SIMD64)
            {
                EmitVectorZeroUpper(Context, Op.Rd);
            }
        }

        public static void EmitVectorBinaryOpByElemSx(AILEmitterCtx Context, Action Emit)
        {
            AOpCodeSimdRegElem Op = (AOpCodeSimdRegElem)Context.CurrOp;

            EmitVectorOpByElem(Context, Emit, Op.Index, false, true);
        }

        public static void EmitVectorBinaryOpByElemZx(AILEmitterCtx Context, Action Emit)
        {
            AOpCodeSimdRegElem Op = (AOpCodeSimdRegElem)Context.CurrOp;

            EmitVectorOpByElem(Context, Emit, Op.Index, false, false);
        }

        public static void EmitVectorTernaryOpByElemZx(AILEmitterCtx Context, Action Emit)
        {
            AOpCodeSimdRegElem Op = (AOpCodeSimdRegElem)Context.CurrOp;

            EmitVectorOpByElem(Context, Emit, Op.Index, true, false);
        }

        public static void EmitVectorOpByElem(AILEmitterCtx Context, Action Emit, int Elem, bool Ternary, bool Signed)
        {
            AOpCodeSimdReg Op = (AOpCodeSimdReg)Context.CurrOp;

            int Bytes = Context.CurrOp.GetBitsCount() >> 3;

            for (int Index = 0; Index < (Bytes >> Op.Size); Index++)
            {
                if (Ternary)
                {
                    EmitVectorExtract(Context, Op.Rd, Index, Op.Size, Signed);
                }

                EmitVectorExtract(Context, Op.Rn, Index, Op.Size, Signed);
                EmitVectorExtract(Context, Op.Rm, Elem,  Op.Size, Signed);

                Emit();

                EmitVectorInsertTmp(Context, Index, Op.Size);
            }

            Context.EmitLdvectmp();
            Context.EmitStvec(Op.Rd);

            if (Op.RegisterSize == ARegisterSize.SIMD64)
            {
                EmitVectorZeroUpper(Context, Op.Rd);
            }
        }

        public static void EmitVectorImmUnaryOp(AILEmitterCtx Context, Action Emit)
        {
            EmitVectorImmOp(Context, Emit, false);
        }

        public static void EmitVectorImmBinaryOp(AILEmitterCtx Context, Action Emit)
        {
            EmitVectorImmOp(Context, Emit, true);
        }

        public static void EmitVectorImmOp(AILEmitterCtx Context, Action Emit, bool Binary)
        {
            AOpCodeSimdImm Op = (AOpCodeSimdImm)Context.CurrOp;

            int Bytes = Context.CurrOp.GetBitsCount() >> 3;

            for (int Index = 0; Index < (Bytes >> Op.Size); Index++)
            {
                if (Binary)
                {
                    EmitVectorExtractZx(Context, Op.Rd, Index, Op.Size);
                }

                Context.EmitLdc_I8(Op.Imm);

                Emit();

                EmitVectorInsert(Context, Op.Rd, Index, Op.Size);
            }

            if (Op.RegisterSize == ARegisterSize.SIMD64)
            {
                EmitVectorZeroUpper(Context, Op.Rd);
            }
        }

        public static void EmitVectorWidenRmBinaryOpSx(AILEmitterCtx Context, Action Emit)
        {
            EmitVectorWidenRmBinaryOp(Context, Emit, true);
        }

        public static void EmitVectorWidenRmBinaryOpZx(AILEmitterCtx Context, Action Emit)
        {
            EmitVectorWidenRmBinaryOp(Context, Emit, false);
        }

        public static void EmitVectorWidenRmBinaryOp(AILEmitterCtx Context, Action Emit, bool Signed)
        {
            AOpCodeSimdReg Op = (AOpCodeSimdReg)Context.CurrOp;

            Context.EmitLdvec(Op.Rd);
            Context.EmitStvectmp();

            int Elems = 8 >> Op.Size;

            int Part = Op.RegisterSize == ARegisterSize.SIMD128 ? Elems : 0;

            for (int Index = 0; Index < Elems; Index++)
            {
                EmitVectorExtract(Context, Op.Rn,        Index, Op.Size + 1, Signed);
                EmitVectorExtract(Context, Op.Rm, Part + Index, Op.Size,     Signed);

                Emit();

                EmitVectorInsertTmp(Context, Index, Op.Size + 1);
            }

            Context.EmitLdvectmp();
            Context.EmitStvec(Op.Rd);
        }

        public static void EmitVectorWidenRnRmBinaryOpSx(AILEmitterCtx Context, Action Emit)
        {
            EmitVectorWidenRnRmOp(Context, Emit, false, true);
        }

        public static void EmitVectorWidenRnRmBinaryOpZx(AILEmitterCtx Context, Action Emit)
        {
            EmitVectorWidenRnRmOp(Context, Emit, false, false);
        }

        public static void EmitVectorWidenRnRmTernaryOpSx(AILEmitterCtx Context, Action Emit)
        {
            EmitVectorWidenRnRmOp(Context, Emit, true, true);
        }

        public static void EmitVectorWidenRnRmTernaryOpZx(AILEmitterCtx Context, Action Emit)
        {
            EmitVectorWidenRnRmOp(Context, Emit, true, false);
        }

        public static void EmitVectorWidenRnRmOp(AILEmitterCtx Context, Action Emit, bool Ternary, bool Signed)
        {
            AOpCodeSimdReg Op = (AOpCodeSimdReg)Context.CurrOp;

            Context.EmitLdvec(Op.Rd);
            Context.EmitStvectmp();

            int Elems = 8 >> Op.Size;

            int Part = Op.RegisterSize == ARegisterSize.SIMD128 ? Elems : 0;

            for (int Index = 0; Index < Elems; Index++)
            {
                if (Ternary)
                {
                    EmitVectorExtract(Context, Op.Rd, Index, Op.Size + 1, Signed);
                }

                EmitVectorExtract(Context, Op.Rn, Part + Index, Op.Size, Signed);
                EmitVectorExtract(Context, Op.Rm, Part + Index, Op.Size, Signed);

                Emit();

                EmitVectorInsertTmp(Context, Index, Op.Size + 1);
            }

            Context.EmitLdvectmp();
            Context.EmitStvec(Op.Rd);
        }

        public static void EmitScalarSet(AILEmitterCtx Context, int Reg, int Size)
        {
            EmitVectorZeroAll(Context, Reg);
            EmitVectorInsert(Context, Reg, 0, Size);
        }

        public static void EmitScalarSetF(AILEmitterCtx Context, int Reg, int Size)
        {
            EmitVectorZeroAll(Context, Reg);
            EmitVectorInsertF(Context, Reg, 0, Size);
        }

        public static void EmitVectorExtractSx(AILEmitterCtx Context, int Reg, int Index, int Size)
        {
            EmitVectorExtract(Context, Reg, Index, Size, true);
        }

        public static void EmitVectorExtractZx(AILEmitterCtx Context, int Reg, int Index, int Size)
        {
            EmitVectorExtract(Context, Reg, Index, Size, false);
        }

        public static void EmitVectorExtract(AILEmitterCtx Context, int Reg, int Index, int Size, bool Signed)
        {
            ThrowIfInvalid(Index, Size);

            IAOpCodeSimd Op = (IAOpCodeSimd)Context.CurrOp;

            Context.EmitLdvec(Reg);
            Context.EmitLdc_I4(Index);
            Context.EmitLdc_I4(Size);

            ASoftFallback.EmitCall(Context, Signed
                ? nameof(ASoftFallback.VectorExtractIntSx)
                : nameof(ASoftFallback.VectorExtractIntZx));
        }

        public static void EmitVectorExtractF(AILEmitterCtx Context, int Reg, int Index, int Size)
        {
            ThrowIfInvalidF(Index, Size);

            Context.EmitLdvec(Reg);
            Context.EmitLdc_I4(Index);

            if (Size == 0)
            {
                ASoftFallback.EmitCall(Context, nameof(ASoftFallback.VectorExtractSingle));
            }
            else if (Size == 1)
            {
                ASoftFallback.EmitCall(Context, nameof(ASoftFallback.VectorExtractDouble));
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(Size));
            }
        }

        public static void EmitVectorZeroAll(AILEmitterCtx Context, int Rd)
        {
            EmitVectorZeroLower(Context, Rd);
            EmitVectorZeroUpper(Context, Rd);
        }

        public static void EmitVectorZeroLower(AILEmitterCtx Context, int Rd)
        {
            EmitVectorInsert(Context, Rd, 0, 3, 0);
        }

        public static void EmitVectorZeroUpper(AILEmitterCtx Context, int Rd)
        {
            EmitVectorInsert(Context, Rd, 1, 3, 0);
        }

        public static void EmitVectorInsert(AILEmitterCtx Context, int Reg, int Index, int Size)
        {
            ThrowIfInvalid(Index, Size);

            Context.EmitLdvec(Reg);
            Context.EmitLdc_I4(Index);
            Context.EmitLdc_I4(Size);

            ASoftFallback.EmitCall(Context, nameof(ASoftFallback.VectorInsertInt));

            Context.EmitStvec(Reg);
        }

        public static void EmitVectorInsertTmp(AILEmitterCtx Context, int Index, int Size)
        {
            ThrowIfInvalid(Index, Size);

            Context.EmitLdvectmp();
            Context.EmitLdc_I4(Index);
            Context.EmitLdc_I4(Size);

            ASoftFallback.EmitCall(Context, nameof(ASoftFallback.VectorInsertInt));

            Context.EmitStvectmp();
        }

        public static void EmitVectorInsert(AILEmitterCtx Context, int Reg, int Index, int Size, long Value)
        {
            ThrowIfInvalid(Index, Size);

            Context.EmitLdc_I8(Value);
            Context.EmitLdvec(Reg);
            Context.EmitLdc_I4(Index);
            Context.EmitLdc_I4(Size);

            ASoftFallback.EmitCall(Context, nameof(ASoftFallback.VectorInsertInt));

            Context.EmitStvec(Reg);
        }

        public static void EmitVectorInsertF(AILEmitterCtx Context, int Reg, int Index, int Size)
        {
            ThrowIfInvalidF(Index, Size);

            Context.EmitLdvec(Reg);
            Context.EmitLdc_I4(Index);

            if (Size == 0)
            {
                ASoftFallback.EmitCall(Context, nameof(ASoftFallback.VectorInsertSingle));
            }
            else if (Size == 1)
            {
                ASoftFallback.EmitCall(Context, nameof(ASoftFallback.VectorInsertDouble));
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(Size));
            }

            Context.EmitStvec(Reg);
        }

        public static void EmitVectorInsertTmpF(AILEmitterCtx Context, int Index, int Size)
        {
            ThrowIfInvalidF(Index, Size);

            Context.EmitLdvectmp();
            Context.EmitLdc_I4(Index);

            if (Size == 0)
            {
                ASoftFallback.EmitCall(Context, nameof(ASoftFallback.VectorInsertSingle));
            }
            else if (Size == 1)
            {
                ASoftFallback.EmitCall(Context, nameof(ASoftFallback.VectorInsertDouble));
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(Size));
            }

            Context.EmitStvectmp();
        }

        private static void ThrowIfInvalid(int Index, int Size)
        {
            if ((uint)Size > 3)
            {
                throw new ArgumentOutOfRangeException(nameof(Size));
            }

            if ((uint)Index >= 16 >> Size)
            {
                throw new ArgumentOutOfRangeException(nameof(Index));
            }
        }

        private static void ThrowIfInvalidF(int Index, int Size)
        {
            if ((uint)Size > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(Size));
            }

            if ((uint)Index >= 4 >> Size)
            {
                throw new ArgumentOutOfRangeException(nameof(Index));
            }
        }
    }
}