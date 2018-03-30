using ChocolArm64.Decoder;
using ChocolArm64.State;
using ChocolArm64.Translation;
using System;
using System.Reflection;
using System.Reflection.Emit;

using static ChocolArm64.Instruction.AInstEmitSimdHelper;

namespace ChocolArm64.Instruction
{
    static partial class AInstEmit
    {
        public static void Add_V(AILEmitterCtx Context)
        {
            EmitVectorBinaryOpZx(Context, () => Context.Emit(OpCodes.Add));
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

        public static void Cnt_V(AILEmitterCtx Context)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            int Elems = Op.RegisterSize == ARegisterSize.SIMD128 ? 16 : 8;

            for (int Index = 0; Index < Elems; Index++)
            {
                EmitVectorExtractZx(Context, Op.Rn, Index, 0);

                Context.Emit(OpCodes.Conv_U1);

                ASoftFallback.EmitCall(Context, nameof(ASoftFallback.CountSetBits8));

                Context.Emit(OpCodes.Conv_U8);

                EmitVectorInsert(Context, Op.Rd, Index, 0);
            }

            if (Op.RegisterSize == ARegisterSize.SIMD64)
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

        public static void Fadd_S(AILEmitterCtx Context)
        {
            EmitScalarBinaryOpF(Context, () => Context.Emit(OpCodes.Add));
        }

        public static void Fadd_V(AILEmitterCtx Context)
        {
            EmitVectorBinaryOpF(Context, () => Context.Emit(OpCodes.Add));
        }

        public static void Fdiv_S(AILEmitterCtx Context)
        {
            EmitScalarBinaryOpF(Context, () => Context.Emit(OpCodes.Div));
        }

        public static void Fdiv_V(AILEmitterCtx Context)
        {
            EmitVectorBinaryOpF(Context, () => Context.Emit(OpCodes.Div));
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
                EmitBinaryMathCall(Context, nameof(Math.Max));
            });
        }

        public static void Fmin_S(AILEmitterCtx Context)
        {
            EmitScalarBinaryOpF(Context, () =>
            {
                EmitBinaryMathCall(Context, nameof(Math.Min));
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
            EmitScalarBinaryOpF(Context, () => Context.Emit(OpCodes.Mul));
        }

        public static void Fmul_V(AILEmitterCtx Context)
        {
            EmitVectorBinaryOpF(Context, () => Context.Emit(OpCodes.Mul));
        }

        public static void Fmul_Ve(AILEmitterCtx Context)
        {
            EmitVectorBinaryOpByElemF(Context, () => Context.Emit(OpCodes.Mul));
        }

        public static void Fneg_S(AILEmitterCtx Context)
        {
            EmitScalarUnaryOpF(Context, () => Context.Emit(OpCodes.Neg));
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
                    ASoftFallback.EmitCall(Context, nameof(ASoftFallback.RoundF));
                }
                else if (Op.Size == 1)
                {
                    ASoftFallback.EmitCall(Context, nameof(ASoftFallback.Round));
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

            EmitVectorUnaryOpF(Context, () =>
            {
                Context.EmitLdarg(ATranslatedSub.StateArgIdx);

                Context.EmitCallPropGet(typeof(AThreadState), nameof(AThreadState.Fpcr));

                if (Op.Size == 2)
                {
                    ASoftFallback.EmitCall(Context, nameof(ASoftFallback.RoundF));
                }
                else if (Op.Size == 3)
                {
                    ASoftFallback.EmitCall(Context, nameof(ASoftFallback.Round));
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
                    ASoftFallback.EmitCall(Context, nameof(ASoftFallback.RoundF));
                }
                else if (Op.Size == 1)
                {
                    ASoftFallback.EmitCall(Context, nameof(ASoftFallback.Round));
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
                    ASoftFallback.EmitCall(Context, nameof(ASoftFallback.RoundF));
                }
                else if (Op.Size == 1)
                {
                    ASoftFallback.EmitCall(Context, nameof(ASoftFallback.Round));
                }
                else
                {
                    throw new InvalidOperationException();
                }
            });
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
            EmitScalarBinaryOpF(Context, () => Context.Emit(OpCodes.Sub));
        }

        public static void Fsub_V(AILEmitterCtx Context)
        {
            EmitVectorBinaryOpF(Context, () => Context.Emit(OpCodes.Sub));
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

        public static void Neg_V(AILEmitterCtx Context)
        {
            EmitVectorUnaryOpSx(Context, () => Context.Emit(OpCodes.Neg));
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

        public static void Sub_S(AILEmitterCtx Context)
        {
            EmitScalarBinaryOpZx(Context, () => Context.Emit(OpCodes.Sub));
        }

        public static void Sub_V(AILEmitterCtx Context)
        {
            EmitVectorBinaryOpZx(Context, () => Context.Emit(OpCodes.Sub));
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
    }
}