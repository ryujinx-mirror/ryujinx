using ChocolArm64.Decoder;
using ChocolArm64.State;
using ChocolArm64.Translation;
using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Intrinsics.X86;

using static ChocolArm64.Instruction.AInstEmitSimdHelper;

namespace ChocolArm64.Instruction
{
    static partial class AInstEmit
    {
        public static void Abs_S(AILEmitterCtx Context)
        {
            EmitScalarUnaryOpSx(Context, () => EmitAbs(Context));
        }

        public static void Abs_V(AILEmitterCtx Context)
        {
            EmitVectorUnaryOpSx(Context, () => EmitAbs(Context));
        }

        public static void Add_S(AILEmitterCtx Context)
        {
            EmitScalarBinaryOpZx(Context, () => Context.Emit(OpCodes.Add));
        }

        public static void Add_V(AILEmitterCtx Context)
        {
            if (AOptimizations.UseSse2)
            {
                EmitSse2Call(Context, nameof(Sse2.Add));
            }
            else
            {
                EmitVectorBinaryOpZx(Context, () => Context.Emit(OpCodes.Add));
            }
        }

        public static void Addhn_V(AILEmitterCtx Context)
        {
            EmitHighNarrow(Context, () => Context.Emit(OpCodes.Add), Round: false);
        }

        public static void Addp_S(AILEmitterCtx Context)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            EmitVectorExtractZx(Context, Op.Rn, 0, Op.Size);
            EmitVectorExtractZx(Context, Op.Rn, 1, Op.Size);

            Context.Emit(OpCodes.Add);

            EmitScalarSet(Context, Op.Rd, Op.Size);
        }

        public static void Addp_V(AILEmitterCtx Context)
        {
            EmitVectorPairwiseOpZx(Context, () => Context.Emit(OpCodes.Add));
        }

        public static void Addv_V(AILEmitterCtx Context)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            int Bytes = Op.GetBitsCount() >> 3;
            int Elems = Bytes >> Op.Size;

            EmitVectorExtractZx(Context, Op.Rn, 0, Op.Size);

            for (int Index = 1; Index < Elems; Index++)
            {
                EmitVectorExtractZx(Context, Op.Rn, Index, Op.Size);

                Context.Emit(OpCodes.Add);
            }

