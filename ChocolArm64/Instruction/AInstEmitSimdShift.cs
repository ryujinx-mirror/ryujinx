using ChocolArm64.Decoder;
using ChocolArm64.State;
using ChocolArm64.Translation;
using System;
using System.Reflection.Emit;

using static ChocolArm64.Instruction.AInstEmitSimdHelper;

namespace ChocolArm64.Instruction
{
    static partial class AInstEmit
    {
        public static void Rshrn_V(AILEmitterCtx Context)
        {
            EmitVectorShrImmNarrowOpZx(Context, Round: true);
        }

        public static void Shl_S(AILEmitterCtx Context)
        {
            AOpCodeSimdShImm Op = (AOpCodeSimdShImm)Context.CurrOp;

            EmitScalarUnaryOpZx(Context, () =>
            {
                Context.EmitLdc_I4(GetImmShl(Op));

                Context.Emit(OpCodes.Shl);
            });
        }

        public static void Shl_V(AILEmitterCtx Context)
        {
            AOpCodeSimdShImm Op = (AOpCodeSimdShImm)Context.CurrOp;

            EmitVectorUnaryOpZx(Context, () =>
            {
                Context.EmitLdc_I4(GetImmShl(Op));

                Context.Emit(OpCodes.Shl);
            });
        }

        public static void Shll_V(AILEmitterCtx Context)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            int Shift = 8 << Op.Size;

            EmitVectorShImmWidenBinaryZx(Context, () => Context.Emit(OpCodes.Shl), Shift);
        }

        public static void Shrn_V(AILEmitterCtx Context)
        {
            EmitVectorShrImmNarrowOpZx(Context, Round: false);
        }

        public static void Sli_V(AILEmitterCtx Context)
        {
            AOpCodeSimdShImm Op = (AOpCodeSimdShImm)Context.CurrOp;

            int Bytes = Op.GetBitsCount() >> 3;
            int Elems = Bytes >> Op.Size;

            int Shift = GetImmShl(Op);

            ulong Mask = Shift != 0 ? ulong.MaxValue >> (64 - Shift) : 0;

            for (int Index = 0; Index < Elems; Index++)
            {
                EmitVectorExtractZx(Context, Op.Rn, Index, Op.Size);

                Context.EmitLdc_I4(Shift);

                Context.Emit(OpCodes.Shl);

                EmitVectorExtractZx(Context, Op.Rd, Index, Op.Size);

                Context.EmitLdc_I8((long)Mask);

                Context.Emit(OpCodes.And);
                Context.Emit(OpCodes.Or);

                EmitVectorInsert(Context, Op.Rd, Index, Op.Size);
            }

            if (Op.RegisterSize == ARegisterSize.SIMD64)
            {
                EmitVectorZeroUpper(Context, Op.Rd);
            }
        }

        public static void Sqrshrn_S(AILEmitterCtx Context)
        {
            EmitRoundShrImmSaturatingNarrowOp(Context, ShrImmSaturatingNarrowFlags.ScalarSxSx);
        }

        public static void Sqrshrn_V(AILEmitterCtx Context)
        {
            EmitRoundShrImmSaturatingNarrowOp(Context, ShrImmSaturatingNarrowFlags.VectorSxSx);
        }

        public static void Sqrshrun_S(AILEmitterCtx Context)
        {
            EmitRoundShrImmSaturatingNarrowOp(Context, ShrImmSaturatingNarrowFlags.ScalarSxZx);
        }

        public static void Sqrshrun_V(AILEmitterCtx Context)
        {
            EmitRoundShrImmSaturatingNarrowOp(Context, ShrImmSaturatingNarrowFlags.VectorSxZx);
        }

        public static void Sqshrn_S(AILEmitterCtx Context)
        {
            EmitShrImmSaturatingNarrowOp(Context, ShrImmSaturatingNarrowFlags.ScalarSxSx);
        }

