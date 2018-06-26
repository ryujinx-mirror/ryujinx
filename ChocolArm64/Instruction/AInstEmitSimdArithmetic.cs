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

        private static void EmitAbs(AILEmitterCtx Context)
        {
            AILLabel LblTrue = new AILLabel();

            Context.Emit(OpCodes.Dup);
            Context.Emit(OpCodes.Ldc_I4_0);
            Context.Emit(OpCodes.Bge_S, LblTrue);

            Context.Emit(OpCodes.Neg);

            Context.MarkLabel(LblTrue);
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
            AOpCodeSimdReg Op = (AOpCodeSimdReg)Context.CurrOp;

            int Bytes = Context.CurrOp.GetBitsCount() >> 3;

            int Elems = Bytes >> Op.Size;
            int Half  = Elems >> 1;

            for (int Index = 0; Index < Elems; Index++)
            {
                int Elem = (Index & (Half - 1)) << 1;

                EmitVectorExtractZx(Context, Index < Half ? Op.Rn : Op.Rm, Elem + 0, Op.Size);
                EmitVectorExtractZx(Context, Index < Half ? Op.Rn : Op.Rm, Elem + 1, Op.Size);

                Context.Emit(OpCodes.Add);

                EmitVectorInsertTmp(Context, Index, Op.Size);
            }

            Context.EmitLdvectmp();
            Context.EmitStvec(Op.Rd);

            if (Op.RegisterSize == ARegisterSize.SIMD64)
            {
                EmitVectorZeroUpper(Context, Op.Rd);
            }
        }

        public static void Addv_V(AILEmitterCtx Context)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            int Bytes = Context.CurrOp.GetBitsCount() >> 3;

            EmitVectorExtractZx(Context, Op.Rn, 0, Op.Size);

            for (int Index = 1; Index < (Bytes >> Op.Size); Index++)
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

            int Bytes = Context.CurrOp.GetBitsCount() >> 3;

            for (int Index = 0; Index < (Bytes >> Op.Size); Index++)
            {
                EmitVectorExtractZx(Context, Op.Rn, Index, Op.Size);

                Context.EmitLdc_I4(8 << Op.Size);

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

                Context.Emit(OpCodes.Conv_U1);

                AVectorHelper.EmitCall(Context, nameof(AVectorHelper.CountSetBits8));

                Context.Emit(OpCodes.Conv_U8);

                EmitVectorInsert(Context, Op.Rd, Index, 0);
            }

            if (Op.RegisterSize == ARegisterSize.SIMD64)
            {
                EmitVectorZeroUpper(Context, Op.Rd);
            }
        }

        private static void EmitHighNarrow(AILEmitterCtx Context, Action Emit, bool Round)
        {
            AOpCodeSimdReg Op = (AOpCodeSimdReg)Context.CurrOp;

            int Elems = 8 >> Op.Size;
            int ESize = 8 << Op.Size;

            int Part = Op.RegisterSize == ARegisterSize.SIMD128 ? Elems : 0;

            for (int Index = 0; Index < Elems; Index++)
            {
                EmitVectorExtractZx(Context, Op.Rn, Index, Op.Size + 1);
                EmitVectorExtractZx(Context, Op.Rm, Index, Op.Size + 1);

                Emit();

                if (Round)
                {
                    Context.EmitLdc_I8(1L << (ESize - 1));

                    Context.Emit(OpCodes.Add);
                }

                Context.EmitLsr(ESize);

                EmitVectorInsert(Context, Op.Rd, Part + Index, Op.Size);
            }

            if (Part == 0)
            {
                EmitVectorZeroUpper(Context, Op.Rd);
            }
        }

        private static void EmitSaturatingExtNarrow(AILEmitterCtx Context, bool SignedSrc, bool SignedDst, bool Scalar)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            int Elems = (!Scalar ? 8 >> Op.Size : 1);
            int ESize = 8 << Op.Size;

            int TMaxValue = (SignedDst ? (1 << (ESize - 1)) - 1 : (int)((1L << ESize) - 1L));
            int TMinValue = (SignedDst ? -((1 << (ESize - 1))) : 0);

            int Part = (!Scalar & (Op.RegisterSize == ARegisterSize.SIMD128) ? Elems : 0);

            Context.EmitLdc_I8(0L);
            Context.EmitSttmp();

            for (int Index = 0; Index < Elems; Index++)
            {
                AILLabel LblLe    = new AILLabel();
                AILLabel LblGeEnd = new AILLabel();

                EmitVectorExtract(Context, Op.Rn, Index, Op.Size + 1, SignedSrc);

                Context.Emit(OpCodes.Dup);

                Context.EmitLdc_I4(TMaxValue);
                Context.Emit(OpCodes.Conv_U8);

                Context.Emit(SignedSrc ? OpCodes.Ble_S : OpCodes.Ble_Un_S, LblLe);

                Context.Emit(OpCodes.Pop);

                Context.EmitLdc_I4(TMaxValue);

                Context.EmitLdc_I8(0x8000000L);
                Context.EmitSttmp();

                Context.Emit(OpCodes.Br_S, LblGeEnd);

                Context.MarkLabel(LblLe);

                Context.Emit(OpCodes.Dup);

                Context.EmitLdc_I4(TMinValue);
                Context.Emit(OpCodes.Conv_I8);

                Context.Emit(SignedSrc ? OpCodes.Bge_S : OpCodes.Bge_Un_S, LblGeEnd);

                Context.Emit(OpCodes.Pop);

                Context.EmitLdc_I4(TMinValue);

                Context.EmitLdc_I8(0x8000000L);
                Context.EmitSttmp();

                Context.MarkLabel(LblGeEnd);

                if (Scalar)
                {
                    EmitVectorZeroLower(Context, Op.Rd);
                }

                EmitVectorInsert(Context, Op.Rd, Part + Index, Op.Size);
            }

            if (Part == 0)
            {
                EmitVectorZeroUpper(Context, Op.Rd);
            }

            Context.EmitLdarg(ATranslatedSub.StateArgIdx);
            Context.EmitLdarg(ATranslatedSub.StateArgIdx);
            Context.EmitCallPropGet(typeof(AThreadState), nameof(AThreadState.Fpsr));
            Context.EmitLdtmp();
            Context.Emit(OpCodes.Conv_I4);
            Context.Emit(OpCodes.Or);
            Context.EmitCallPropSet(typeof(AThreadState), nameof(AThreadState.Fpsr));
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
                EmitSseOrSse2CallF(Context, nameof(Sse.AddScalar));
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
                EmitSseOrSse2CallF(Context, nameof(Sse.Add));
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

            int Bytes = Context.CurrOp.GetBitsCount() >> 3;

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
                EmitSseOrSse2CallF(Context, nameof(Sse.DivideScalar));
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
                EmitSseOrSse2CallF(Context, nameof(Sse.Divide));
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
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            EmitScalarBinaryOpF(Context, () =>
            {
                if (Op.Size == 0)
                {
                    AVectorHelper.EmitCall(Context, nameof(AVectorHelper.MaxF));
                }
                else if (Op.Size == 1)
                {
                    AVectorHelper.EmitCall(Context, nameof(AVectorHelper.Max));
                }
                else
                {
                    throw new InvalidOperationException();
                }
            });
        }

        public static void Fmax_V(AILEmitterCtx Context)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            EmitVectorBinaryOpF(Context, () =>
            {
                if (Op.Size == 0)
                {
                    AVectorHelper.EmitCall(Context, nameof(AVectorHelper.MaxF));
                }
                else if (Op.Size == 1)
                {
                    AVectorHelper.EmitCall(Context, nameof(AVectorHelper.Max));
                }
                else
                {
                    throw new InvalidOperationException();
                }
            });
        }

        public static void Fmin_S(AILEmitterCtx Context)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            EmitScalarBinaryOpF(Context, () =>
            {
                if (Op.Size == 0)
                {
                    AVectorHelper.EmitCall(Context, nameof(AVectorHelper.MinF));
                }
                else if (Op.Size == 1)
                {
                    AVectorHelper.EmitCall(Context, nameof(AVectorHelper.Min));
                }
                else
                {
                    throw new InvalidOperationException();
                }
            });
        }

        public static void Fmin_V(AILEmitterCtx Context)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            int SizeF = Op.Size & 1;

            EmitVectorBinaryOpF(Context, () =>
            {
                if (SizeF == 0)
                {
                    AVectorHelper.EmitCall(Context, nameof(AVectorHelper.MinF));
                }
                else if (SizeF == 1)
                {
                    AVectorHelper.EmitCall(Context, nameof(AVectorHelper.Min));
                }
                else
                {
                    throw new InvalidOperationException();
                }
            });
        }

        public static void Fmaxnm_S(AILEmitterCtx Context)
        {
            Fmax_S(Context);
        }

        public static void Fminnm_S(AILEmitterCtx Context)
        {
            Fmin_S(Context);
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
                EmitSseOrSse2CallF(Context, nameof(Sse.MultiplyScalar));
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
                EmitSseOrSse2CallF(Context, nameof(Sse.Multiply));
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
            EmitFrecpe(Context, 0, Scalar: true);
        }

        public static void Frecpe_V(AILEmitterCtx Context)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            int SizeF = Op.Size & 1;

            int Bytes = Context.CurrOp.GetBitsCount() >> 3;

            for (int Index = 0; Index < Bytes >> SizeF + 2; Index++)
            {
                EmitFrecpe(Context, Index, Scalar: false);
            }

            if (Op.RegisterSize == ARegisterSize.SIMD64)
            {
                EmitVectorZeroUpper(Context, Op.Rd);
            }
        }

        private static void EmitFrecpe(AILEmitterCtx Context, int Index, bool Scalar)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            int SizeF = Op.Size & 1;

            if (SizeF == 0)
            {
                Context.EmitLdc_R4(1);
            }
            else /* if (SizeF == 1) */
            {
                Context.EmitLdc_R8(1);
            }

            EmitVectorExtractF(Context, Op.Rn, Index, SizeF);

            Context.Emit(OpCodes.Div);

            if (Scalar)
            {
                EmitVectorZeroAll(Context, Op.Rd);
            }

            EmitVectorInsertF(Context, Op.Rd, Index, SizeF);
        }

        public static void Frecps_S(AILEmitterCtx Context)
        {
            EmitFrecps(Context, 0, Scalar: true);
        }

        public static void Frecps_V(AILEmitterCtx Context)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            int SizeF = Op.Size & 1;

            int Bytes = Context.CurrOp.GetBitsCount() >> 3;

            for (int Index = 0; Index < Bytes >> SizeF + 2; Index++)
            {
                EmitFrecps(Context, Index, Scalar: false);
            }

            if (Op.RegisterSize == ARegisterSize.SIMD64)
            {
                EmitVectorZeroUpper(Context, Op.Rd);
            }
        }

        private static void EmitFrecps(AILEmitterCtx Context, int Index, bool Scalar)
        {
            AOpCodeSimdReg Op = (AOpCodeSimdReg)Context.CurrOp;

            int SizeF = Op.Size & 1;

            if (SizeF == 0)
            {
                Context.EmitLdc_R4(2);
            }
            else /* if (SizeF == 1) */
            {
                Context.EmitLdc_R8(2);
            }

            EmitVectorExtractF(Context, Op.Rn, Index, SizeF);
            EmitVectorExtractF(Context, Op.Rm, Index, SizeF);

            Context.Emit(OpCodes.Mul);
            Context.Emit(OpCodes.Sub);

            if (Scalar)
            {
                EmitVectorZeroAll(Context, Op.Rd);
            }

            EmitVectorInsertF(Context, Op.Rd, Index, SizeF);
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

            int Bytes = Context.CurrOp.GetBitsCount() >> 3;

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
                EmitSseOrSse2CallF(Context, nameof(Sse.SubtractScalar));
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
                EmitSseOrSse2CallF(Context, nameof(Sse.Subtract));
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

        public static void Saddw_V(AILEmitterCtx Context)
        {
            EmitVectorWidenRmBinaryOpSx(Context, () => Context.Emit(OpCodes.Add));
        }

        public static void Smax_V(AILEmitterCtx Context)
        {
            Type[] Types = new Type[] { typeof(long), typeof(long) };

            MethodInfo MthdInfo = typeof(Math).GetMethod(nameof(Math.Max), Types);

            EmitVectorBinaryOpSx(Context, () => Context.EmitCall(MthdInfo));
        }

        public static void Smin_V(AILEmitterCtx Context)
        {
            Type[] Types = new Type[] { typeof(long), typeof(long) };

            MethodInfo MthdInfo = typeof(Math).GetMethod(nameof(Math.Min), Types);

            EmitVectorBinaryOpSx(Context, () => Context.EmitCall(MthdInfo));
        }

        public static void Smlal_V(AILEmitterCtx Context)
        {
            EmitVectorWidenRnRmTernaryOpSx(Context, () =>
            {
                Context.Emit(OpCodes.Mul);
                Context.Emit(OpCodes.Add);
            });
        }

        public static void Smull_V(AILEmitterCtx Context)
        {
            EmitVectorWidenRnRmBinaryOpSx(Context, () => Context.Emit(OpCodes.Mul));
        }

        public static void Sqxtn_S(AILEmitterCtx Context)
        {
            EmitSaturatingExtNarrow(Context, SignedSrc: true, SignedDst: true, Scalar: true);
        }

        public static void Sqxtn_V(AILEmitterCtx Context)
        {
            EmitSaturatingExtNarrow(Context, SignedSrc: true, SignedDst: true, Scalar: false);
        }

        public static void Sqxtun_S(AILEmitterCtx Context)
        {
            EmitSaturatingExtNarrow(Context, SignedSrc: true, SignedDst: false, Scalar: true);
        }

        public static void Sqxtun_V(AILEmitterCtx Context)
        {
            EmitSaturatingExtNarrow(Context, SignedSrc: true, SignedDst: false, Scalar: false);
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

        public static void Uabd_V(AILEmitterCtx Context)
        {
            EmitVectorBinaryOpZx(Context, () => EmitAbd(Context));
        }

        public static void Uabdl_V(AILEmitterCtx Context)
        {
            EmitVectorWidenRnRmBinaryOpZx(Context, () => EmitAbd(Context));
        }

        private static void EmitAbd(AILEmitterCtx Context)
        {
            Context.Emit(OpCodes.Sub);

            Type[] Types = new Type[] { typeof(long) };

            Context.EmitCall(typeof(Math).GetMethod(nameof(Math.Abs), Types));
        }

        public static void Uaddl_V(AILEmitterCtx Context)
        {
            EmitVectorWidenRnRmBinaryOpZx(Context, () => Context.Emit(OpCodes.Add));
        }

        public static void Uaddlv_V(AILEmitterCtx Context)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            int Bytes = Context.CurrOp.GetBitsCount() >> 3;

            EmitVectorExtractZx(Context, Op.Rn, 0, Op.Size);

            for (int Index = 1; Index < (Bytes >> Op.Size); Index++)
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

                Context.EmitLdc_I4(1);

                Context.Emit(OpCodes.Shr_Un);
            });
        }

        public static void Umull_V(AILEmitterCtx Context)
        {
            EmitVectorWidenRnRmBinaryOpZx(Context, () => Context.Emit(OpCodes.Mul));
        }

        public static void Uqxtn_S(AILEmitterCtx Context)
        {
            EmitSaturatingExtNarrow(Context, SignedSrc: false, SignedDst: false, Scalar: true);
        }

        public static void Uqxtn_V(AILEmitterCtx Context)
        {
            EmitSaturatingExtNarrow(Context, SignedSrc: false, SignedDst: false, Scalar: false);
        }
    }
}