            EmitScalarSet(Context, Op.Rd, Op.Size);
        }

        public static void Cls_V(AILEmitterCtx Context)
        {
            MethodInfo MthdInfo = typeof(ASoftFallback).GetMethod(nameof(ASoftFallback.CountLeadingSigns));

            EmitCountLeadingBits(Context, () => Context.EmitCall(MthdInfo));
        }

        public static void Clz_V(AILEmitterCtx Context)
        {
            MethodInfo MthdInfo = typeof(ASoftFallback).GetMethod(nameof(ASoftFallback.CountLeadingZeros));

            EmitCountLeadingBits(Context, () => Context.EmitCall(MthdInfo));
        }

        private static void EmitCountLeadingBits(AILEmitterCtx Context, Action Emit)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            int Bytes = Op.GetBitsCount() >> 3;
            int Elems = Bytes >> Op.Size;

            int ESize = 8 << Op.Size;

            for (int Index = 0; Index < Elems; Index++)
            {
                EmitVectorExtractZx(Context, Op.Rn, Index, Op.Size);

                Context.EmitLdc_I4(ESize);

                Emit();

                EmitVectorInsert(Context, Op.Rd, Index, Op.Size);
            }

            if (Op.RegisterSize == ARegisterSize.SIMD64)
            {
                EmitVectorZeroUpper(Context, Op.Rd);
            }
        }

        public static void Cnt_V(AILEmitterCtx Context)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            int Elems = Op.RegisterSize == ARegisterSize.SIMD128 ? 16 : 8;

            for (int Index = 0; Index < Elems; Index++)
            {
                EmitVectorExtractZx(Context, Op.Rn, Index, 0);

                Context.Emit(OpCodes.Conv_U4);

                ASoftFallback.EmitCall(Context, nameof(ASoftFallback.CountSetBits8));

                Context.Emit(OpCodes.Conv_U8);

                EmitVectorInsert(Context, Op.Rd, Index, 0);
            }

            if (Op.RegisterSize == ARegisterSize.SIMD64)
            {
                EmitVectorZeroUpper(Context, Op.Rd);
            }
        }

        private static void EmitAbs(AILEmitterCtx Context)
        {
            AILLabel LblTrue = new AILLabel();

            Context.Emit(OpCodes.Dup);
            Context.Emit(OpCodes.Ldc_I4_0);
            Context.Emit(OpCodes.Bge_S, LblTrue);

            Context.Emit(OpCodes.Neg);

            Context.MarkLabel(LblTrue);
        }

        private static void EmitAddLongPairwise(AILEmitterCtx Context, bool Signed, bool Accumulate)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            int Words = Op.GetBitsCount() >> 4;
            int Pairs = Words >> Op.Size;

            for (int Index = 0; Index < Pairs; Index++)
            {
                int Idx = Index << 1;

                EmitVectorExtract(Context, Op.Rn, Idx,     Op.Size, Signed);
                EmitVectorExtract(Context, Op.Rn, Idx + 1, Op.Size, Signed);

                Context.Emit(OpCodes.Add);

                if (Accumulate)
                {
                    EmitVectorExtract(Context, Op.Rd, Index, Op.Size + 1, Signed);

                    Context.Emit(OpCodes.Add);
                }

                EmitVectorInsertTmp(Context, Index, Op.Size + 1);
            }

            Context.EmitLdvectmp();
            Context.EmitStvec(Op.Rd);

            if (Op.RegisterSize == ARegisterSize.SIMD64)
            {
                EmitVectorZeroUpper(Context, Op.Rd);
            }
        }

        private static void EmitDoublingMultiplyHighHalf(AILEmitterCtx Context, bool Round)
        {
            AOpCodeSimdReg Op = (AOpCodeSimdReg)Context.CurrOp;

            int ESize = 8 << Op.Size;

            Context.Emit(OpCodes.Mul);

            if (!Round)
            {
                Context.EmitAsr(ESize - 1);
            }
            else
            {
                long RoundConst = 1L << (ESize - 1);

                AILLabel LblTrue = new AILLabel();

                Context.EmitLsl(1);

                Context.EmitLdc_I8(RoundConst);

                Context.Emit(OpCodes.Add);

                Context.EmitAsr(ESize);

                Context.Emit(OpCodes.Dup);
                Context.EmitLdc_I8((long)int.MinValue);
                Context.Emit(OpCodes.Bne_Un_S, LblTrue);

                Context.Emit(OpCodes.Neg);

                Context.MarkLabel(LblTrue);
            }
        }

        private static void EmitHighNarrow(AILEmitterCtx Context, Action Emit, bool Round)
        {
            AOpCodeSimdReg Op = (AOpCodeSimdReg)Context.CurrOp;

            int Elems = 8 >> Op.Size;

            int ESize = 8 << Op.Size;

            int Part = Op.RegisterSize == ARegisterSize.SIMD128 ? Elems : 0;

            long RoundConst = 1L << (ESize - 1);

            if (Part != 0)
            {
                Context.EmitLdvec(Op.Rd);
                Context.EmitStvectmp();
            }

            for (int Index = 0; Index < Elems; Index++)
            {
                EmitVectorExtractZx(Context, Op.Rn, Index, Op.Size + 1);
                EmitVectorExtractZx(Context, Op.Rm, Index, Op.Size + 1);

                Emit();

                if (Round)
                {
                    Context.EmitLdc_I8(RoundConst);

                    Context.Emit(OpCodes.Add);
                }

                Context.EmitLsr(ESize);

                EmitVectorInsertTmp(Context, Part + Index, Op.Size);
            }

            Context.EmitLdvectmp();
            Context.EmitStvec(Op.Rd);

            if (Part == 0)
            {
                EmitVectorZeroUpper(Context, Op.Rd);
            }
        }

        public static void Fabd_S(AILEmitterCtx Context)
        {
            EmitScalarBinaryOpF(Context, () =>
            {
                Context.Emit(OpCodes.Sub);

                EmitUnaryMathCall(Context, nameof(Math.Abs));
            });
        }

        public static void Fabs_S(AILEmitterCtx Context)
        {
            EmitScalarUnaryOpF(Context, () =>
            {
                EmitUnaryMathCall(Context, nameof(Math.Abs));
            });
        }

        public static void Fabs_V(AILEmitterCtx Context)
        {
            EmitVectorUnaryOpF(Context, () =>
            {
                EmitUnaryMathCall(Context, nameof(Math.Abs));
            });
        }

        public static void Fadd_S(AILEmitterCtx Context)
        {
            if (AOptimizations.UseSse && AOptimizations.UseSse2)
            {
                EmitScalarSseOrSse2CallF(Context, nameof(Sse.AddScalar));
            }
            else
            {
                EmitScalarBinaryOpF(Context, () => Context.Emit(OpCodes.Add));
            }
        }

        public static void Fadd_V(AILEmitterCtx Context)
        {
            if (AOptimizations.UseSse && AOptimizations.UseSse2)
            {
                EmitVectorSseOrSse2CallF(Context, nameof(Sse.Add));
            }
            else
            {
                EmitVectorBinaryOpF(Context, () => Context.Emit(OpCodes.Add));
            }
        }

        public static void Faddp_S(AILEmitterCtx Context)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            int SizeF = Op.Size & 1;

            EmitVectorExtractF(Context, Op.Rn, 0, SizeF);
            EmitVectorExtractF(Context, Op.Rn, 1, SizeF);

            Context.Emit(OpCodes.Add);

            EmitScalarSetF(Context, Op.Rd, SizeF);
        }

        public static void Faddp_V(AILEmitterCtx Context)
        {
            AOpCodeSimdReg Op = (AOpCodeSimdReg)Context.CurrOp;

            int SizeF = Op.Size & 1;

            int Bytes = Op.GetBitsCount() >> 3;

            int Elems = Bytes >> SizeF + 2;
            int Half  = Elems >> 1;

            for (int Index = 0; Index < Elems; Index++)
            {
                int Elem = (Index & (Half - 1)) << 1;

                EmitVectorExtractF(Context, Index < Half ? Op.Rn : Op.Rm, Elem + 0, SizeF);
                EmitVectorExtractF(Context, Index < Half ? Op.Rn : Op.Rm, Elem + 1, SizeF);

                Context.Emit(OpCodes.Add);

                EmitVectorInsertTmpF(Context, Index, SizeF);
            }

            Context.EmitLdvectmp();
            Context.EmitStvec(Op.Rd);

            if (Op.RegisterSize == ARegisterSize.SIMD64)
            {
                EmitVectorZeroUpper(Context, Op.Rd);
            }
        }

        public static void Fdiv_S(AILEmitterCtx Context)
        {
            if (AOptimizations.UseSse && AOptimizations.UseSse2)
            {
                EmitScalarSseOrSse2CallF(Context, nameof(Sse.DivideScalar));
            }
            else
            {
                EmitScalarBinaryOpF(Context, () => Context.Emit(OpCodes.Div));
            }
        }

        public static void Fdiv_V(AILEmitterCtx Context)
        {
            if (AOptimizations.UseSse && AOptimizations.UseSse2)
            {
                EmitVectorSseOrSse2CallF(Context, nameof(Sse.Divide));
            }
            else
            {
                EmitVectorBinaryOpF(Context, () => Context.Emit(OpCodes.Div));
            }
        }

        public static void Fmadd_S(AILEmitterCtx Context)
        {
            EmitScalarTernaryRaOpF(Context, () =>
            {
                Context.Emit(OpCodes.Mul);
                Context.Emit(OpCodes.Add);
            });
        }

        public static void Fmax_S(AILEmitterCtx Context)
        {
            EmitScalarBinaryOpF(Context, () =>
            {
                EmitBinarySoftFloatCall(Context, nameof(ASoftFloat.Max));
            });
        }

        public static void Fmax_V(AILEmitterCtx Context)
        {
            EmitVectorBinaryOpF(Context, () =>
            {
                EmitBinarySoftFloatCall(Context, nameof(ASoftFloat.Max));
            });
        }

        public static void Fmaxnm_S(AILEmitterCtx Context)
        {
            EmitScalarBinaryOpF(Context, () =>
            {
                EmitBinarySoftFloatCall(Context, nameof(ASoftFloat.MaxNum));
            });
        }

        public static void Fmaxnm_V(AILEmitterCtx Context)
        {
            EmitVectorBinaryOpF(Context, () =>
            {
                EmitBinarySoftFloatCall(Context, nameof(ASoftFloat.MaxNum));
            });
        }

        public static void Fmin_S(AILEmitterCtx Context)
        {
            EmitScalarBinaryOpF(Context, () =>
            {
                EmitBinarySoftFloatCall(Context, nameof(ASoftFloat.Min));
            });
        }

        public static void Fmin_V(AILEmitterCtx Context)
        {
            EmitVectorBinaryOpF(Context, () =>
            {
                EmitBinarySoftFloatCall(Context, nameof(ASoftFloat.Min));
            });
        }

        public static void Fminnm_S(AILEmitterCtx Context)
        {
            EmitScalarBinaryOpF(Context, () =>
            {
                EmitBinarySoftFloatCall(Context, nameof(ASoftFloat.MinNum));
            });
        }

        public static void Fminnm_V(AILEmitterCtx Context)
        {
            EmitVectorBinaryOpF(Context, () =>
            {
                EmitBinarySoftFloatCall(Context, nameof(ASoftFloat.MinNum));
            });
        }

        public static void Fmla_Se(AILEmitterCtx Context)
        {
            EmitScalarTernaryOpByElemF(Context, () =>
            {
                Context.Emit(OpCodes.Mul);
                Context.Emit(OpCodes.Add);
            });
        }

        public static void Fmla_V(AILEmitterCtx Context)
        {
            EmitVectorTernaryOpF(Context, () =>
            {
                Context.Emit(OpCodes.Mul);
                Context.Emit(OpCodes.Add);
            });
        }

        public static void Fmla_Ve(AILEmitterCtx Context)
        {
            EmitVectorTernaryOpByElemF(Context, () =>
            {
                Context.Emit(OpCodes.Mul);
                Context.Emit(OpCodes.Add);
            });
        }

        public static void Fmls_V(AILEmitterCtx Context)
        {
            EmitVectorTernaryOpF(Context, () =>
            {
                Context.Emit(OpCodes.Mul);
                Context.Emit(OpCodes.Sub);
            });
        }

        public static void Fmls_Ve(AILEmitterCtx Context)
        {
            EmitVectorTernaryOpByElemF(Context, () =>
            {
                Context.Emit(OpCodes.Mul);
                Context.Emit(OpCodes.Sub);
            });
        }

        public static void Fmsub_S(AILEmitterCtx Context)
        {
            EmitScalarTernaryRaOpF(Context, () =>
            {
                Context.Emit(OpCodes.Mul);
                Context.Emit(OpCodes.Sub);
            });
        }

        public static void Fmul_S(AILEmitterCtx Context)
        {
            if (AOptimizations.UseSse && AOptimizations.UseSse2)
            {
                EmitScalarSseOrSse2CallF(Context, nameof(Sse.MultiplyScalar));
            }
            else
            {
                EmitScalarBinaryOpF(Context, () => Context.Emit(OpCodes.Mul));
            }
        }

        public static void Fmul_Se(AILEmitterCtx Context)
        {
            EmitScalarBinaryOpByElemF(Context, () => Context.Emit(OpCodes.Mul));
        }

        public static void Fmul_V(AILEmitterCtx Context)
        {
            if (AOptimizations.UseSse && AOptimizations.UseSse2)
            {
                EmitVectorSseOrSse2CallF(Context, nameof(Sse.Multiply));
            }
            else
            {
                EmitVectorBinaryOpF(Context, () => Context.Emit(OpCodes.Mul));
            }
        }

        public static void Fmul_Ve(AILEmitterCtx Context)
        {
            EmitVectorBinaryOpByElemF(Context, () => Context.Emit(OpCodes.Mul));
        }

        public static void Fneg_S(AILEmitterCtx Context)
        {
            EmitScalarUnaryOpF(Context, () => Context.Emit(OpCodes.Neg));
        }

        public static void Fneg_V(AILEmitterCtx Context)
        {
            EmitVectorUnaryOpF(Context, () => Context.Emit(OpCodes.Neg));
        }

        public static void Fnmadd_S(AILEmitterCtx Context)
        {
            AOpCodeSimdReg Op = (AOpCodeSimdReg)Context.CurrOp;

            int SizeF = Op.Size & 1;

            EmitVectorExtractF(Context, Op.Rn, 0, SizeF);

            Context.Emit(OpCodes.Neg);

            EmitVectorExtractF(Context, Op.Rm, 0, SizeF);

            Context.Emit(OpCodes.Mul);

            EmitVectorExtractF(Context, Op.Ra, 0, SizeF);

            Context.Emit(OpCodes.Sub);

            EmitScalarSetF(Context, Op.Rd, SizeF);
        }

        public static void Fnmsub_S(AILEmitterCtx Context)
        {
            AOpCodeSimdReg Op = (AOpCodeSimdReg)Context.CurrOp;

            int SizeF = Op.Size & 1;

            EmitVectorExtractF(Context, Op.Rn, 0, SizeF);
            EmitVectorExtractF(Context, Op.Rm, 0, SizeF);

            Context.Emit(OpCodes.Mul);

            EmitVectorExtractF(Context, Op.Ra, 0, SizeF);

            Context.Emit(OpCodes.Sub);

            EmitScalarSetF(Context, Op.Rd, SizeF);
        }

        public static void Fnmul_S(AILEmitterCtx Context)
        {
            EmitScalarBinaryOpF(Context, () =>
            {
                Context.Emit(OpCodes.Mul);
                Context.Emit(OpCodes.Neg);
            });
        }

        public static void Frecpe_S(AILEmitterCtx Context)
        {
            EmitScalarUnaryOpF(Context, () =>
            {
                EmitUnarySoftFloatCall(Context, nameof(ASoftFloat.RecipEstimate));
            });
        }

        public static void Frecpe_V(AILEmitterCtx Context)
        {
            EmitVectorUnaryOpF(Context, () =>
            {
                EmitUnarySoftFloatCall(Context, nameof(ASoftFloat.RecipEstimate));
            });
        }

        public static void Frecps_S(AILEmitterCtx Context)
        {
            EmitScalarBinaryOpF(Context, () =>
            {
                EmitBinarySoftFloatCall(Context, nameof(ASoftFloat.RecipStep));
            });
        }

        public static void Frecps_V(AILEmitterCtx Context)
        {
            EmitVectorBinaryOpF(Context, () =>
            {
                EmitBinarySoftFloatCall(Context, nameof(ASoftFloat.RecipStep));
            });
        }

        public static void Frinta_S(AILEmitterCtx Context)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            EmitVectorExtractF(Context, Op.Rn, 0, Op.Size);

            EmitRoundMathCall(Context, MidpointRounding.AwayFromZero);

            EmitScalarSetF(Context, Op.Rd, Op.Size);
        }

        public static void Frinta_V(AILEmitterCtx Context)
        {
            EmitVectorUnaryOpF(Context, () =>
            {
                EmitRoundMathCall(Context, MidpointRounding.AwayFromZero);
            });
        }

        public static void Frinti_S(AILEmitterCtx Context)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            EmitScalarUnaryOpF(Context, () =>
            {
                Context.EmitLdarg(ATranslatedSub.StateArgIdx);

                Context.EmitCallPropGet(typeof(AThreadState), nameof(AThreadState.Fpcr));

                if (Op.Size == 0)
                {
                    AVectorHelper.EmitCall(Context, nameof(AVectorHelper.RoundF));
                }
                else if (Op.Size == 1)
                {
                    AVectorHelper.EmitCall(Context, nameof(AVectorHelper.Round));
                }
                else
                {
                    throw new InvalidOperationException();
                }
            });
        }

        public static void Frinti_V(AILEmitterCtx Context)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            int SizeF = Op.Size & 1;

            EmitVectorUnaryOpF(Context, () =>
            {
                Context.EmitLdarg(ATranslatedSub.StateArgIdx);

                Context.EmitCallPropGet(typeof(AThreadState), nameof(AThreadState.Fpcr));

                if (SizeF == 0)
                {
                    AVectorHelper.EmitCall(Context, nameof(AVectorHelper.RoundF));
                }
                else if (SizeF == 1)
                {
                    AVectorHelper.EmitCall(Context, nameof(AVectorHelper.Round));
                }
                else
                {
                    throw new InvalidOperationException();
                }
            });
        }

        public static void Frintm_S(AILEmitterCtx Context)
        {
            EmitScalarUnaryOpF(Context, () =>
            {
                EmitUnaryMathCall(Context, nameof(Math.Floor));
            });
        }

        public static void Frintm_V(AILEmitterCtx Context)
        {
            EmitVectorUnaryOpF(Context, () =>
            {
                EmitUnaryMathCall(Context, nameof(Math.Floor));
            });
        }

        public static void Frintn_S(AILEmitterCtx Context)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            EmitVectorExtractF(Context, Op.Rn, 0, Op.Size);

            EmitRoundMathCall(Context, MidpointRounding.ToEven);

            EmitScalarSetF(Context, Op.Rd, Op.Size);
        }

        public static void Frintn_V(AILEmitterCtx Context)
        {
            EmitVectorUnaryOpF(Context, () =>
            {
                EmitRoundMathCall(Context, MidpointRounding.ToEven);
            });
        }

        public static void Frintp_S(AILEmitterCtx Context)
        {
            EmitScalarUnaryOpF(Context, () =>
            {
                EmitUnaryMathCall(Context, nameof(Math.Ceiling));
            });
        }

        public static void Frintp_V(AILEmitterCtx Context)
        {
            EmitVectorUnaryOpF(Context, () =>
            {
                EmitUnaryMathCall(Context, nameof(Math.Ceiling));
            });
        }

        public static void Frintx_S(AILEmitterCtx Context)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            EmitScalarUnaryOpF(Context, () =>
            {
                Context.EmitLdarg(ATranslatedSub.StateArgIdx);

                Context.EmitCallPropGet(typeof(AThreadState), nameof(AThreadState.Fpcr));

                if (Op.Size == 0)
                {
                    AVectorHelper.EmitCall(Context, nameof(AVectorHelper.RoundF));
                }
                else if (Op.Size == 1)
                {
                    AVectorHelper.EmitCall(Context, nameof(AVectorHelper.Round));
                }
                else
                {
                    throw new InvalidOperationException();
                }
            });
        }

        public static void Frintx_V(AILEmitterCtx Context)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            EmitVectorUnaryOpF(Context, () =>
            {
                Context.EmitLdarg(ATranslatedSub.StateArgIdx);

                Context.EmitCallPropGet(typeof(AThreadState), nameof(AThreadState.Fpcr));

                if (Op.Size == 0)
                {
                    AVectorHelper.EmitCall(Context, nameof(AVectorHelper.RoundF));
                }
                else if (Op.Size == 1)
                {
                    AVectorHelper.EmitCall(Context, nameof(AVectorHelper.Round));
                }
                else
                {
                    throw new InvalidOperationException();
                }
            });
        }

        public static void Frsqrte_S(AILEmitterCtx Context)
        {
            EmitScalarUnaryOpF(Context, () =>
            {
                EmitUnarySoftFloatCall(Context, nameof(ASoftFloat.InvSqrtEstimate));
            });
        }

        public static void Frsqrte_V(AILEmitterCtx Context)
        {
            EmitVectorUnaryOpF(Context, () =>
            {
                EmitUnarySoftFloatCall(Context, nameof(ASoftFloat.InvSqrtEstimate));
            });
        }

        public static void Frsqrts_S(AILEmitterCtx Context)
        {
            EmitFrsqrts(Context, 0, Scalar: true);
        }

        public static void Frsqrts_V(AILEmitterCtx Context)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            int SizeF = Op.Size & 1;

            int Bytes = Op.GetBitsCount() >> 3;

            for (int Index = 0; Index < Bytes >> SizeF + 2; Index++)
            {
                EmitFrsqrts(Context, Index, Scalar: false);
            }

            if (Op.RegisterSize == ARegisterSize.SIMD64)
            {
                EmitVectorZeroUpper(Context, Op.Rd);
            }
        }

        private static void EmitFrsqrts(AILEmitterCtx Context, int Index, bool Scalar)
        {
            AOpCodeSimdReg Op = (AOpCodeSimdReg)Context.CurrOp;

            int SizeF = Op.Size & 1;

            if (SizeF == 0)
            {
                Context.EmitLdc_R4(3);
            }
            else /* if (SizeF == 1) */
            {
                Context.EmitLdc_R8(3);
            }

            EmitVectorExtractF(Context, Op.Rn, Index, SizeF);
            EmitVectorExtractF(Context, Op.Rm, Index, SizeF);

            Context.Emit(OpCodes.Mul);
            Context.Emit(OpCodes.Sub);

            if (SizeF == 0)
            {
                Context.EmitLdc_R4(0.5f);
            }
            else /* if (SizeF == 1) */
            {
                Context.EmitLdc_R8(0.5);
            }

            Context.Emit(OpCodes.Mul);

            if (Scalar)
            {
                EmitVectorZeroAll(Context, Op.Rd);
            }

            EmitVectorInsertF(Context, Op.Rd, Index, SizeF);
        }

        public static void Fsqrt_S(AILEmitterCtx Context)
        {
            EmitScalarUnaryOpF(Context, () =>
            {
                EmitUnaryMathCall(Context, nameof(Math.Sqrt));
            });
        }

        public static void Fsub_S(AILEmitterCtx Context)
        {
            if (AOptimizations.UseSse && AOptimizations.UseSse2)
            {
                EmitScalarSseOrSse2CallF(Context, nameof(Sse.SubtractScalar));
            }
            else
            {
                EmitScalarBinaryOpF(Context, () => Context.Emit(OpCodes.Sub));
            }
        }

        public static void Fsub_V(AILEmitterCtx Context)
        {
            if (AOptimizations.UseSse && AOptimizations.UseSse2)
            {
                EmitVectorSseOrSse2CallF(Context, nameof(Sse.Subtract));
            }
            else
            {
                EmitVectorBinaryOpF(Context, () => Context.Emit(OpCodes.Sub));
            }
        }

        public static void Mla_V(AILEmitterCtx Context)
        {
            EmitVectorTernaryOpZx(Context, () =>
            {
                Context.Emit(OpCodes.Mul);
                Context.Emit(OpCodes.Add);
            });
        }

        public static void Mla_Ve(AILEmitterCtx Context)
        {
            EmitVectorTernaryOpByElemZx(Context, () =>
            {
                Context.Emit(OpCodes.Mul);
                Context.Emit(OpCodes.Add);
            });
        }

        public static void Mls_V(AILEmitterCtx Context)
        {
            EmitVectorTernaryOpZx(Context, () =>
            {
                Context.Emit(OpCodes.Mul);
                Context.Emit(OpCodes.Sub);
            });
        }

        public static void Mul_V(AILEmitterCtx Context)
        {
            EmitVectorBinaryOpZx(Context, () => Context.Emit(OpCodes.Mul));
        }

        public static void Mul_Ve(AILEmitterCtx Context)
        {
            EmitVectorBinaryOpByElemZx(Context, () => Context.Emit(OpCodes.Mul));
        }

        public static void Neg_S(AILEmitterCtx Context)
        {
            EmitScalarUnaryOpSx(Context, () => Context.Emit(OpCodes.Neg));
        }

        public static void Neg_V(AILEmitterCtx Context)
        {
            EmitVectorUnaryOpSx(Context, () => Context.Emit(OpCodes.Neg));
        }

        public static void Raddhn_V(AILEmitterCtx Context)
        {
            EmitHighNarrow(Context, () => Context.Emit(OpCodes.Add), Round: true);
        }

        public static void Rsubhn_V(AILEmitterCtx Context)
        {
            EmitHighNarrow(Context, () => Context.Emit(OpCodes.Sub), Round: true);
        }

        public static void Saba_V(AILEmitterCtx Context)
        {
            EmitVectorTernaryOpSx(Context, () =>
            {
                Context.Emit(OpCodes.Sub);
                EmitAbs(Context);

                Context.Emit(OpCodes.Add);
            });
        }

        public static void Sabal_V(AILEmitterCtx Context)
        {
            EmitVectorWidenRnRmTernaryOpSx(Context, () =>
            {
                Context.Emit(OpCodes.Sub);
                EmitAbs(Context);

                Context.Emit(OpCodes.Add);
            });
        }

        public static void Sabd_V(AILEmitterCtx Context)
        {
            EmitVectorBinaryOpSx(Context, () =>
            {
                Context.Emit(OpCodes.Sub);
                EmitAbs(Context);
            });
        }

        public static void Sabdl_V(AILEmitterCtx Context)
        {
            EmitVectorWidenRnRmBinaryOpSx(Context, () =>
            {
                Context.Emit(OpCodes.Sub);
                EmitAbs(Context);
            });
        }

        public static void Sadalp_V(AILEmitterCtx Context)
        {
            EmitAddLongPairwise(Context, Signed: true, Accumulate: true);
        }

        public static void Saddl_V(AILEmitterCtx Context)
        {
            EmitVectorWidenRnRmBinaryOpSx(Context, () => Context.Emit(OpCodes.Add));
        }

        public static void Saddlp_V(AILEmitterCtx Context)
        {
            EmitAddLongPairwise(Context, Signed: true, Accumulate: false);
        }

        public static void Saddw_V(AILEmitterCtx Context)
        {
            EmitVectorWidenRmBinaryOpSx(Context, () => Context.Emit(OpCodes.Add));
        }

        public static void Shadd_V(AILEmitterCtx Context)
        {
            EmitVectorBinaryOpSx(Context, () =>
            {
                Context.Emit(OpCodes.Add);

                Context.Emit(OpCodes.Ldc_I4_1);
                Context.Emit(OpCodes.Shr);
            });
        }

        public static void Shsub_V(AILEmitterCtx Context)
        {
            EmitVectorBinaryOpSx(Context, () =>
            {
                Context.Emit(OpCodes.Sub);

                Context.Emit(OpCodes.Ldc_I4_1);
                Context.Emit(OpCodes.Shr);
            });
        }

        public static void Smax_V(AILEmitterCtx Context)
        {
            Type[] Types = new Type[] { typeof(long), typeof(long) };

            MethodInfo MthdInfo = typeof(Math).GetMethod(nameof(Math.Max), Types);

            EmitVectorBinaryOpSx(Context, () => Context.EmitCall(MthdInfo));
        }

        public static void Smaxp_V(AILEmitterCtx Context)
        {
            Type[] Types = new Type[] { typeof(long), typeof(long) };

            MethodInfo MthdInfo = typeof(Math).GetMethod(nameof(Math.Max), Types);

            EmitVectorPairwiseOpSx(Context, () => Context.EmitCall(MthdInfo));
        }

        public static void Smin_V(AILEmitterCtx Context)
        {
            Type[] Types = new Type[] { typeof(long), typeof(long) };

            MethodInfo MthdInfo = typeof(Math).GetMethod(nameof(Math.Min), Types);

            EmitVectorBinaryOpSx(Context, () => Context.EmitCall(MthdInfo));
        }

        public static void Sminp_V(AILEmitterCtx Context)
        {
            Type[] Types = new Type[] { typeof(long), typeof(long) };

            MethodInfo MthdInfo = typeof(Math).GetMethod(nameof(Math.Min), Types);

            EmitVectorPairwiseOpSx(Context, () => Context.EmitCall(MthdInfo));
        }

        public static void Smlal_V(AILEmitterCtx Context)
        {
            EmitVectorWidenRnRmTernaryOpSx(Context, () =>
            {
                Context.Emit(OpCodes.Mul);
                Context.Emit(OpCodes.Add);
            });
        }

        public static void Smlsl_V(AILEmitterCtx Context)
        {
            EmitVectorWidenRnRmTernaryOpSx(Context, () =>
            {
                Context.Emit(OpCodes.Mul);
                Context.Emit(OpCodes.Sub);
            });
        }

        public static void Smull_V(AILEmitterCtx Context)
        {
            EmitVectorWidenRnRmBinaryOpSx(Context, () => Context.Emit(OpCodes.Mul));
        }

        public static void Sqabs_S(AILEmitterCtx Context)
        {
            EmitScalarSaturatingUnaryOpSx(Context, () => EmitAbs(Context));
        }

        public static void Sqabs_V(AILEmitterCtx Context)
        {
            EmitVectorSaturatingUnaryOpSx(Context, () => EmitAbs(Context));
        }

        public static void Sqadd_S(AILEmitterCtx Context)
        {
            EmitScalarSaturatingBinaryOpSx(Context, SaturatingFlags.Add);
        }

        public static void Sqadd_V(AILEmitterCtx Context)
        {
            EmitVectorSaturatingBinaryOpSx(Context, SaturatingFlags.Add);
        }

        public static void Sqdmulh_S(AILEmitterCtx Context)
        {
            EmitSaturatingBinaryOp(Context, () => EmitDoublingMultiplyHighHalf(Context, Round: false), SaturatingFlags.ScalarSx);
        }

        public static void Sqdmulh_V(AILEmitterCtx Context)
        {
            EmitSaturatingBinaryOp(Context, () => EmitDoublingMultiplyHighHalf(Context, Round: false), SaturatingFlags.VectorSx);
        }

        public static void Sqneg_S(AILEmitterCtx Context)
        {
            EmitScalarSaturatingUnaryOpSx(Context, () => Context.Emit(OpCodes.Neg));
        }

        public static void Sqneg_V(AILEmitterCtx Context)
        {
            EmitVectorSaturatingUnaryOpSx(Context, () => Context.Emit(OpCodes.Neg));
        }

        public static void Sqrdmulh_S(AILEmitterCtx Context)
        {
            EmitSaturatingBinaryOp(Context, () => EmitDoublingMultiplyHighHalf(Context, Round: true), SaturatingFlags.ScalarSx);
        }

        public static void Sqrdmulh_V(AILEmitterCtx Context)
        {
            EmitSaturatingBinaryOp(Context, () => EmitDoublingMultiplyHighHalf(Context, Round: true), SaturatingFlags.VectorSx);
        }

        public static void Sqsub_S(AILEmitterCtx Context)
        {
            EmitScalarSaturatingBinaryOpSx(Context, SaturatingFlags.Sub);
        }

        public static void Sqsub_V(AILEmitterCtx Context)
        {
            EmitVectorSaturatingBinaryOpSx(Context, SaturatingFlags.Sub);
        }

        public static void Sqxtn_S(AILEmitterCtx Context)
        {
            EmitScalarSaturatingNarrowOpSxSx(Context, () => { });
        }

        public static void Sqxtn_V(AILEmitterCtx Context)
        {
            EmitVectorSaturatingNarrowOpSxSx(Context, () => { });
        }

        public static void Sqxtun_S(AILEmitterCtx Context)
        {
            EmitScalarSaturatingNarrowOpSxZx(Context, () => { });
        }

        public static void Sqxtun_V(AILEmitterCtx Context)
        {
            EmitVectorSaturatingNarrowOpSxZx(Context, () => { });
        }

        public static void Srhadd_V(AILEmitterCtx Context)
        {
            EmitVectorBinaryOpSx(Context, () =>
            {
                Context.Emit(OpCodes.Add);

                Context.Emit(OpCodes.Ldc_I4_1);
                Context.Emit(OpCodes.Add);

                Context.Emit(OpCodes.Ldc_I4_1);
                Context.Emit(OpCodes.Shr);
            });
        }

        public static void Ssubl_V(AILEmitterCtx Context)
        {
            EmitVectorWidenRnRmBinaryOpSx(Context, () => Context.Emit(OpCodes.Sub));
        }

        public static void Ssubw_V(AILEmitterCtx Context)
        {
            EmitVectorWidenRmBinaryOpSx(Context, () => Context.Emit(OpCodes.Sub));
        }

        public static void Sub_S(AILEmitterCtx Context)
        {
            EmitScalarBinaryOpZx(Context, () => Context.Emit(OpCodes.Sub));
        }

        public static void Sub_V(AILEmitterCtx Context)
        {
            if (AOptimizations.UseSse2)
            {
                EmitSse2Call(Context, nameof(Sse2.Subtract));
            }
            else
            {
                EmitVectorBinaryOpZx(Context, () => Context.Emit(OpCodes.Sub));
            }
        }

        public static void Subhn_V(AILEmitterCtx Context)
        {
            EmitHighNarrow(Context, () => Context.Emit(OpCodes.Sub), Round: false);
        }

        public static void Suqadd_S(AILEmitterCtx Context)
        {
            EmitScalarSaturatingBinaryOpSx(Context, SaturatingFlags.Accumulate);
        }

        public static void Suqadd_V(AILEmitterCtx Context)
        {
            EmitVectorSaturatingBinaryOpSx(Context, SaturatingFlags.Accumulate);
        }

        public static void Uaba_V(AILEmitterCtx Context)
        {
            EmitVectorTernaryOpZx(Context, () =>
            {
                Context.Emit(OpCodes.Sub);
                EmitAbs(Context);

                Context.Emit(OpCodes.Add);
            });
        }

        public static void Uabal_V(AILEmitterCtx Context)
        {
            EmitVectorWidenRnRmTernaryOpZx(Context, () =>
            {
                Context.Emit(OpCodes.Sub);
                EmitAbs(Context);

                Context.Emit(OpCodes.Add);
            });
        }

        public static void Uabd_V(AILEmitterCtx Context)
        {
            EmitVectorBinaryOpZx(Context, () =>
            {
                Context.Emit(OpCodes.Sub);
                EmitAbs(Context);
            });
        }

        public static void Uabdl_V(AILEmitterCtx Context)
        {
            EmitVectorWidenRnRmBinaryOpZx(Context, () =>
            {
                Context.Emit(OpCodes.Sub);
                EmitAbs(Context);
            });
        }

        public static void Uadalp_V(AILEmitterCtx Context)
        {
            EmitAddLongPairwise(Context, Signed: false, Accumulate: true);
        }

        public static void Uaddl_V(AILEmitterCtx Context)
        {
            EmitVectorWidenRnRmBinaryOpZx(Context, () => Context.Emit(OpCodes.Add));
        }

        public static void Uaddlp_V(AILEmitterCtx Context)
        {
            EmitAddLongPairwise(Context, Signed: false, Accumulate: false);
        }

        public static void Uaddlv_V(AILEmitterCtx Context)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            int Bytes = Op.GetBitsCount() >> 3;
            int Elems = Bytes >> Op.Size;

            EmitVectorExtractZx(Context, Op.Rn, 0, Op.Size);

            for (int Index = 1; Index < Elems; Index++)
            {
                EmitVectorExtractZx(Context, Op.Rn, Index, Op.Size);

                Context.Emit(OpCodes.Add);
            }

            EmitScalarSet(Context, Op.Rd, Op.Size + 1);
        }

        public static void Uaddw_V(AILEmitterCtx Context)
        {
            EmitVectorWidenRmBinaryOpZx(Context, () => Context.Emit(OpCodes.Add));
        }

        public static void Uhadd_V(AILEmitterCtx Context)
        {
            EmitVectorBinaryOpZx(Context, () =>
            {
                Context.Emit(OpCodes.Add);

                Context.Emit(OpCodes.Ldc_I4_1);
                Context.Emit(OpCodes.Shr_Un);
            });
        }

        public static void Uhsub_V(AILEmitterCtx Context)
        {
            EmitVectorBinaryOpZx(Context, () =>
            {
                Context.Emit(OpCodes.Sub);

                Context.Emit(OpCodes.Ldc_I4_1);
                Context.Emit(OpCodes.Shr_Un);
            });
        }

        public static void Umax_V(AILEmitterCtx Context)
        {
            Type[] Types = new Type[] { typeof(ulong), typeof(ulong) };

            MethodInfo MthdInfo = typeof(Math).GetMethod(nameof(Math.Max), Types);

            EmitVectorBinaryOpZx(Context, () => Context.EmitCall(MthdInfo));
        }

        public static void Umaxp_V(AILEmitterCtx Context)
        {
            Type[] Types = new Type[] { typeof(ulong), typeof(ulong) };

            MethodInfo MthdInfo = typeof(Math).GetMethod(nameof(Math.Max), Types);

            EmitVectorPairwiseOpZx(Context, () => Context.EmitCall(MthdInfo));
        }

        public static void Umin_V(AILEmitterCtx Context)
        {
            Type[] Types = new Type[] { typeof(ulong), typeof(ulong) };

            MethodInfo MthdInfo = typeof(Math).GetMethod(nameof(Math.Min), Types);

            EmitVectorBinaryOpZx(Context, () => Context.EmitCall(MthdInfo));
        }

        public static void Uminp_V(AILEmitterCtx Context)
        {
            Type[] Types = new Type[] { typeof(ulong), typeof(ulong) };

            MethodInfo MthdInfo = typeof(Math).GetMethod(nameof(Math.Min), Types);

            EmitVectorPairwiseOpZx(Context, () => Context.EmitCall(MthdInfo));
        }

        public static void Umlal_V(AILEmitterCtx Context)
        {
            EmitVectorWidenRnRmTernaryOpZx(Context, () =>
            {
                Context.Emit(OpCodes.Mul);
                Context.Emit(OpCodes.Add);
            });
        }

        public static void Umlsl_V(AILEmitterCtx Context)
        {
            EmitVectorWidenRnRmTernaryOpZx(Context, () =>
            {
                Context.Emit(OpCodes.Mul);
                Context.Emit(OpCodes.Sub);
            });
        }

        public static void Umull_V(AILEmitterCtx Context)
        {
            EmitVectorWidenRnRmBinaryOpZx(Context, () => Context.Emit(OpCodes.Mul));
        }

        public static void Uqadd_S(AILEmitterCtx Context)
        {
            EmitScalarSaturatingBinaryOpZx(Context, SaturatingFlags.Add);
        }

        public static void Uqadd_V(AILEmitterCtx Context)
        {
            EmitVectorSaturatingBinaryOpZx(Context, SaturatingFlags.Add);
        }

        public static void Uqsub_S(AILEmitterCtx Context)
        {
            EmitScalarSaturatingBinaryOpZx(Context, SaturatingFlags.Sub);
        }

        public static void Uqsub_V(AILEmitterCtx Context)
        {
            EmitVectorSaturatingBinaryOpZx(Context, SaturatingFlags.Sub);
        }

        public static void Uqxtn_S(AILEmitterCtx Context)
        {
            EmitScalarSaturatingNarrowOpZxZx(Context, () => { });
        }

        public static void Uqxtn_V(AILEmitterCtx Context)
        {
            EmitVectorSaturatingNarrowOpZxZx(Context, () => { });
        }

        public static void Urhadd_V(AILEmitterCtx Context)
        {
            EmitVectorBinaryOpZx(Context, () =>
            {
                Context.Emit(OpCodes.Add);

                Context.Emit(OpCodes.Ldc_I4_1);
                Context.Emit(OpCodes.Add);

                Context.Emit(OpCodes.Ldc_I4_1);
                Context.Emit(OpCodes.Shr_Un);
            });
        }

        public static void Usqadd_S(AILEmitterCtx Context)
        {
            EmitScalarSaturatingBinaryOpZx(Context, SaturatingFlags.Accumulate);
        }

        public static void Usqadd_V(AILEmitterCtx Context)
        {
            EmitVectorSaturatingBinaryOpZx(Context, SaturatingFlags.Accumulate);
        }

        public static void Usubl_V(AILEmitterCtx Context)
        {
            EmitVectorWidenRnRmBinaryOpZx(Context, () => Context.Emit(OpCodes.Sub));
        }

        public static void Usubw_V(AILEmitterCtx Context)
        {
            EmitVectorWidenRmBinaryOpZx(Context, () => Context.Emit(OpCodes.Sub));
        }
    }
}