        public static void Sqshrn_V(AILEmitterCtx Context)
        {
            EmitShrImmSaturatingNarrowOp(Context, ShrImmSaturatingNarrowFlags.VectorSxSx);
        }

        public static void Sqshrun_S(AILEmitterCtx Context)
        {
            EmitShrImmSaturatingNarrowOp(Context, ShrImmSaturatingNarrowFlags.ScalarSxZx);
        }

        public static void Sqshrun_V(AILEmitterCtx Context)
        {
            EmitShrImmSaturatingNarrowOp(Context, ShrImmSaturatingNarrowFlags.VectorSxZx);
        }

        public static void Srshr_S(AILEmitterCtx Context)
        {
            EmitScalarShrImmOpSx(Context, ShrImmFlags.Round);
        }

        public static void Srshr_V(AILEmitterCtx Context)
        {
            EmitVectorShrImmOpSx(Context, ShrImmFlags.Round);
        }

        public static void Srsra_S(AILEmitterCtx Context)
        {
            EmitScalarShrImmOpSx(Context, ShrImmFlags.Round | ShrImmFlags.Accumulate);
        }

        public static void Srsra_V(AILEmitterCtx Context)
        {
            EmitVectorShrImmOpSx(Context, ShrImmFlags.Round | ShrImmFlags.Accumulate);
        }

        public static void Sshl_V(AILEmitterCtx Context)
        {
            EmitVectorShl(Context, Signed: true);
        }

        public static void Sshll_V(AILEmitterCtx Context)
        {
            AOpCodeSimdShImm Op = (AOpCodeSimdShImm)Context.CurrOp;

            EmitVectorShImmWidenBinarySx(Context, () => Context.Emit(OpCodes.Shl), GetImmShl(Op));
        }

        public static void Sshr_S(AILEmitterCtx Context)
        {
            EmitShrImmOp(Context, ShrImmFlags.ScalarSx);
        }

        public static void Sshr_V(AILEmitterCtx Context)
        {
            EmitShrImmOp(Context, ShrImmFlags.VectorSx);
        }

        public static void Ssra_S(AILEmitterCtx Context)
        {
            EmitScalarShrImmOpSx(Context, ShrImmFlags.Accumulate);
        }

        public static void Ssra_V(AILEmitterCtx Context)
        {
            EmitVectorShrImmOpSx(Context, ShrImmFlags.Accumulate);
        }

        public static void Uqrshrn_S(AILEmitterCtx Context)
        {
            EmitRoundShrImmSaturatingNarrowOp(Context, ShrImmSaturatingNarrowFlags.ScalarZxZx);
        }

        public static void Uqrshrn_V(AILEmitterCtx Context)
        {
            EmitRoundShrImmSaturatingNarrowOp(Context, ShrImmSaturatingNarrowFlags.VectorZxZx);
        }

        public static void Uqshrn_S(AILEmitterCtx Context)
        {
            EmitShrImmSaturatingNarrowOp(Context, ShrImmSaturatingNarrowFlags.ScalarZxZx);
        }

        public static void Uqshrn_V(AILEmitterCtx Context)
        {
            EmitShrImmSaturatingNarrowOp(Context, ShrImmSaturatingNarrowFlags.VectorZxZx);
        }

        public static void Urshr_S(AILEmitterCtx Context)
        {
            EmitScalarShrImmOpZx(Context, ShrImmFlags.Round);
        }

        public static void Urshr_V(AILEmitterCtx Context)
        {
            EmitVectorShrImmOpZx(Context, ShrImmFlags.Round);
        }

        public static void Ursra_S(AILEmitterCtx Context)
        {
            EmitScalarShrImmOpZx(Context, ShrImmFlags.Round | ShrImmFlags.Accumulate);
        }

        public static void Ursra_V(AILEmitterCtx Context)
        {
            EmitVectorShrImmOpZx(Context, ShrImmFlags.Round | ShrImmFlags.Accumulate);
        }

        public static void Ushl_V(AILEmitterCtx Context)
        {
            EmitVectorShl(Context, Signed: false);
        }

        public static void Ushll_V(AILEmitterCtx Context)
        {
            AOpCodeSimdShImm Op = (AOpCodeSimdShImm)Context.CurrOp;

            EmitVectorShImmWidenBinaryZx(Context, () => Context.Emit(OpCodes.Shl), GetImmShl(Op));
        }

        public static void Ushr_S(AILEmitterCtx Context)
        {
            EmitShrImmOp(Context, ShrImmFlags.ScalarZx);
        }

        public static void Ushr_V(AILEmitterCtx Context)
        {
            EmitShrImmOp(Context, ShrImmFlags.VectorZx);
        }

        public static void Usra_S(AILEmitterCtx Context)
        {
            EmitScalarShrImmOpZx(Context, ShrImmFlags.Accumulate);
        }

        public static void Usra_V(AILEmitterCtx Context)
        {
            EmitVectorShrImmOpZx(Context, ShrImmFlags.Accumulate);
        }

        private static void EmitVectorShl(AILEmitterCtx Context, bool Signed)
        {
            //This instruction shifts the value on vector A by the number of bits
            //specified on the signed, lower 8 bits of vector B. If the shift value
            //is greater or equal to the data size of each lane, then the result is zero.
            //Additionally, negative shifts produces right shifts by the negated shift value.
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            int MaxShift = 8 << Op.Size;

            Action Emit = () =>
            {
                AILLabel LblShl  = new AILLabel();
                AILLabel LblZero = new AILLabel();
                AILLabel LblEnd  = new AILLabel();

                void EmitShift(OpCode ILOp)
                {
                    Context.Emit(OpCodes.Dup);

                    Context.EmitLdc_I4(MaxShift);

                    Context.Emit(OpCodes.Bge_S, LblZero);
                    Context.Emit(ILOp);
                    Context.Emit(OpCodes.Br_S, LblEnd);
                }

                Context.Emit(OpCodes.Conv_I1);
                Context.Emit(OpCodes.Dup);

                Context.EmitLdc_I4(0);

                Context.Emit(OpCodes.Bge_S, LblShl);
                Context.Emit(OpCodes.Neg);

                EmitShift(Signed
                    ? OpCodes.Shr
                    : OpCodes.Shr_Un);

                Context.MarkLabel(LblShl);

                EmitShift(OpCodes.Shl);

                Context.MarkLabel(LblZero);

                Context.Emit(OpCodes.Pop);
                Context.Emit(OpCodes.Pop);

                Context.EmitLdc_I8(0);

                Context.MarkLabel(LblEnd);
            };

            if (Signed)
            {
                EmitVectorBinaryOpSx(Context, Emit);
            }
            else
            {
                EmitVectorBinaryOpZx(Context, Emit);
            }
        }

        [Flags]
        private enum ShrImmFlags
        {
            Scalar = 1 << 0,
            Signed = 1 << 1,

            Round      = 1 << 2,
            Accumulate = 1 << 3,

            ScalarSx = Scalar | Signed,
            ScalarZx = Scalar,

            VectorSx = Signed,
            VectorZx = 0
        }

        private static void EmitScalarShrImmOpSx(AILEmitterCtx Context, ShrImmFlags Flags)
        {
            EmitShrImmOp(Context, ShrImmFlags.ScalarSx | Flags);
        }

        private static void EmitScalarShrImmOpZx(AILEmitterCtx Context, ShrImmFlags Flags)
        {
            EmitShrImmOp(Context, ShrImmFlags.ScalarZx | Flags);
        }

        private static void EmitVectorShrImmOpSx(AILEmitterCtx Context, ShrImmFlags Flags)
        {
            EmitShrImmOp(Context, ShrImmFlags.VectorSx | Flags);
        }

        private static void EmitVectorShrImmOpZx(AILEmitterCtx Context, ShrImmFlags Flags)
        {
            EmitShrImmOp(Context, ShrImmFlags.VectorZx | Flags);
        }

        private static void EmitShrImmOp(AILEmitterCtx Context, ShrImmFlags Flags)
        {
            AOpCodeSimdShImm Op = (AOpCodeSimdShImm)Context.CurrOp;

            bool Scalar     = (Flags & ShrImmFlags.Scalar)     != 0;
            bool Signed     = (Flags & ShrImmFlags.Signed)     != 0;
            bool Round      = (Flags & ShrImmFlags.Round)      != 0;
            bool Accumulate = (Flags & ShrImmFlags.Accumulate) != 0;

            int Shift = GetImmShr(Op);

            long RoundConst = 1L << (Shift - 1);

            int Bytes = Op.GetBitsCount() >> 3;
            int Elems = !Scalar ? Bytes >> Op.Size : 1;

            for (int Index = 0; Index < Elems; Index++)
            {
                EmitVectorExtract(Context, Op.Rn, Index, Op.Size, Signed);

                if (Op.Size <= 2)
                {
                    if (Round)
                    {
                        Context.EmitLdc_I8(RoundConst);

                        Context.Emit(OpCodes.Add);
                    }

                    Context.EmitLdc_I4(Shift);

                    Context.Emit(Signed ? OpCodes.Shr : OpCodes.Shr_Un);
                }
                else /* if (Op.Size == 3) */
                {
                    EmitShrImm_64(Context, Signed, Round ? RoundConst : 0L, Shift);
                }

                if (Accumulate)
                {
                    EmitVectorExtract(Context, Op.Rd, Index, Op.Size, Signed);

                    Context.Emit(OpCodes.Add);
                }

                EmitVectorInsertTmp(Context, Index, Op.Size);
            }

            Context.EmitLdvectmp();
            Context.EmitStvec(Op.Rd);

            if ((Op.RegisterSize == ARegisterSize.SIMD64) || Scalar)
            {
                EmitVectorZeroUpper(Context, Op.Rd);
            }
        }

        private static void EmitVectorShrImmNarrowOpZx(AILEmitterCtx Context, bool Round)
        {
            AOpCodeSimdShImm Op = (AOpCodeSimdShImm)Context.CurrOp;

            int Shift = GetImmShr(Op);

            long RoundConst = 1L << (Shift - 1);

            int Elems = 8 >> Op.Size;

            int Part = Op.RegisterSize == ARegisterSize.SIMD128 ? Elems : 0;

            if (Part != 0)
            {
                Context.EmitLdvec(Op.Rd);
                Context.EmitStvectmp();
            }

            for (int Index = 0; Index < Elems; Index++)
            {
                EmitVectorExtractZx(Context, Op.Rn, Index, Op.Size + 1);

                if (Round)
                {
                    Context.EmitLdc_I8(RoundConst);

                    Context.Emit(OpCodes.Add);
                }

                Context.EmitLdc_I4(Shift);

                Context.Emit(OpCodes.Shr_Un);

                EmitVectorInsertTmp(Context, Part + Index, Op.Size);
            }

            Context.EmitLdvectmp();
            Context.EmitStvec(Op.Rd);

            if (Part == 0)
            {
                EmitVectorZeroUpper(Context, Op.Rd);
            }
        }

        [Flags]
        private enum ShrImmSaturatingNarrowFlags
        {
            Scalar    = 1 << 0,
            SignedSrc = 1 << 1,
            SignedDst = 1 << 2,

            Round = 1 << 3,

            ScalarSxSx = Scalar | SignedSrc | SignedDst,
            ScalarSxZx = Scalar | SignedSrc,
            ScalarZxZx = Scalar,

            VectorSxSx = SignedSrc | SignedDst,
            VectorSxZx = SignedSrc,
            VectorZxZx = 0
        }

        private static void EmitRoundShrImmSaturatingNarrowOp(AILEmitterCtx Context, ShrImmSaturatingNarrowFlags Flags)
        {
            EmitShrImmSaturatingNarrowOp(Context, ShrImmSaturatingNarrowFlags.Round | Flags);
        }

        private static void EmitShrImmSaturatingNarrowOp(AILEmitterCtx Context, ShrImmSaturatingNarrowFlags Flags)
        {
            AOpCodeSimdShImm Op = (AOpCodeSimdShImm)Context.CurrOp;

            bool Scalar    = (Flags & ShrImmSaturatingNarrowFlags.Scalar)    != 0;
            bool SignedSrc = (Flags & ShrImmSaturatingNarrowFlags.SignedSrc) != 0;
            bool SignedDst = (Flags & ShrImmSaturatingNarrowFlags.SignedDst) != 0;
            bool Round     = (Flags & ShrImmSaturatingNarrowFlags.Round)     != 0;

            int Shift = GetImmShr(Op);

            long RoundConst = 1L << (Shift - 1);

            int Elems = !Scalar ? 8 >> Op.Size : 1;

            int Part = !Scalar && (Op.RegisterSize == ARegisterSize.SIMD128) ? Elems : 0;

            if (Scalar)
            {
                EmitVectorZeroLowerTmp(Context);
            }

            if (Part != 0)
            {
                Context.EmitLdvec(Op.Rd);
                Context.EmitStvectmp();
            }

            for (int Index = 0; Index < Elems; Index++)
            {
                EmitVectorExtract(Context, Op.Rn, Index, Op.Size + 1, SignedSrc);

                if (Op.Size <= 1 || !Round)
                {
                    if (Round)
                    {
                        Context.EmitLdc_I8(RoundConst);

                        Context.Emit(OpCodes.Add);
                    }

                    Context.EmitLdc_I4(Shift);

                    Context.Emit(SignedSrc ? OpCodes.Shr : OpCodes.Shr_Un);
                }
                else /* if (Op.Size == 2 && Round) */
                {
                    EmitShrImm_64(Context, SignedSrc, RoundConst, Shift); // Shift <= 32
                }

                EmitSatQ(Context, Op.Size, SignedSrc, SignedDst);

                EmitVectorInsertTmp(Context, Part + Index, Op.Size);
            }

            Context.EmitLdvectmp();
            Context.EmitStvec(Op.Rd);

            if (Part == 0)
            {
                EmitVectorZeroUpper(Context, Op.Rd);
            }
        }

        // Dst_64 = (Int(Src_64, Signed) + RoundConst) >> Shift;
        private static void EmitShrImm_64(
            AILEmitterCtx Context,
            bool Signed,
            long RoundConst,
            int  Shift)
        {
            Context.EmitLdc_I8(RoundConst);
            Context.EmitLdc_I4(Shift);

            ASoftFallback.EmitCall(Context, Signed
                ? nameof(ASoftFallback.SignedShrImm_64)
                : nameof(ASoftFallback.UnsignedShrImm_64));
        }

        private static void EmitVectorShImmWidenBinarySx(AILEmitterCtx Context, Action Emit, int Imm)
        {
            EmitVectorShImmWidenBinaryOp(Context, Emit, Imm, true);
        }

        private static void EmitVectorShImmWidenBinaryZx(AILEmitterCtx Context, Action Emit, int Imm)
        {
            EmitVectorShImmWidenBinaryOp(Context, Emit, Imm, false);
        }

        private static void EmitVectorShImmWidenBinaryOp(AILEmitterCtx Context, Action Emit, int Imm, bool Signed)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            int Elems = 8 >> Op.Size;

            int Part = Op.RegisterSize == ARegisterSize.SIMD128 ? Elems : 0;

            for (int Index = 0; Index < Elems; Index++)
            {
                EmitVectorExtract(Context, Op.Rn, Part + Index, Op.Size, Signed);

                Context.EmitLdc_I4(Imm);

                Emit();

                EmitVectorInsertTmp(Context, Index, Op.Size + 1);
            }

            Context.EmitLdvectmp();
            Context.EmitStvec(Op.Rd);
        }
    }
}
