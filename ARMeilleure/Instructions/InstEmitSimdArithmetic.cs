// https://github.com/intel/ARM_NEON_2_x86_SSE/blob/master/NEON_2_SSE.h
// https://www.agner.org/optimize/#vectorclass @ vectori128.h

using ARMeilleure.Decoders;
using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.State;
using ARMeilleure.Translation;
using System;

using static ARMeilleure.Instructions.InstEmitHelper;
using static ARMeilleure.Instructions.InstEmitSimdHelper;
using static ARMeilleure.IntermediateRepresentation.OperandHelper;

namespace ARMeilleure.Instructions
{
    using Func2I = Func<Operand, Operand, Operand>;

    static partial class InstEmit
    {
        public static void Abs_S(ArmEmitterContext context)
        {
            EmitScalarUnaryOpSx(context, (op1) => EmitAbs(context, op1));
        }

        public static void Abs_V(ArmEmitterContext context)
        {
            EmitVectorUnaryOpSx(context, (op1) => EmitAbs(context, op1));
        }

        public static void Add_S(ArmEmitterContext context)
        {
            EmitScalarBinaryOpZx(context, (op1, op2) => context.Add(op1, op2));
        }

        public static void Add_V(ArmEmitterContext context)
        {
            if (Optimizations.UseSse2)
            {
                OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

                Operand n = GetVec(op.Rn);
                Operand m = GetVec(op.Rm);

                Intrinsic addInst = X86PaddInstruction[op.Size];

                Operand res = context.AddIntrinsic(addInst, n, m);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    res = context.VectorZeroUpper64(res);
                }

                context.Copy(GetVec(op.Rd), res);
            }
            else
            {
                EmitVectorBinaryOpZx(context, (op1, op2) => context.Add(op1, op2));
            }
        }

        public static void Addhn_V(ArmEmitterContext context)
        {
            EmitHighNarrow(context, (op1, op2) => context.Add(op1, op2), round: false);
        }

        public static void Addp_S(ArmEmitterContext context)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            Operand ne0 = EmitVectorExtractZx(context, op.Rn, 0, op.Size);
            Operand ne1 = EmitVectorExtractZx(context, op.Rn, 1, op.Size);

            Operand res = context.Add(ne0, ne1);

            context.Copy(GetVec(op.Rd), EmitVectorInsert(context, context.VectorZero(), res, 0, op.Size));
        }

        public static void Addp_V(ArmEmitterContext context)
        {
            EmitVectorPairwiseOpZx(context, (op1, op2) => context.Add(op1, op2));
        }

        public static void Addv_V(ArmEmitterContext context)
        {
            EmitVectorAcrossVectorOpZx(context, (op1, op2) => context.Add(op1, op2));
        }

        public static void Cls_V(ArmEmitterContext context)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            Operand res = context.VectorZero();

            int elems = op.GetBytesCount() >> op.Size;

            int eSize = 8 << op.Size;

            for (int index = 0; index < elems; index++)
            {
                Operand ne = EmitVectorExtractZx(context, op.Rn, index, op.Size);

                Operand de = context.Call(new _U64_U64_S32(SoftFallback.CountLeadingSigns), ne, Const(eSize));

                res = EmitVectorInsert(context, res, de, index, op.Size);
            }

            context.Copy(GetVec(op.Rd), res);
        }

        public static void Clz_V(ArmEmitterContext context)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            Operand res = context.VectorZero();

            int elems = op.GetBytesCount() >> op.Size;

            int eSize = 8 << op.Size;

            for (int index = 0; index < elems; index++)
            {
                Operand ne = EmitVectorExtractZx(context, op.Rn, index, op.Size);

                Operand de;

                if (eSize == 64)
                {
                    de = context.CountLeadingZeros(ne);
                }
                else
                {
                    de = context.Call(new _U64_U64_S32(SoftFallback.CountLeadingZeros), ne, Const(eSize));
                }

                res = EmitVectorInsert(context, res, de, index, op.Size);
            }

            context.Copy(GetVec(op.Rd), res);
        }

        public static void Cnt_V(ArmEmitterContext context)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            Operand res = context.VectorZero();

            int elems = op.RegisterSize == RegisterSize.Simd128 ? 16 : 8;

            for (int index = 0; index < elems; index++)
            {
                Operand ne = EmitVectorExtractZx(context, op.Rn, index, 0);

                Operand de;

                if (Optimizations.UsePopCnt)
                {
                    de = context.AddIntrinsicLong(Intrinsic.X86Popcnt, ne);
                }
                else
                {
                    de = context.Call(new _U64_U64(SoftFallback.CountSetBits8), ne);
                }

                res = EmitVectorInsert(context, res, de, index, 0);
            }

            context.Copy(GetVec(op.Rd), res);
        }

        public static void Fabd_S(ArmEmitterContext context)
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

                int sizeF = op.Size & 1;

                if (sizeF == 0)
                {
                    Operand res = context.AddIntrinsic(Intrinsic.X86Subss, GetVec(op.Rn), GetVec(op.Rm));

                    Operand mask = X86GetScalar(context, -0f);

                    res = context.AddIntrinsic(Intrinsic.X86Andnps, mask, res);

                    context.Copy(GetVec(op.Rd), context.VectorZeroUpper96(res));
                }
                else /* if (sizeF == 1) */
                {
                    Operand res = context.AddIntrinsic(Intrinsic.X86Subsd, GetVec(op.Rn), GetVec(op.Rm));

                    Operand mask = X86GetScalar(context, -0d);

                    res = context.AddIntrinsic(Intrinsic.X86Andnpd, mask, res);

                    context.Copy(GetVec(op.Rd), context.VectorZeroUpper64(res));
                }
            }
            else
            {
                EmitScalarBinaryOpF(context, (op1, op2) =>
                {
                    Operand res = EmitSoftFloatCall(context, SoftFloat32.FPSub, SoftFloat64.FPSub, op1, op2);

                    return EmitUnaryMathCall(context, MathF.Abs, Math.Abs, res);
                });
            }
        }

        public static void Fabd_V(ArmEmitterContext context)
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

                int sizeF = op.Size & 1;

                if (sizeF == 0)
                {
                    Operand res = context.AddIntrinsic(Intrinsic.X86Subps, GetVec(op.Rn), GetVec(op.Rm));

                    Operand mask = X86GetAllElements(context, -0f);

                    res = context.AddIntrinsic(Intrinsic.X86Andnps, mask, res);

                    if (op.RegisterSize == RegisterSize.Simd64)
                    {
                        res = context.VectorZeroUpper64(res);
                    }

                    context.Copy(GetVec(op.Rd), res);
                }
                else /* if (sizeF == 1) */
                {
                    Operand res = context.AddIntrinsic(Intrinsic.X86Subpd, GetVec(op.Rn), GetVec(op.Rm));

                    Operand mask = X86GetAllElements(context, -0d);

                    res = context.AddIntrinsic(Intrinsic.X86Andnpd, mask, res);

                    context.Copy(GetVec(op.Rd), res);
                }
            }
            else
            {
                EmitVectorBinaryOpF(context, (op1, op2) =>
                {
                    Operand res = EmitSoftFloatCall(context, SoftFloat32.FPSub, SoftFloat64.FPSub, op1, op2);

                    return EmitUnaryMathCall(context, MathF.Abs, Math.Abs, res);
                });
            }
        }

        public static void Fabs_S(ArmEmitterContext context)
        {
            if (Optimizations.UseSse2)
            {
                OpCodeSimd op = (OpCodeSimd)context.CurrOp;

                if (op.Size == 0)
                {
                    Operand mask = X86GetScalar(context, -0f);

                    Operand res = context.AddIntrinsic(Intrinsic.X86Andnps, mask, GetVec(op.Rn));

                    context.Copy(GetVec(op.Rd), context.VectorZeroUpper96(res));
                }
                else /* if (op.Size == 1) */
                {
                    Operand mask = X86GetScalar(context, -0d);

                    Operand res = context.AddIntrinsic(Intrinsic.X86Andnpd, mask, GetVec(op.Rn));

                    context.Copy(GetVec(op.Rd), context.VectorZeroUpper64(res));
                }
            }
            else
            {
                EmitScalarUnaryOpF(context, (op1) =>
                {
                    return EmitUnaryMathCall(context, MathF.Abs, Math.Abs, op1);
                });
            }
        }

        public static void Fabs_V(ArmEmitterContext context)
        {
            if (Optimizations.UseSse2)
            {
                OpCodeSimd op = (OpCodeSimd)context.CurrOp;

                int sizeF = op.Size & 1;

                 if (sizeF == 0)
                {
                    Operand mask = X86GetAllElements(context, -0f);

                    Operand res = context.AddIntrinsic(Intrinsic.X86Andnps, mask, GetVec(op.Rn));

                    if (op.RegisterSize == RegisterSize.Simd64)
                    {
                        res = context.VectorZeroUpper64(res);
                    }

                    context.Copy(GetVec(op.Rd), res);
                }
                else /* if (sizeF == 1) */
                {
                    Operand mask = X86GetAllElements(context, -0d);

                    Operand res = context.AddIntrinsic(Intrinsic.X86Andnpd, mask, GetVec(op.Rn));

                    context.Copy(GetVec(op.Rd), res);
                }
            }
            else
            {
                EmitVectorUnaryOpF(context, (op1) =>
                {
                    return EmitUnaryMathCall(context, MathF.Abs, Math.Abs, op1);
                });
            }
        }

        public static void Fadd_S(ArmEmitterContext context)
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                EmitScalarBinaryOpF(context, Intrinsic.X86Addss, Intrinsic.X86Addsd);
            }
            else if (Optimizations.FastFP)
            {
                EmitScalarBinaryOpF(context, (op1, op2) => context.Add(op1, op2));
            }
            else
            {
                EmitScalarBinaryOpF(context, (op1, op2) =>
                {
                    return EmitSoftFloatCall(context, SoftFloat32.FPAdd, SoftFloat64.FPAdd, op1, op2);
                });
            }
        }

        public static void Fadd_V(ArmEmitterContext context)
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                EmitVectorBinaryOpF(context, Intrinsic.X86Addps, Intrinsic.X86Addpd);
            }
            else if (Optimizations.FastFP)
            {
                EmitVectorBinaryOpF(context, (op1, op2) => context.Add(op1, op2));
            }
            else
            {
                EmitVectorBinaryOpF(context, (op1, op2) =>
                {
                    return EmitSoftFloatCall(context, SoftFloat32.FPAdd, SoftFloat64.FPAdd, op1, op2);
                });
            }
        }

        public static void Faddp_S(ArmEmitterContext context)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            int sizeF = op.Size & 1;

            if (Optimizations.FastFP && Optimizations.UseSse3)
            {
                if (sizeF == 0)
                {
                    Operand res = context.AddIntrinsic(Intrinsic.X86Haddps, GetVec(op.Rn), GetVec(op.Rn));

                    context.Copy(GetVec(op.Rd), context.VectorZeroUpper96(res));
                }
                else /* if (sizeF == 1) */
                {
                    Operand res = context.AddIntrinsic(Intrinsic.X86Haddpd, GetVec(op.Rn), GetVec(op.Rn));

                    context.Copy(GetVec(op.Rd), context.VectorZeroUpper64(res));
                }
            }
            else
            {
                OperandType type = sizeF != 0 ? OperandType.FP64
                                              : OperandType.FP32;

                Operand ne0 = context.VectorExtract(type, GetVec(op.Rn), 0);
                Operand ne1 = context.VectorExtract(type, GetVec(op.Rn), 1);

                Operand res = EmitSoftFloatCall(context, SoftFloat32.FPAdd, SoftFloat64.FPAdd, ne0, ne1);

                context.Copy(GetVec(op.Rd), context.VectorInsert(context.VectorZero(), res, 0));
            }
        }

        public static void Faddp_V(ArmEmitterContext context)
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                EmitVectorPairwiseOpF(context, Intrinsic.X86Addps, Intrinsic.X86Addpd);
            }
            else
            {
                EmitVectorPairwiseOpF(context, (op1, op2) =>
                {
                    return EmitSoftFloatCall(context, SoftFloat32.FPAdd, SoftFloat64.FPAdd, op1, op2);
                });
            }
        }

        public static void Fdiv_S(ArmEmitterContext context)
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                EmitScalarBinaryOpF(context, Intrinsic.X86Divss, Intrinsic.X86Divsd);
            }
            else if (Optimizations.FastFP)
            {
                EmitScalarBinaryOpF(context, (op1, op2) => context.Divide(op1, op2));
            }
            else
            {
                EmitScalarBinaryOpF(context, (op1, op2) =>
                {
                    return EmitSoftFloatCall(context, SoftFloat32.FPDiv, SoftFloat64.FPDiv, op1, op2);
                });
            }
        }

        public static void Fdiv_V(ArmEmitterContext context)
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                EmitVectorBinaryOpF(context, Intrinsic.X86Divps, Intrinsic.X86Divpd);
            }
            else if (Optimizations.FastFP)
            {
                EmitVectorBinaryOpF(context, (op1, op2) => context.Divide(op1, op2));
            }
            else
            {
                EmitVectorBinaryOpF(context, (op1, op2) =>
                {
                    return EmitSoftFloatCall(context, SoftFloat32.FPDiv, SoftFloat64.FPDiv, op1, op2);
                });
            }
        }

        public static void Fmadd_S(ArmEmitterContext context) // Fused.
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

                Operand a = GetVec(op.Ra);
                Operand n = GetVec(op.Rn);
                Operand m = GetVec(op.Rm);

                if (op.Size == 0)
                {
                    Operand res = context.AddIntrinsic(Intrinsic.X86Mulss, n, m);

                    res = context.AddIntrinsic(Intrinsic.X86Addss, a, res);

                    context.Copy(GetVec(op.Rd), context.VectorZeroUpper96(res));
                }
                else /* if (op.Size == 1) */
                {
                    Operand res = context.AddIntrinsic(Intrinsic.X86Mulsd, n, m);

                    res = context.AddIntrinsic(Intrinsic.X86Addsd, a, res);

                    context.Copy(GetVec(op.Rd), context.VectorZeroUpper64(res));
                }
            }
            else
            {
                EmitScalarTernaryRaOpF(context, (op1, op2, op3) =>
                {
                    return EmitSoftFloatCall(context, SoftFloat32.FPMulAdd, SoftFloat64.FPMulAdd, op1, op2, op3);
                });
            }
        }

        public static void Fmax_S(ArmEmitterContext context)
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                EmitScalarBinaryOpF(context, Intrinsic.X86Maxss, Intrinsic.X86Maxsd);
            }
            else
            {
                EmitScalarBinaryOpF(context, (op1, op2) =>
                {
                    return EmitSoftFloatCall(context, SoftFloat32.FPMax, SoftFloat64.FPMax, op1, op2);
                });
            }
        }

        public static void Fmax_V(ArmEmitterContext context)
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                EmitVectorBinaryOpF(context, Intrinsic.X86Maxps, Intrinsic.X86Maxpd);
            }
            else
            {
                EmitVectorBinaryOpF(context, (op1, op2) =>
                {
                    return EmitSoftFloatCall(context, SoftFloat32.FPMax, SoftFloat64.FPMax, op1, op2);
                });
            }
        }

        public static void Fmaxnm_S(ArmEmitterContext context)
        {
            EmitScalarBinaryOpF(context, (op1, op2) =>
            {
                return EmitSoftFloatCall(context, SoftFloat32.FPMaxNum, SoftFloat64.FPMaxNum, op1, op2);
            });
        }

        public static void Fmaxnm_V(ArmEmitterContext context)
        {
            EmitVectorBinaryOpF(context, (op1, op2) =>
            {
                return EmitSoftFloatCall(context, SoftFloat32.FPMaxNum, SoftFloat64.FPMaxNum, op1, op2);
            });
        }

        public static void Fmaxp_V(ArmEmitterContext context)
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                EmitVectorPairwiseOpF(context, Intrinsic.X86Maxps, Intrinsic.X86Maxpd);
            }
            else
            {
                EmitVectorPairwiseOpF(context, (op1, op2) =>
                {
                    return EmitSoftFloatCall(context, SoftFloat32.FPMax, SoftFloat64.FPMax, op1, op2);
                });
            }
        }

        public static void Fmin_S(ArmEmitterContext context)
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                EmitScalarBinaryOpF(context, Intrinsic.X86Minss, Intrinsic.X86Minsd);
            }
            else
            {
                EmitScalarBinaryOpF(context, (op1, op2) =>
                {
                    return EmitSoftFloatCall(context, SoftFloat32.FPMin, SoftFloat64.FPMin, op1, op2);
                });
            }
        }

        public static void Fmin_V(ArmEmitterContext context)
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                EmitVectorBinaryOpF(context, Intrinsic.X86Minps, Intrinsic.X86Minpd);
            }
            else
            {
                EmitVectorBinaryOpF(context, (op1, op2) =>
                {
                    return EmitSoftFloatCall(context, SoftFloat32.FPMin, SoftFloat64.FPMin, op1, op2);
                });
            }
        }

        public static void Fminnm_S(ArmEmitterContext context)
        {
            EmitScalarBinaryOpF(context, (op1, op2) =>
            {
                return EmitSoftFloatCall(context, SoftFloat32.FPMinNum, SoftFloat64.FPMinNum, op1, op2);
            });
        }

        public static void Fminnm_V(ArmEmitterContext context)
        {
            EmitVectorBinaryOpF(context, (op1, op2) =>
            {
                return EmitSoftFloatCall(context, SoftFloat32.FPMinNum, SoftFloat64.FPMinNum, op1, op2);
            });
        }

        public static void Fminp_V(ArmEmitterContext context)
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                EmitVectorPairwiseOpF(context, Intrinsic.X86Minps, Intrinsic.X86Minpd);
            }
            else
            {
                EmitVectorPairwiseOpF(context, (op1, op2) =>
                {
                    return EmitSoftFloatCall(context, SoftFloat32.FPMin, SoftFloat64.FPMin, op1, op2);
                });
            }
        }

        public static void Fmla_Se(ArmEmitterContext context) // Fused.
        {
            EmitScalarTernaryOpByElemF(context, (op1, op2, op3) =>
            {
                return context.Add(op1, context.Multiply(op2, op3));
            });
        }

        public static void Fmla_V(ArmEmitterContext context) // Fused.
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

                Operand d = GetVec(op.Rd);
                Operand n = GetVec(op.Rn);
                Operand m = GetVec(op.Rm);

                int sizeF = op.Size & 1;

                if (sizeF == 0)
                {
                    Operand res = context.AddIntrinsic(Intrinsic.X86Mulps, n, m);

                    res = context.AddIntrinsic(Intrinsic.X86Addps, d, res);

                    if (op.RegisterSize == RegisterSize.Simd64)
                    {
                        res = context.VectorZeroUpper64(res);
                    }

                    context.Copy(GetVec(op.Rd), res);
                }
                else /* if (sizeF == 1) */
                {
                    Operand res = context.AddIntrinsic(Intrinsic.X86Mulpd, n, m);

                    res = context.AddIntrinsic(Intrinsic.X86Addpd, d, res);

                    context.Copy(GetVec(op.Rd), res);
                }
            }
            else
            {
                EmitVectorTernaryOpF(context, (op1, op2, op3) =>
                {
                    return EmitSoftFloatCall(context, SoftFloat32.FPMulAdd, SoftFloat64.FPMulAdd, op1, op2, op3);
                });
            }
        }

        public static void Fmla_Ve(ArmEmitterContext context) // Fused.
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                OpCodeSimdRegElemF op = (OpCodeSimdRegElemF)context.CurrOp;

                Operand d = GetVec(op.Rd);
                Operand n = GetVec(op.Rn);
                Operand m = GetVec(op.Rm);

                int sizeF = op.Size & 1;

                if (sizeF == 0)
                {
                    int shuffleMask = op.Index | op.Index << 2 | op.Index << 4 | op.Index << 6;

                    Operand res = context.AddIntrinsic(Intrinsic.X86Shufps, m, m, Const(shuffleMask));

                    res = context.AddIntrinsic(Intrinsic.X86Mulps, n, res);
                    res = context.AddIntrinsic(Intrinsic.X86Addps, d, res);

                    if (op.RegisterSize == RegisterSize.Simd64)
                    {
                        res = context.VectorZeroUpper64(res);
                    }

                    context.Copy(GetVec(op.Rd), res);
                }
                else /* if (sizeF == 1) */
                {
                    int shuffleMask = op.Index | op.Index << 1;

                    Operand res = context.AddIntrinsic(Intrinsic.X86Shufpd, m, m, Const(shuffleMask));

                    res = context.AddIntrinsic(Intrinsic.X86Mulpd, n, res);
                    res = context.AddIntrinsic(Intrinsic.X86Addpd, d, res);

                    context.Copy(GetVec(op.Rd), res);
                }
            }
            else
            {
                EmitVectorTernaryOpByElemF(context, (op1, op2, op3) =>
                {
                    return EmitSoftFloatCall(context, SoftFloat32.FPMulAdd, SoftFloat64.FPMulAdd, op1, op2, op3);
                });
            }
        }

        public static void Fmls_Se(ArmEmitterContext context) // Fused.
        {
            EmitScalarTernaryOpByElemF(context, (op1, op2, op3) =>
            {
                return context.Subtract(op1, context.Multiply(op2, op3));
            });
        }

        public static void Fmls_V(ArmEmitterContext context) // Fused.
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

                Operand d = GetVec(op.Rd);
                Operand n = GetVec(op.Rn);
                Operand m = GetVec(op.Rm);

                int sizeF = op.Size & 1;

                if (sizeF == 0)
                {
                    Operand res = context.AddIntrinsic(Intrinsic.X86Mulps, n, m);

                    res = context.AddIntrinsic(Intrinsic.X86Subps, d, res);

                    if (op.RegisterSize == RegisterSize.Simd64)
                    {
                        res = context.VectorZeroUpper64(res);
                    }

                    context.Copy(GetVec(op.Rd), res);
                }
                else /* if (sizeF == 1) */
                {
                    Operand res = context.AddIntrinsic(Intrinsic.X86Mulpd, n, m);

                    res = context.AddIntrinsic(Intrinsic.X86Subpd, d, res);

                    context.Copy(GetVec(op.Rd), res);
                }
            }
            else
            {
                EmitVectorTernaryOpF(context, (op1, op2, op3) =>
                {
                    return EmitSoftFloatCall(context, SoftFloat32.FPMulSub, SoftFloat64.FPMulSub, op1, op2, op3);
                });
            }
        }

        public static void Fmls_Ve(ArmEmitterContext context) // Fused.
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                OpCodeSimdRegElemF op = (OpCodeSimdRegElemF)context.CurrOp;

                Operand d = GetVec(op.Rd);
                Operand n = GetVec(op.Rn);
                Operand m = GetVec(op.Rm);

                int sizeF = op.Size & 1;

                if (sizeF == 0)
                {
                    int shuffleMask = op.Index | op.Index << 2 | op.Index << 4 | op.Index << 6;

                    Operand res = context.AddIntrinsic(Intrinsic.X86Shufps, m, m, Const(shuffleMask));

                    res = context.AddIntrinsic(Intrinsic.X86Mulps, n, res);
                    res = context.AddIntrinsic(Intrinsic.X86Subps, d, res);

                    if (op.RegisterSize == RegisterSize.Simd64)
                    {
                        res = context.VectorZeroUpper64(res);
                    }

                    context.Copy(GetVec(op.Rd), res);
                }
                else /* if (sizeF == 1) */
                {
                    int shuffleMask = op.Index | op.Index << 1;

                    Operand res = context.AddIntrinsic(Intrinsic.X86Shufpd, m, m, Const(shuffleMask));

                    res = context.AddIntrinsic(Intrinsic.X86Mulpd, n, res);
                    res = context.AddIntrinsic(Intrinsic.X86Subpd, d, res);

                    context.Copy(GetVec(op.Rd), res);
                }
            }
            else
            {
                EmitVectorTernaryOpByElemF(context, (op1, op2, op3) =>
                {
                    return EmitSoftFloatCall(context, SoftFloat32.FPMulSub, SoftFloat64.FPMulSub, op1, op2, op3);
                });
            }
        }

        public static void Fmsub_S(ArmEmitterContext context) // Fused.
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

                Operand a = GetVec(op.Ra);
                Operand n = GetVec(op.Rn);
                Operand m = GetVec(op.Rm);

                if (op.Size == 0)
                {
                    Operand res = context.AddIntrinsic(Intrinsic.X86Mulss, n, m);

                    res = context.AddIntrinsic(Intrinsic.X86Subss, a, res);

                    context.Copy(GetVec(op.Rd), context.VectorZeroUpper96(res));
                }
                else /* if (op.Size == 1) */
                {
                    Operand res = context.AddIntrinsic(Intrinsic.X86Mulsd, n, m);

                    res = context.AddIntrinsic(Intrinsic.X86Subsd, a, res);

                    context.Copy(GetVec(op.Rd), context.VectorZeroUpper64(res));
                }
            }
            else
            {
                EmitScalarTernaryRaOpF(context, (op1, op2, op3) =>
                {
                    return EmitSoftFloatCall(context, SoftFloat32.FPMulSub, SoftFloat64.FPMulSub, op1, op2, op3);
                });
            }
        }

        public static void Fmul_S(ArmEmitterContext context)
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                EmitScalarBinaryOpF(context, Intrinsic.X86Mulss, Intrinsic.X86Mulsd);
            }
            else if (Optimizations.FastFP)
            {
                EmitScalarBinaryOpF(context, (op1, op2) => context.Multiply(op1, op2));
            }
            else
            {
                EmitScalarBinaryOpF(context, (op1, op2) =>
                {
                    return EmitSoftFloatCall(context, SoftFloat32.FPMul, SoftFloat64.FPMul, op1, op2);
                });
            }
        }

        public static void Fmul_Se(ArmEmitterContext context)
        {
            EmitScalarBinaryOpByElemF(context, (op1, op2) => context.Multiply(op1, op2));
        }

        public static void Fmul_V(ArmEmitterContext context)
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                EmitVectorBinaryOpF(context, Intrinsic.X86Mulps, Intrinsic.X86Mulpd);
            }
            else if (Optimizations.FastFP)
            {
                EmitVectorBinaryOpF(context, (op1, op2) => context.Multiply(op1, op2));
            }
            else
            {
                EmitVectorBinaryOpF(context, (op1, op2) =>
                {
                    return EmitSoftFloatCall(context, SoftFloat32.FPMul, SoftFloat64.FPMul, op1, op2);
                });
            }
        }

        public static void Fmul_Ve(ArmEmitterContext context)
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                OpCodeSimdRegElemF op = (OpCodeSimdRegElemF)context.CurrOp;

                Operand n = GetVec(op.Rn);
                Operand m = GetVec(op.Rm);

                int sizeF = op.Size & 1;

                if (sizeF == 0)
                {
                    int shuffleMask = op.Index | op.Index << 2 | op.Index << 4 | op.Index << 6;

                    Operand res = context.AddIntrinsic(Intrinsic.X86Shufps, m, m, Const(shuffleMask));

                    res = context.AddIntrinsic(Intrinsic.X86Mulps, n, res);

                    if (op.RegisterSize == RegisterSize.Simd64)
                    {
                        res = context.VectorZeroUpper64(res);
                    }

                    context.Copy(GetVec(op.Rd), res);
                }
                else /* if (sizeF == 1) */
                {
                    int shuffleMask = op.Index | op.Index << 1;

                    Operand res = context.AddIntrinsic(Intrinsic.X86Shufpd, m, m, Const(shuffleMask));

                    res = context.AddIntrinsic(Intrinsic.X86Mulpd, n, res);

                    context.Copy(GetVec(op.Rd), res);
                }
            }
            else if (Optimizations.FastFP)
            {
                EmitVectorBinaryOpByElemF(context, (op1, op2) => context.Multiply(op1, op2));
            }
            else
            {
                EmitVectorBinaryOpByElemF(context, (op1, op2) =>
                {
                    return EmitSoftFloatCall(context, SoftFloat32.FPMul, SoftFloat64.FPMul, op1, op2);
                });
            }
        }

        public static void Fmulx_S(ArmEmitterContext context)
        {
            EmitScalarBinaryOpF(context, (op1, op2) =>
            {
                return EmitSoftFloatCall(context, SoftFloat32.FPMulX, SoftFloat64.FPMulX, op1, op2);
            });
        }

        public static void Fmulx_Se(ArmEmitterContext context)
        {
            EmitScalarBinaryOpByElemF(context, (op1, op2) =>
            {
                return EmitSoftFloatCall(context, SoftFloat32.FPMulX, SoftFloat64.FPMulX, op1, op2);
            });
        }

        public static void Fmulx_V(ArmEmitterContext context)
        {
            EmitVectorBinaryOpF(context, (op1, op2) =>
            {
                return EmitSoftFloatCall(context, SoftFloat32.FPMulX, SoftFloat64.FPMulX, op1, op2);
            });
        }

        public static void Fmulx_Ve(ArmEmitterContext context)
        {
            EmitVectorBinaryOpByElemF(context, (op1, op2) =>
            {
                return EmitSoftFloatCall(context, SoftFloat32.FPMulX, SoftFloat64.FPMulX, op1, op2);
            });
        }

        public static void Fneg_S(ArmEmitterContext context)
        {
            if (Optimizations.UseSse2)
            {
                OpCodeSimd op = (OpCodeSimd)context.CurrOp;

                if (op.Size == 0)
                {
                    Operand mask = X86GetScalar(context, -0f);

                    Operand res = context.AddIntrinsic(Intrinsic.X86Xorps, mask, GetVec(op.Rn));

                    context.Copy(GetVec(op.Rd), context.VectorZeroUpper96(res));
                }
                else /* if (op.Size == 1) */
                {
                    Operand mask = X86GetScalar(context, -0d);

                    Operand res = context.AddIntrinsic(Intrinsic.X86Xorpd, mask, GetVec(op.Rn));

                    context.Copy(GetVec(op.Rd), context.VectorZeroUpper64(res));
                }
            }
            else
            {
                EmitScalarUnaryOpF(context, (op1) => context.Negate(op1));
            }
        }

        public static void Fneg_V(ArmEmitterContext context)
        {
            if (Optimizations.UseSse2)
            {
                OpCodeSimd op = (OpCodeSimd)context.CurrOp;

                int sizeF = op.Size & 1;

                if (sizeF == 0)
                {
                    Operand mask = X86GetAllElements(context, -0f);

                    Operand res = context.AddIntrinsic(Intrinsic.X86Xorps, mask, GetVec(op.Rn));

                    if (op.RegisterSize == RegisterSize.Simd64)
                    {
                        res = context.VectorZeroUpper64(res);
                    }

                    context.Copy(GetVec(op.Rd), res);
                }
                else /* if (sizeF == 1) */
                {
                    Operand mask = X86GetAllElements(context, -0d);

                    Operand res = context.AddIntrinsic(Intrinsic.X86Xorpd, mask, GetVec(op.Rn));

                    context.Copy(GetVec(op.Rd), res);
                }
            }
            else
            {
                EmitVectorUnaryOpF(context, (op1) => context.Negate(op1));
            }
        }

        public static void Fnmadd_S(ArmEmitterContext context)
        {
            OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

            int sizeF = op.Size & 1;

            OperandType type = sizeF != 0 ? OperandType.FP64
                                          : OperandType.FP32;

            Operand ne = context.VectorExtract(type, GetVec(op.Rn), 0);
            Operand me = context.VectorExtract(type, GetVec(op.Rm), 0);
            Operand ae = context.VectorExtract(type, GetVec(op.Ra), 0);

            Operand res = context.Subtract(context.Multiply(context.Negate(ne), me), ae);

            context.Copy(GetVec(op.Rd), context.VectorInsert(context.VectorZero(), res, 0));
        }

        public static void Fnmsub_S(ArmEmitterContext context)
        {
            OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

            int sizeF = op.Size & 1;

            OperandType type = sizeF != 0 ? OperandType.FP64
                                          : OperandType.FP32;

            Operand ne = context.VectorExtract(type, GetVec(op.Rn), 0);
            Operand me = context.VectorExtract(type, GetVec(op.Rm), 0);
            Operand ae = context.VectorExtract(type, GetVec(op.Ra), 0);

            Operand res = context.Subtract(context.Multiply(ne, me), ae);

            context.Copy(GetVec(op.Rd), context.VectorInsert(context.VectorZero(), res, 0));
        }

        public static void Fnmul_S(ArmEmitterContext context)
        {
            EmitScalarBinaryOpF(context, (op1, op2) => context.Negate(context.Multiply(op1, op2)));
        }

        public static void Frecpe_S(ArmEmitterContext context)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            int sizeF = op.Size & 1;

            if (Optimizations.FastFP && Optimizations.UseSse && sizeF == 0)
            {
                EmitScalarUnaryOpF(context, Intrinsic.X86Rcpss, 0);
            }
            else
            {
                EmitScalarUnaryOpF(context, (op1) =>
                {
                    return EmitSoftFloatCall(context, SoftFloat32.FPRecipEstimate, SoftFloat64.FPRecipEstimate, op1);
                });
            }
        }

        public static void Frecpe_V(ArmEmitterContext context)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            int sizeF = op.Size & 1;

            if (Optimizations.FastFP && Optimizations.UseSse && sizeF == 0)
            {
                EmitVectorUnaryOpF(context, Intrinsic.X86Rcpps, 0);
            }
            else
            {
                EmitVectorUnaryOpF(context, (op1) =>
                {
                    return EmitSoftFloatCall(context, SoftFloat32.FPRecipEstimate, SoftFloat64.FPRecipEstimate, op1);
                });
            }
        }

        public static void Frecps_S(ArmEmitterContext context) // Fused.
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

                int sizeF = op.Size & 1;

                if (sizeF == 0)
                {
                    Operand mask = X86GetScalar(context, 2f);

                    Operand res = context.AddIntrinsic(Intrinsic.X86Mulss, GetVec(op.Rn), GetVec(op.Rm));

                    res = context.AddIntrinsic(Intrinsic.X86Subss, mask, res);

                    context.Copy(GetVec(op.Rd), context.VectorZeroUpper96(res));
                }
                else /* if (sizeF == 1) */
                {
                    Operand mask = X86GetScalar(context, 2d);

                    Operand res = context.AddIntrinsic(Intrinsic.X86Mulsd, GetVec(op.Rn), GetVec(op.Rm));

                    res = context.AddIntrinsic(Intrinsic.X86Subsd, mask, res);

                    context.Copy(GetVec(op.Rd), context.VectorZeroUpper64(res));
                }
            }
            else
            {
                EmitScalarBinaryOpF(context, (op1, op2) =>
                {
                    return EmitSoftFloatCall(context, SoftFloat32.FPRecipStepFused, SoftFloat64.FPRecipStepFused, op1, op2);
                });
            }
        }

        public static void Frecps_V(ArmEmitterContext context) // Fused.
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

                int sizeF = op.Size & 1;

                if (sizeF == 0)
                {
                    Operand mask = X86GetAllElements(context, 2f);

                    Operand res = context.AddIntrinsic(Intrinsic.X86Mulps, GetVec(op.Rn), GetVec(op.Rm));

                    res = context.AddIntrinsic(Intrinsic.X86Subps, mask, res);

                    if (op.RegisterSize == RegisterSize.Simd64)
                    {
                        res = context.VectorZeroUpper64(res);
                    }

                    context.Copy(GetVec(op.Rd), res);
                }
                else /* if (sizeF == 1) */
                {
                    Operand mask = X86GetAllElements(context, 2d);

                    Operand res = context.AddIntrinsic(Intrinsic.X86Mulpd, GetVec(op.Rn), GetVec(op.Rm));

                    res = context.AddIntrinsic(Intrinsic.X86Subpd, mask, res);

                    context.Copy(GetVec(op.Rd), res);
                }
            }
            else
            {
                EmitVectorBinaryOpF(context, (op1, op2) =>
                {
                    return EmitSoftFloatCall(context, SoftFloat32.FPRecipStepFused, SoftFloat64.FPRecipStepFused, op1, op2);
                });
            }
        }

        public static void Frecpx_S(ArmEmitterContext context)
        {
            EmitScalarUnaryOpF(context, (op1) =>
            {
                return EmitSoftFloatCall(context, SoftFloat32.FPRecpX, SoftFloat64.FPRecpX, op1);
            });
        }

        public static void Frinta_S(ArmEmitterContext context)
        {
            EmitScalarUnaryOpF(context, (op1) =>
            {
                return EmitRoundMathCall(context, MidpointRounding.AwayFromZero, op1);
            });
        }

        public static void Frinta_V(ArmEmitterContext context)
        {
            EmitVectorUnaryOpF(context, (op1) =>
            {
                return EmitRoundMathCall(context, MidpointRounding.AwayFromZero, op1);
            });
        }

        public static void Frinti_S(ArmEmitterContext context)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            EmitScalarUnaryOpF(context, (op1) =>
            {
                if (op.Size == 0)
                {
                    return context.Call(new _F32_F32(SoftFallback.RoundF), op1);
                }
                else /* if (op.Size == 1) */
                {
                    return context.Call(new _F64_F64(SoftFallback.Round), op1);
                }
            });
        }

        public static void Frinti_V(ArmEmitterContext context)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            int sizeF = op.Size & 1;

            EmitVectorUnaryOpF(context, (op1) =>
            {
                if (sizeF == 0)
                {
                    return context.Call(new _F32_F32(SoftFallback.RoundF), op1);
                }
                else /* if (sizeF == 1) */
                {
                    return context.Call(new _F64_F64(SoftFallback.Round), op1);
                }
            });
        }

        public static void Frintm_S(ArmEmitterContext context)
        {
            if (Optimizations.UseSse41)
            {
                EmitScalarRoundOpF(context, FPRoundingMode.TowardsMinusInfinity);
            }
            else
            {
                EmitScalarUnaryOpF(context, (op1) =>
                {
                    return EmitUnaryMathCall(context, MathF.Floor, Math.Floor, op1);
                });
            }
        }

        public static void Frintm_V(ArmEmitterContext context)
        {
            if (Optimizations.UseSse41)
            {
                EmitVectorRoundOpF(context, FPRoundingMode.TowardsMinusInfinity);
            }
            else
            {
                EmitVectorUnaryOpF(context, (op1) =>
                {
                    return EmitUnaryMathCall(context, MathF.Floor, Math.Floor, op1);
                });
            }
        }

        public static void Frintn_S(ArmEmitterContext context)
        {
            if (Optimizations.UseSse41)
            {
                EmitScalarRoundOpF(context, FPRoundingMode.ToNearest);
            }
            else
            {
                EmitScalarUnaryOpF(context, (op1) =>
                {
                    return EmitRoundMathCall(context, MidpointRounding.ToEven, op1);
                });
            }
        }

        public static void Frintn_V(ArmEmitterContext context)
        {
            if (Optimizations.UseSse41)
            {
                EmitVectorRoundOpF(context, FPRoundingMode.ToNearest);
            }
            else
            {
                EmitVectorUnaryOpF(context, (op1) =>
                {
                    return EmitRoundMathCall(context, MidpointRounding.ToEven, op1);
                });
            }
        }

        public static void Frintp_S(ArmEmitterContext context)
        {
            if (Optimizations.UseSse41)
            {
                EmitScalarRoundOpF(context, FPRoundingMode.TowardsPlusInfinity);
            }
            else
            {
                EmitScalarUnaryOpF(context, (op1) =>
                {
                    return EmitUnaryMathCall(context, MathF.Ceiling, Math.Ceiling, op1);
                });
            }
        }

        public static void Frintp_V(ArmEmitterContext context)
        {
            if (Optimizations.UseSse41)
            {
                EmitVectorRoundOpF(context, FPRoundingMode.TowardsPlusInfinity);
            }
            else
            {
                EmitVectorUnaryOpF(context, (op1) =>
                {
                    return EmitUnaryMathCall(context, MathF.Ceiling, Math.Ceiling, op1);
                });
            }
        }

        public static void Frintx_S(ArmEmitterContext context)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            EmitScalarUnaryOpF(context, (op1) =>
            {
                if (op.Size == 0)
                {
                    return context.Call(new _F32_F32(SoftFallback.RoundF), op1);
                }
                else /* if (op.Size == 1) */
                {
                    return context.Call(new _F64_F64(SoftFallback.Round), op1);
                }
            });
        }

        public static void Frintx_V(ArmEmitterContext context)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            int sizeF = op.Size & 1;

            EmitVectorUnaryOpF(context, (op1) =>
            {
                if (sizeF == 0)
                {
                    return context.Call(new _F32_F32(SoftFallback.RoundF), op1);
                }
                else /* if (sizeF == 1) */
                {
                    return context.Call(new _F64_F64(SoftFallback.Round), op1);
                }
            });
        }

        public static void Frintz_S(ArmEmitterContext context)
        {
            if (Optimizations.UseSse41)
            {
                EmitScalarRoundOpF(context, FPRoundingMode.TowardsZero);
            }
            else
            {
                EmitScalarUnaryOpF(context, (op1) =>
                {
                    return EmitUnaryMathCall(context, MathF.Truncate, Math.Truncate, op1);
                });
            }
        }

        public static void Frintz_V(ArmEmitterContext context)
        {
            if (Optimizations.UseSse41)
            {
                EmitVectorRoundOpF(context, FPRoundingMode.TowardsZero);
            }
            else
            {
                EmitVectorUnaryOpF(context, (op1) =>
                {
                    return EmitUnaryMathCall(context, MathF.Truncate, Math.Truncate, op1);
                });
            }
        }

        public static void Frsqrte_S(ArmEmitterContext context)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            int sizeF = op.Size & 1;

            if (Optimizations.FastFP && Optimizations.UseSse && sizeF == 0)
            {
                EmitScalarUnaryOpF(context, Intrinsic.X86Rsqrtss, 0);
            }
            else
            {
                EmitScalarUnaryOpF(context, (op1) =>
                {
                    return EmitSoftFloatCall(context, SoftFloat32.FPRSqrtEstimate, SoftFloat64.FPRSqrtEstimate, op1);
                });
            }
        }

        public static void Frsqrte_V(ArmEmitterContext context)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            int sizeF = op.Size & 1;

            if (Optimizations.FastFP && Optimizations.UseSse && sizeF == 0)
            {
                EmitVectorUnaryOpF(context, Intrinsic.X86Rsqrtps, 0);
            }
            else
            {
                EmitVectorUnaryOpF(context, (op1) =>
                {
                    return EmitSoftFloatCall(context, SoftFloat32.FPRSqrtEstimate, SoftFloat64.FPRSqrtEstimate, op1);
                });
            }
        }

        public static void Frsqrts_S(ArmEmitterContext context) // Fused.
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

                int sizeF = op.Size & 1;

                if (sizeF == 0)
                {
                    Operand maskHalf  = X86GetScalar(context, 0.5f);
                    Operand maskThree = X86GetScalar(context, 3f);

                    Operand res = context.AddIntrinsic(Intrinsic.X86Mulss, GetVec(op.Rn), GetVec(op.Rm));

                    res = context.AddIntrinsic(Intrinsic.X86Subss, maskThree, res);
                    res = context.AddIntrinsic(Intrinsic.X86Mulss, maskHalf,  res);

                    context.Copy(GetVec(op.Rd), context.VectorZeroUpper96(res));
                }
                else /* if (sizeF == 1) */
                {
                    Operand maskHalf  = X86GetScalar(context, 0.5d);
                    Operand maskThree = X86GetScalar(context, 3d);

                    Operand res = context.AddIntrinsic(Intrinsic.X86Mulsd, GetVec(op.Rn), GetVec(op.Rm));

                    res = context.AddIntrinsic(Intrinsic.X86Subsd, maskThree, res);
                    res = context.AddIntrinsic(Intrinsic.X86Mulsd, maskHalf,  res);

                    context.Copy(GetVec(op.Rd), context.VectorZeroUpper64(res));
                }
            }
            else
            {
                EmitScalarBinaryOpF(context, (op1, op2) =>
                {
                    return EmitSoftFloatCall(context, SoftFloat32.FPRSqrtStepFused, SoftFloat64.FPRSqrtStepFused, op1, op2);
                });
            }
        }

        public static void Frsqrts_V(ArmEmitterContext context) // Fused.
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

                int sizeF = op.Size & 1;

                if (sizeF == 0)
                {
                    Operand maskHalf  = X86GetAllElements(context, 0.5f);
                    Operand maskThree = X86GetAllElements(context, 3f);

                    Operand res = context.AddIntrinsic(Intrinsic.X86Mulps, GetVec(op.Rn), GetVec(op.Rm));

                    res = context.AddIntrinsic(Intrinsic.X86Subps, maskThree, res);
                    res = context.AddIntrinsic(Intrinsic.X86Mulps, maskHalf,  res);

                    if (op.RegisterSize == RegisterSize.Simd64)
                    {
                        res = context.VectorZeroUpper64(res);
                    }

                    context.Copy(GetVec(op.Rd), res);
                }
                else /* if (sizeF == 1) */
                {
                    Operand maskHalf  = X86GetAllElements(context, 0.5d);
                    Operand maskThree = X86GetAllElements(context, 3d);

                    Operand res = context.AddIntrinsic(Intrinsic.X86Mulpd, GetVec(op.Rn), GetVec(op.Rm));

                    res = context.AddIntrinsic(Intrinsic.X86Subpd, maskThree, res);
                    res = context.AddIntrinsic(Intrinsic.X86Mulpd, maskHalf,  res);

                    context.Copy(GetVec(op.Rd), res);
                }
            }
            else
            {
                EmitVectorBinaryOpF(context, (op1, op2) =>
                {
                    return EmitSoftFloatCall(context, SoftFloat32.FPRSqrtStepFused, SoftFloat64.FPRSqrtStepFused, op1, op2);
                });
            }
        }

        public static void Fsqrt_S(ArmEmitterContext context)
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                EmitScalarUnaryOpF(context, Intrinsic.X86Sqrtss, Intrinsic.X86Sqrtsd);
            }
            else
            {
                EmitScalarUnaryOpF(context, (op1) =>
                {
                    return EmitSoftFloatCall(context, SoftFloat32.FPSqrt, SoftFloat64.FPSqrt, op1);
                });
            }
        }

        public static void Fsqrt_V(ArmEmitterContext context)
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                EmitVectorUnaryOpF(context, Intrinsic.X86Sqrtps, Intrinsic.X86Sqrtpd);
            }
            else
            {
                EmitVectorUnaryOpF(context, (op1) =>
                {
                    return EmitSoftFloatCall(context, SoftFloat32.FPSqrt, SoftFloat64.FPSqrt, op1);
                });
            }
        }

        public static void Fsub_S(ArmEmitterContext context)
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                EmitScalarBinaryOpF(context, Intrinsic.X86Subss, Intrinsic.X86Subsd);
            }
            else if (Optimizations.FastFP)
            {
                EmitScalarBinaryOpF(context, (op1, op2) => context.Subtract(op1, op2));
            }
            else
            {
                EmitScalarBinaryOpF(context, (op1, op2) =>
                {
                    return EmitSoftFloatCall(context, SoftFloat32.FPSub, SoftFloat64.FPSub, op1, op2);
                });
            }
        }

        public static void Fsub_V(ArmEmitterContext context)
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                EmitVectorBinaryOpF(context, Intrinsic.X86Subps, Intrinsic.X86Subpd);
            }
            else if (Optimizations.FastFP)
            {
                EmitVectorBinaryOpF(context, (op1, op2) => context.Subtract(op1, op2));
            }
            else
            {
                EmitVectorBinaryOpF(context, (op1, op2) =>
                {
                    return EmitSoftFloatCall(context, SoftFloat32.FPSub, SoftFloat64.FPSub, op1, op2);
                });
            }
        }

        public static void Mla_V(ArmEmitterContext context)
        {
            if (Optimizations.UseSse41)
            {
                EmitSse41Mul_AddSub(context, AddSub.Add);
            }
            else
            {
                EmitVectorTernaryOpZx(context, (op1, op2, op3) =>
                {
                    return context.Add(op1, context.Multiply(op2, op3));
                });
            }
        }

        public static void Mla_Ve(ArmEmitterContext context)
        {
            EmitVectorTernaryOpByElemZx(context, (op1, op2, op3) =>
            {
                return context.Add(op1, context.Multiply(op2, op3));
            });
        }

        public static void Mls_V(ArmEmitterContext context)
        {
            if (Optimizations.UseSse41)
            {
                EmitSse41Mul_AddSub(context, AddSub.Subtract);
            }
            else
            {
                EmitVectorTernaryOpZx(context, (op1, op2, op3) =>
                {
                    return context.Subtract(op1, context.Multiply(op2, op3));
                });
            }
        }

        public static void Mls_Ve(ArmEmitterContext context)
        {
            EmitVectorTernaryOpByElemZx(context, (op1, op2, op3) =>
            {
                return context.Subtract(op1, context.Multiply(op2, op3));
            });
        }

        public static void Mul_V(ArmEmitterContext context)
        {
            if (Optimizations.UseSse41)
            {
                EmitSse41Mul_AddSub(context, AddSub.None);
            }
            else
            {
                EmitVectorBinaryOpZx(context, (op1, op2) => context.Multiply(op1, op2));
            }
        }

        public static void Mul_Ve(ArmEmitterContext context)
        {
            EmitVectorBinaryOpByElemZx(context, (op1, op2) => context.Multiply(op1, op2));
        }

        public static void Neg_S(ArmEmitterContext context)
        {
            EmitScalarUnaryOpSx(context, (op1) => context.Negate(op1));
        }

        public static void Neg_V(ArmEmitterContext context)
        {
            if (Optimizations.UseSse2)
            {
                OpCodeSimd op = (OpCodeSimd)context.CurrOp;

                Intrinsic subInst = X86PsubInstruction[op.Size];

                Operand res = context.AddIntrinsic(subInst, context.VectorZero(), GetVec(op.Rn));

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    res = context.VectorZeroUpper64(res);
                }

                context.Copy(GetVec(op.Rd), res);
            }
            else
            {
                EmitVectorUnaryOpSx(context, (op1) => context.Negate(op1));
            }
        }

        public static void Raddhn_V(ArmEmitterContext context)
        {
            EmitHighNarrow(context, (op1, op2) => context.Add(op1, op2), round: true);
        }

        public static void Rsubhn_V(ArmEmitterContext context)
        {
            EmitHighNarrow(context, (op1, op2) => context.Subtract(op1, op2), round: true);
        }

        public static void Saba_V(ArmEmitterContext context)
        {
            EmitVectorTernaryOpSx(context, (op1, op2, op3) =>
            {
                return context.Add(op1, EmitAbs(context, context.Subtract(op2, op3)));
            });
        }

        public static void Sabal_V(ArmEmitterContext context)
        {
            EmitVectorWidenRnRmTernaryOpSx(context, (op1, op2, op3) =>
            {
                return context.Add(op1, EmitAbs(context, context.Subtract(op2, op3)));
            });
        }

        public static void Sabd_V(ArmEmitterContext context)
        {
            if (Optimizations.UseSse2)
            {
                OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

                Operand n = GetVec(op.Rn);
                Operand m = GetVec(op.Rm);

                EmitSse41Sabd(context, op, n, m, isLong: false);
            }
            else
            {
                EmitVectorBinaryOpSx(context, (op1, op2) =>
                {
                    return EmitAbs(context, context.Subtract(op1, op2));
                });
            }
        }

        public static void Sabdl_V(ArmEmitterContext context)
        {
            OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

            if (Optimizations.UseSse41 && op.Size < 2)
            {
                Operand n = GetVec(op.Rn);
                Operand m = GetVec(op.Rm);

                if (op.RegisterSize == RegisterSize.Simd128)
                {
                    n = context.AddIntrinsic(Intrinsic.X86Psrldq, n, Const(8));
                    m = context.AddIntrinsic(Intrinsic.X86Psrldq, m, Const(8));
                }

                Intrinsic movInst = op.Size == 0
                    ? Intrinsic.X86Pmovsxbw
                    : Intrinsic.X86Pmovsxwd;

                n = context.AddIntrinsic(movInst, n);
                m = context.AddIntrinsic(movInst, m);

                EmitSse41Sabd(context, op, n, m, isLong: true);
            }
            else
            {
                EmitVectorWidenRnRmBinaryOpSx(context, (op1, op2) =>
                {
                    return EmitAbs(context, context.Subtract(op1, op2));
                });
            }
        }

        public static void Sadalp_V(ArmEmitterContext context)
        {
            EmitAddLongPairwise(context, signed: true, accumulate: true);
        }

        public static void Saddl_V(ArmEmitterContext context)
        {
            if (Optimizations.UseSse41)
            {
                OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

                Operand n = GetVec(op.Rn);
                Operand m = GetVec(op.Rm);

                if (op.RegisterSize == RegisterSize.Simd128)
                {
                    n = context.AddIntrinsic(Intrinsic.X86Psrldq, n, Const(8));
                    m = context.AddIntrinsic(Intrinsic.X86Psrldq, m, Const(8));
                }

                Intrinsic movInst = X86PmovsxInstruction[op.Size];

                n = context.AddIntrinsic(movInst, n);
                m = context.AddIntrinsic(movInst, m);

                Intrinsic addInst = X86PaddInstruction[op.Size + 1];

                context.Copy(GetVec(op.Rd), context.AddIntrinsic(addInst, n, m));
            }
            else
            {
                EmitVectorWidenRnRmBinaryOpSx(context, (op1, op2) => context.Add(op1, op2));
            }
        }

        public static void Saddlp_V(ArmEmitterContext context)
        {
            EmitAddLongPairwise(context, signed: true, accumulate: false);
        }

        public static void Saddlv_V(ArmEmitterContext context)
        {
            EmitVectorLongAcrossVectorOpSx(context, (op1, op2) => context.Add(op1, op2));
        }

        public static void Saddw_V(ArmEmitterContext context)
        {
            if (Optimizations.UseSse41)
            {
                OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

                Operand n = GetVec(op.Rn);
                Operand m = GetVec(op.Rm);

                if (op.RegisterSize == RegisterSize.Simd128)
                {
                    m = context.AddIntrinsic(Intrinsic.X86Psrldq, m, Const(8));
                }

                Intrinsic movInst = X86PmovsxInstruction[op.Size];

                m = context.AddIntrinsic(movInst, m);

                Intrinsic addInst = X86PaddInstruction[op.Size + 1];

                context.Copy(GetVec(op.Rd), context.AddIntrinsic(addInst, n, m));
            }
            else
            {
                EmitVectorWidenRmBinaryOpSx(context, (op1, op2) => context.Add(op1, op2));
            }
        }

        public static void Shadd_V(ArmEmitterContext context)
        {
            OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

            if (Optimizations.UseSse2 && op.Size > 0)
            {
                Operand n = GetVec(op.Rn);
                Operand m = GetVec(op.Rm);

                Operand res  = context.AddIntrinsic(Intrinsic.X86Pand, n, m);
                Operand res2 = context.AddIntrinsic(Intrinsic.X86Pxor, n, m);

                Intrinsic shiftInst = op.Size == 1 ? Intrinsic.X86Psraw : Intrinsic.X86Psrad;

                res2 = context.AddIntrinsic(shiftInst, res2, Const(1));

                Intrinsic addInst = X86PaddInstruction[op.Size];

                res = context.AddIntrinsic(addInst, res, res2);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    res = context.VectorZeroUpper64(res);
                }

                context.Copy(GetVec(op.Rd), res);
            }
            else
            {
                EmitVectorBinaryOpSx(context, (op1, op2) =>
                {
                    return context.ShiftRightSI(context.Add(op1, op2), Const(1));
                });
            }
        }

        public static void Shsub_V(ArmEmitterContext context)
        {
            OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

            if (Optimizations.UseSse2 && op.Size < 2)
            {
                Operand n = GetVec(op.Rn);
                Operand m = GetVec(op.Rm);

                Operand mask = X86GetAllElements(context, (int)(op.Size == 0 ? 0x80808080u : 0x80008000u));

                Intrinsic addInst = X86PaddInstruction[op.Size];

                Operand nPlusMask = context.AddIntrinsic(addInst, n, mask);
                Operand mPlusMask = context.AddIntrinsic(addInst, m, mask);

                Intrinsic avgInst = op.Size == 0 ? Intrinsic.X86Pavgb : Intrinsic.X86Pavgw;

                Operand res = context.AddIntrinsic(avgInst, nPlusMask, mPlusMask);

                Intrinsic subInst = X86PsubInstruction[op.Size];

                res = context.AddIntrinsic(subInst, nPlusMask, res);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    res = context.VectorZeroUpper64(res);
                }

                context.Copy(GetVec(op.Rd), res);
            }
            else
            {
                EmitVectorBinaryOpSx(context, (op1, op2) =>
                {
                    return context.ShiftRightSI(context.Subtract(op1, op2), Const(1));
                });
            }
        }

        public static void Smax_V(ArmEmitterContext context)
        {
            if (Optimizations.UseSse41)
            {
                OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

                Operand n = GetVec(op.Rn);
                Operand m = GetVec(op.Rm);

                Intrinsic maxInst = X86PmaxsInstruction[op.Size];

                Operand res = context.AddIntrinsic(maxInst, n, m);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    res = context.VectorZeroUpper64(res);
                }

                context.Copy(GetVec(op.Rd), res);
            }
            else
            {
                Delegate dlg = new _S64_S64_S64(Math.Max);

                EmitVectorBinaryOpSx(context, (op1, op2) => context.Call(dlg, op1, op2));
            }
        }

        public static void Smaxp_V(ArmEmitterContext context)
        {
            Delegate dlg = new _S64_S64_S64(Math.Max);

            EmitVectorPairwiseOpSx(context, (op1, op2) => context.Call(dlg, op1, op2));
        }

        public static void Smaxv_V(ArmEmitterContext context)
        {
            Delegate dlg = new _S64_S64_S64(Math.Max);

            EmitVectorAcrossVectorOpSx(context, (op1, op2) => context.Call(dlg, op1, op2));
        }

        public static void Smin_V(ArmEmitterContext context)
        {
            if (Optimizations.UseSse41)
            {
                OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

                Operand n = GetVec(op.Rn);
                Operand m = GetVec(op.Rm);

                Intrinsic minInst = X86PminsInstruction[op.Size];

                Operand res = context.AddIntrinsic(minInst, n, m);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    res = context.VectorZeroUpper64(res);
                }

                context.Copy(GetVec(op.Rd), res);
            }
            else
            {
                Delegate dlg = new _S64_S64_S64(Math.Min);

                EmitVectorBinaryOpSx(context, (op1, op2) => context.Call(dlg, op1, op2));
            }
        }

        public static void Sminp_V(ArmEmitterContext context)
        {
            Delegate dlg = new _S64_S64_S64(Math.Min);

            EmitVectorPairwiseOpSx(context, (op1, op2) => context.Call(dlg, op1, op2));
        }

        public static void Sminv_V(ArmEmitterContext context)
        {
            Delegate dlg = new _S64_S64_S64(Math.Min);

            EmitVectorAcrossVectorOpSx(context, (op1, op2) => context.Call(dlg, op1, op2));
        }

        public static void Smlal_V(ArmEmitterContext context)
        {
            OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

            if (Optimizations.UseSse41 && op.Size < 2)
            {
                Operand d = GetVec(op.Rd);
                Operand n = GetVec(op.Rn);
                Operand m = GetVec(op.Rm);

                if (op.RegisterSize == RegisterSize.Simd128)
                {
                    n = context.AddIntrinsic(Intrinsic.X86Psrldq, n, Const(8));
                    m = context.AddIntrinsic(Intrinsic.X86Psrldq, m, Const(8));
                }

                Intrinsic movInst = X86PmovsxInstruction[op.Size];

                n = context.AddIntrinsic(movInst, n);
                m = context.AddIntrinsic(movInst, m);

                Intrinsic mullInst = op.Size == 0 ? Intrinsic.X86Pmullw : Intrinsic.X86Pmulld;

                Operand res = context.AddIntrinsic(mullInst, n, m);

                Intrinsic addInst = X86PaddInstruction[op.Size + 1];

                context.Copy(d, context.AddIntrinsic(addInst, d, res));
            }
            else
            {
                EmitVectorWidenRnRmTernaryOpSx(context, (op1, op2, op3) =>
                {
                    return context.Add(op1, context.Multiply(op2, op3));
                });
            }
        }

        public static void Smlal_Ve(ArmEmitterContext context)
        {
            EmitVectorWidenTernaryOpByElemSx(context, (op1, op2, op3) =>
            {
                return context.Add(op1, context.Multiply(op2, op3));
            });
        }

        public static void Smlsl_V(ArmEmitterContext context)
        {
            OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

            if (Optimizations.UseSse41 && op.Size < 2)
            {
                Operand d = GetVec(op.Rd);
                Operand n = GetVec(op.Rn);
                Operand m = GetVec(op.Rm);

                if (op.RegisterSize == RegisterSize.Simd128)
                {
                    n = context.AddIntrinsic(Intrinsic.X86Psrldq, n, Const(8));
                    m = context.AddIntrinsic(Intrinsic.X86Psrldq, m, Const(8));
                }

                Intrinsic movInst = op.Size == 0
                    ? Intrinsic.X86Pmovsxbw
                    : Intrinsic.X86Pmovsxwd;

                n = context.AddIntrinsic(movInst, n);
                m = context.AddIntrinsic(movInst, m);

                Intrinsic mullInst = op.Size == 0 ? Intrinsic.X86Pmullw : Intrinsic.X86Pmulld;

                Operand res = context.AddIntrinsic(mullInst, n, m);

                Intrinsic subInst = X86PsubInstruction[op.Size + 1];

                context.Copy(d, context.AddIntrinsic(subInst, d, res));
            }
            else
            {
                EmitVectorWidenRnRmTernaryOpSx(context, (op1, op2, op3) =>
                {
                    return context.Subtract(op1, context.Multiply(op2, op3));
                });
            }
        }

        public static void Smlsl_Ve(ArmEmitterContext context)
        {
            EmitVectorWidenTernaryOpByElemSx(context, (op1, op2, op3) =>
            {
                return context.Subtract(op1, context.Multiply(op2, op3));
            });
        }

        public static void Smull_V(ArmEmitterContext context)
        {
            EmitVectorWidenRnRmBinaryOpSx(context, (op1, op2) => context.Multiply(op1, op2));
        }

        public static void Smull_Ve(ArmEmitterContext context)
        {
            EmitVectorWidenBinaryOpByElemSx(context, (op1, op2) => context.Multiply(op1, op2));
        }

        public static void Sqabs_S(ArmEmitterContext context)
        {
            EmitScalarSaturatingUnaryOpSx(context, (op1) => EmitAbs(context, op1));
        }

        public static void Sqabs_V(ArmEmitterContext context)
        {
            EmitVectorSaturatingUnaryOpSx(context, (op1) => EmitAbs(context, op1));
        }

        public static void Sqadd_S(ArmEmitterContext context)
        {
            EmitScalarSaturatingBinaryOpSx(context, SaturatingFlags.Add);
        }

        public static void Sqadd_V(ArmEmitterContext context)
        {
            EmitVectorSaturatingBinaryOpSx(context, SaturatingFlags.Add);
        }

        public static void Sqdmulh_S(ArmEmitterContext context)
        {
            EmitSaturatingBinaryOp(context, (op1, op2) => EmitDoublingMultiplyHighHalf(context, op1, op2, round: false), SaturatingFlags.ScalarSx);
        }

        public static void Sqdmulh_V(ArmEmitterContext context)
        {
            EmitSaturatingBinaryOp(context, (op1, op2) => EmitDoublingMultiplyHighHalf(context, op1, op2, round: false), SaturatingFlags.VectorSx);
        }

        public static void Sqneg_S(ArmEmitterContext context)
        {
            EmitScalarSaturatingUnaryOpSx(context, (op1) => context.Negate(op1));
        }

        public static void Sqneg_V(ArmEmitterContext context)
        {
            EmitVectorSaturatingUnaryOpSx(context, (op1) => context.Negate(op1));
        }

        public static void Sqrdmulh_S(ArmEmitterContext context)
        {
            EmitSaturatingBinaryOp(context, (op1, op2) => EmitDoublingMultiplyHighHalf(context, op1, op2, round: true), SaturatingFlags.ScalarSx);
        }

        public static void Sqrdmulh_V(ArmEmitterContext context)
        {
            EmitSaturatingBinaryOp(context, (op1, op2) => EmitDoublingMultiplyHighHalf(context, op1, op2, round: true), SaturatingFlags.VectorSx);
        }

        public static void Sqsub_S(ArmEmitterContext context)
        {
            EmitScalarSaturatingBinaryOpSx(context, SaturatingFlags.Sub);
        }

        public static void Sqsub_V(ArmEmitterContext context)
        {
            EmitVectorSaturatingBinaryOpSx(context, SaturatingFlags.Sub);
        }

        public static void Sqxtn_S(ArmEmitterContext context)
        {
            EmitSaturatingNarrowOp(context, SaturatingNarrowFlags.ScalarSxSx);
        }

        public static void Sqxtn_V(ArmEmitterContext context)
        {
            EmitSaturatingNarrowOp(context, SaturatingNarrowFlags.VectorSxSx);
        }

        public static void Sqxtun_S(ArmEmitterContext context)
        {
            EmitSaturatingNarrowOp(context, SaturatingNarrowFlags.ScalarSxZx);
        }

        public static void Sqxtun_V(ArmEmitterContext context)
        {
            EmitSaturatingNarrowOp(context, SaturatingNarrowFlags.VectorSxZx);
        }

        public static void Srhadd_V(ArmEmitterContext context)
        {
            OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

            if (Optimizations.UseSse2 && op.Size < 2)
            {
                Operand n = GetVec(op.Rn);
                Operand m = GetVec(op.Rm);

                Operand mask = X86GetAllElements(context, (int)(op.Size == 0 ? 0x80808080u : 0x80008000u));

                Intrinsic subInst = X86PsubInstruction[op.Size];

                Operand nMinusMask = context.AddIntrinsic(subInst, n, mask);
                Operand mMinusMask = context.AddIntrinsic(subInst, m, mask);

                Intrinsic avgInst = op.Size == 0 ? Intrinsic.X86Pavgb : Intrinsic.X86Pavgw;

                Operand res = context.AddIntrinsic(avgInst, nMinusMask, mMinusMask);

                Intrinsic addInst = X86PaddInstruction[op.Size];

                res = context.AddIntrinsic(addInst, mask, res);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    res = context.VectorZeroUpper64(res);
                }

                context.Copy(GetVec(op.Rd), res);
            }
            else
            {
                EmitVectorBinaryOpSx(context, (op1, op2) =>
                {
                    Operand res = context.Add(op1, op2);

                    res = context.Add(res, Const(1L));

                    return context.ShiftRightSI(res, Const(1));
                });
            }
        }

        public static void Ssubl_V(ArmEmitterContext context)
        {
            if (Optimizations.UseSse41)
            {
                OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

                Operand n = GetVec(op.Rn);
                Operand m = GetVec(op.Rm);

                if (op.RegisterSize == RegisterSize.Simd128)
                {
                    n = context.AddIntrinsic(Intrinsic.X86Psrldq, n, Const(8));
                    m = context.AddIntrinsic(Intrinsic.X86Psrldq, m, Const(8));
                }

                Intrinsic movInst = X86PmovsxInstruction[op.Size];

                n = context.AddIntrinsic(movInst, n);
                m = context.AddIntrinsic(movInst, m);

                Intrinsic subInst = X86PsubInstruction[op.Size + 1];

                context.Copy(GetVec(op.Rd), context.AddIntrinsic(subInst, n, m));
            }
            else
            {
                EmitVectorWidenRnRmBinaryOpSx(context, (op1, op2) => context.Subtract(op1, op2));
            }
        }

        public static void Ssubw_V(ArmEmitterContext context)
        {
            if (Optimizations.UseSse41)
            {
                OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

                Operand n = GetVec(op.Rn);
                Operand m = GetVec(op.Rm);

                if (op.RegisterSize == RegisterSize.Simd128)
                {
                    m = context.AddIntrinsic(Intrinsic.X86Psrldq, m, Const(8));
                }

                Intrinsic movInst = X86PmovsxInstruction[op.Size];

                m = context.AddIntrinsic(movInst, m);

                Intrinsic subInst = X86PsubInstruction[op.Size + 1];

                context.Copy(GetVec(op.Rd), context.AddIntrinsic(subInst, n, m));
            }
            else
            {
                EmitVectorWidenRmBinaryOpSx(context, (op1, op2) => context.Subtract(op1, op2));
            }
        }

        public static void Sub_S(ArmEmitterContext context)
        {
            EmitScalarBinaryOpZx(context, (op1, op2) => context.Subtract(op1, op2));
        }

        public static void Sub_V(ArmEmitterContext context)
        {
            if (Optimizations.UseSse2)
            {
                OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

                Operand n = GetVec(op.Rn);
                Operand m = GetVec(op.Rm);

                Intrinsic subInst = X86PsubInstruction[op.Size];

                Operand res = context.AddIntrinsic(subInst, n, m);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    res = context.VectorZeroUpper64(res);
                }

                context.Copy(GetVec(op.Rd), res);
            }
            else
            {
                EmitVectorBinaryOpZx(context, (op1, op2) => context.Subtract(op1, op2));
            }
        }

        public static void Subhn_V(ArmEmitterContext context)
        {
            EmitHighNarrow(context, (op1, op2) => context.Subtract(op1, op2), round: false);
        }

        public static void Suqadd_S(ArmEmitterContext context)
        {
            EmitScalarSaturatingBinaryOpSx(context, SaturatingFlags.Accumulate);
        }

        public static void Suqadd_V(ArmEmitterContext context)
        {
            EmitVectorSaturatingBinaryOpSx(context, SaturatingFlags.Accumulate);
        }

        public static void Uaba_V(ArmEmitterContext context)
        {
            EmitVectorTernaryOpZx(context, (op1, op2, op3) =>
            {
                return context.Add(op1, EmitAbs(context, context.Subtract(op2, op3)));
            });
        }

        public static void Uabal_V(ArmEmitterContext context)
        {
            EmitVectorWidenRnRmTernaryOpZx(context, (op1, op2, op3) =>
            {
                return context.Add(op1, EmitAbs(context, context.Subtract(op2, op3)));
            });
        }

        public static void Uabd_V(ArmEmitterContext context)
        {
            if (Optimizations.UseSse41)
            {
                OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

                Operand n = GetVec(op.Rn);
                Operand m = GetVec(op.Rm);

                EmitSse41Uabd(context, op, n, m, isLong: false);
            }
            else
            {
                EmitVectorBinaryOpZx(context, (op1, op2) =>
                {
                    return EmitAbs(context, context.Subtract(op1, op2));
                });
            }
        }

        public static void Uabdl_V(ArmEmitterContext context)
        {
            OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

            if (Optimizations.UseSse41 && op.Size < 2)
            {
                Operand n = GetVec(op.Rn);
                Operand m = GetVec(op.Rm);

                if (op.RegisterSize == RegisterSize.Simd128)
                {
                    n = context.AddIntrinsic(Intrinsic.X86Psrldq, n, Const(8));
                    m = context.AddIntrinsic(Intrinsic.X86Psrldq, m, Const(8));
                }

                Intrinsic movInst = op.Size == 0
                    ? Intrinsic.X86Pmovzxbw
                    : Intrinsic.X86Pmovzxwd;

                n = context.AddIntrinsic(movInst, n);
                m = context.AddIntrinsic(movInst, m);

                EmitSse41Uabd(context, op, n, m, isLong: true);
            }
            else
            {
                EmitVectorWidenRnRmBinaryOpZx(context, (op1, op2) =>
                {
                    return EmitAbs(context, context.Subtract(op1, op2));
                });
            }
        }

        public static void Uadalp_V(ArmEmitterContext context)
        {
            EmitAddLongPairwise(context, signed: false, accumulate: true);
        }

        public static void Uaddl_V(ArmEmitterContext context)
        {
            if (Optimizations.UseSse41)
            {
                OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

                Operand n = GetVec(op.Rn);
                Operand m = GetVec(op.Rm);

                if (op.RegisterSize == RegisterSize.Simd128)
                {
                    n = context.AddIntrinsic(Intrinsic.X86Psrldq, n, Const(8));
                    m = context.AddIntrinsic(Intrinsic.X86Psrldq, m, Const(8));
                }

                Intrinsic movInst = X86PmovzxInstruction[op.Size];

                n = context.AddIntrinsic(movInst, n);
                m = context.AddIntrinsic(movInst, m);

                Intrinsic addInst = X86PaddInstruction[op.Size + 1];

                context.Copy(GetVec(op.Rd), context.AddIntrinsic(addInst, n, m));
            }
            else
            {
                EmitVectorWidenRnRmBinaryOpZx(context, (op1, op2) => context.Add(op1, op2));
            }
        }

        public static void Uaddlp_V(ArmEmitterContext context)
        {
            EmitAddLongPairwise(context, signed: false, accumulate: false);
        }

        public static void Uaddlv_V(ArmEmitterContext context)
        {
            EmitVectorLongAcrossVectorOpZx(context, (op1, op2) => context.Add(op1, op2));
        }

        public static void Uaddw_V(ArmEmitterContext context)
        {
            if (Optimizations.UseSse41)
            {
                OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

                Operand n = GetVec(op.Rn);
                Operand m = GetVec(op.Rm);

                if (op.RegisterSize == RegisterSize.Simd128)
                {
                    m = context.AddIntrinsic(Intrinsic.X86Psrldq, m, Const(8));
                }

                Intrinsic movInst = X86PmovzxInstruction[op.Size];

                m = context.AddIntrinsic(movInst, m);

                Intrinsic addInst = X86PaddInstruction[op.Size + 1];

                context.Copy(GetVec(op.Rd), context.AddIntrinsic(addInst, n, m));
            }
            else
            {
                EmitVectorWidenRmBinaryOpZx(context, (op1, op2) => context.Add(op1, op2));
            }
        }

        public static void Uhadd_V(ArmEmitterContext context)
        {
            OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

            if (Optimizations.UseSse2 && op.Size > 0)
            {
                Operand n = GetVec(op.Rn);
                Operand m = GetVec(op.Rm);

                Operand res  = context.AddIntrinsic(Intrinsic.X86Pand, n, m);
                Operand res2 = context.AddIntrinsic(Intrinsic.X86Pxor, n, m);

                Intrinsic shiftInst = op.Size == 1 ? Intrinsic.X86Psrlw : Intrinsic.X86Psrld;

                res2 = context.AddIntrinsic(shiftInst, res2, Const(1));

                Intrinsic addInst = X86PaddInstruction[op.Size];

                res = context.AddIntrinsic(addInst, res, res2);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    res = context.VectorZeroUpper64(res);
                }

                context.Copy(GetVec(op.Rd), res);
            }
            else
            {
                EmitVectorBinaryOpZx(context, (op1, op2) =>
                {
                    return context.ShiftRightUI(context.Add(op1, op2), Const(1));
                });
            }
        }

        public static void Uhsub_V(ArmEmitterContext context)
        {
            OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

            if (Optimizations.UseSse2 && op.Size < 2)
            {
                Operand n = GetVec(op.Rn);
                Operand m = GetVec(op.Rm);

                Intrinsic avgInst = op.Size == 0 ? Intrinsic.X86Pavgb : Intrinsic.X86Pavgw;

                Operand res = context.AddIntrinsic(avgInst, n, m);

                Intrinsic subInst = X86PsubInstruction[op.Size];

                res = context.AddIntrinsic(subInst, n, res);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    res = context.VectorZeroUpper64(res);
                }

                context.Copy(GetVec(op.Rd), res);
            }
            else
            {
                EmitVectorBinaryOpZx(context, (op1, op2) =>
                {
                    return context.ShiftRightUI(context.Subtract(op1, op2), Const(1));
                });
            }
        }

        public static void Umax_V(ArmEmitterContext context)
        {
            if (Optimizations.UseSse41)
            {
                OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

                Operand n = GetVec(op.Rn);
                Operand m = GetVec(op.Rm);

                Intrinsic maxInst = X86PmaxuInstruction[op.Size];

                Operand res = context.AddIntrinsic(maxInst, n, m);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    res = context.VectorZeroUpper64(res);
                }

                context.Copy(GetVec(op.Rd), res);
            }
            else
            {
                Delegate dlg = new _U64_U64_U64(Math.Max);

                EmitVectorBinaryOpZx(context, (op1, op2) => context.Call(dlg, op1, op2));
            }
        }

        public static void Umaxp_V(ArmEmitterContext context)
        {
            Delegate dlg = new _U64_U64_U64(Math.Max);

            EmitVectorPairwiseOpZx(context, (op1, op2) => context.Call(dlg, op1, op2));
        }

        public static void Umaxv_V(ArmEmitterContext context)
        {
            Delegate dlg = new _U64_U64_U64(Math.Max);

            EmitVectorAcrossVectorOpZx(context, (op1, op2) => context.Call(dlg, op1, op2));
        }

        public static void Umin_V(ArmEmitterContext context)
        {
            if (Optimizations.UseSse41)
            {
                OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

                Operand n = GetVec(op.Rn);
                Operand m = GetVec(op.Rm);

                Intrinsic minInst = X86PminuInstruction[op.Size];

                Operand res = context.AddIntrinsic(minInst, n, m);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    res = context.VectorZeroUpper64(res);
                }

                context.Copy(GetVec(op.Rd), res);
            }
            else
            {
                Delegate dlg = new _U64_U64_U64(Math.Min);

                EmitVectorBinaryOpZx(context, (op1, op2) => context.Call(dlg, op1, op2));
            }
        }

        public static void Uminp_V(ArmEmitterContext context)
        {
            Delegate dlg = new _U64_U64_U64(Math.Min);

            EmitVectorPairwiseOpZx(context, (op1, op2) => context.Call(dlg, op1, op2));
        }

        public static void Uminv_V(ArmEmitterContext context)
        {
            Delegate dlg = new _U64_U64_U64(Math.Min);

            EmitVectorAcrossVectorOpZx(context, (op1, op2) => context.Call(dlg, op1, op2));
        }

        public static void Umlal_V(ArmEmitterContext context)
        {
            OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

            if (Optimizations.UseSse41 && op.Size < 2)
            {
                Operand d = GetVec(op.Rd);
                Operand n = GetVec(op.Rn);
                Operand m = GetVec(op.Rm);

                if (op.RegisterSize == RegisterSize.Simd128)
                {
                    n = context.AddIntrinsic(Intrinsic.X86Psrldq, n, Const(8));
                    m = context.AddIntrinsic(Intrinsic.X86Psrldq, m, Const(8));
                }

                Intrinsic movInst = X86PmovzxInstruction[op.Size];

                n = context.AddIntrinsic(movInst, n);
                m = context.AddIntrinsic(movInst, m);

                Intrinsic mullInst = op.Size == 0 ? Intrinsic.X86Pmullw : Intrinsic.X86Pmulld;

                Operand res = context.AddIntrinsic(mullInst, n, m);

                Intrinsic addInst = X86PaddInstruction[op.Size + 1];

                context.Copy(d, context.AddIntrinsic(addInst, d, res));
            }
            else
            {
                EmitVectorWidenRnRmTernaryOpZx(context, (op1, op2, op3) =>
                {
                    return context.Add(op1, context.Multiply(op2, op3));
                });
            }
        }

        public static void Umlal_Ve(ArmEmitterContext context)
        {
            EmitVectorWidenTernaryOpByElemZx(context, (op1, op2, op3) =>
            {
                return context.Add(op1, context.Multiply(op2, op3));
            });
        }

        public static void Umlsl_V(ArmEmitterContext context)
        {
            OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

            if (Optimizations.UseSse41 && op.Size < 2)
            {
                Operand d = GetVec(op.Rd);
                Operand n = GetVec(op.Rn);
                Operand m = GetVec(op.Rm);

                if (op.RegisterSize == RegisterSize.Simd128)
                {
                    n = context.AddIntrinsic(Intrinsic.X86Psrldq, n, Const(8));
                    m = context.AddIntrinsic(Intrinsic.X86Psrldq, m, Const(8));
                }

                Intrinsic movInst = op.Size == 0
                    ? Intrinsic.X86Pmovzxbw
                    : Intrinsic.X86Pmovzxwd;

                n = context.AddIntrinsic(movInst, n);
                m = context.AddIntrinsic(movInst, m);

                Intrinsic mullInst = op.Size == 0 ? Intrinsic.X86Pmullw : Intrinsic.X86Pmulld;

                Operand res = context.AddIntrinsic(mullInst, n, m);

                Intrinsic subInst = X86PsubInstruction[op.Size + 1];

                context.Copy(d, context.AddIntrinsic(subInst, d, res));
            }
            else
            {
                EmitVectorWidenRnRmTernaryOpZx(context, (op1, op2, op3) =>
                {
                    return context.Subtract(op1, context.Multiply(op2, op3));
                });
            }
        }

        public static void Umlsl_Ve(ArmEmitterContext context)
        {
            EmitVectorWidenTernaryOpByElemZx(context, (op1, op2, op3) =>
            {
                return context.Subtract(op1, context.Multiply(op2, op3));
            });
        }

        public static void Umull_V(ArmEmitterContext context)
        {
            EmitVectorWidenRnRmBinaryOpZx(context, (op1, op2) => context.Multiply(op1, op2));
        }

        public static void Umull_Ve(ArmEmitterContext context)
        {
            EmitVectorWidenBinaryOpByElemZx(context, (op1, op2) => context.Multiply(op1, op2));
        }

        public static void Uqadd_S(ArmEmitterContext context)
        {
            EmitScalarSaturatingBinaryOpZx(context, SaturatingFlags.Add);
        }

        public static void Uqadd_V(ArmEmitterContext context)
        {
            EmitVectorSaturatingBinaryOpZx(context, SaturatingFlags.Add);
        }

        public static void Uqsub_S(ArmEmitterContext context)
        {
            EmitScalarSaturatingBinaryOpZx(context, SaturatingFlags.Sub);
        }

        public static void Uqsub_V(ArmEmitterContext context)
        {
            EmitVectorSaturatingBinaryOpZx(context, SaturatingFlags.Sub);
        }

        public static void Uqxtn_S(ArmEmitterContext context)
        {
            EmitSaturatingNarrowOp(context, SaturatingNarrowFlags.ScalarZxZx);
        }

        public static void Uqxtn_V(ArmEmitterContext context)
        {
            EmitSaturatingNarrowOp(context, SaturatingNarrowFlags.VectorZxZx);
        }

        public static void Urhadd_V(ArmEmitterContext context)
        {
            OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

            if (Optimizations.UseSse2 && op.Size < 2)
            {
                Operand n = GetVec(op.Rn);
                Operand m = GetVec(op.Rm);

                Intrinsic avgInst = op.Size == 0 ? Intrinsic.X86Pavgb : Intrinsic.X86Pavgw;

                Operand res = context.AddIntrinsic(avgInst, n, m);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    res = context.VectorZeroUpper64(res);
                }

                context.Copy(GetVec(op.Rd), res);
            }
            else
            {
                EmitVectorBinaryOpZx(context, (op1, op2) =>
                {
                    Operand res = context.Add(op1, op2);

                    res = context.Add(res, Const(1L));

                    return context.ShiftRightUI(res, Const(1));
                });
            }
        }

        public static void Usqadd_S(ArmEmitterContext context)
        {
            EmitScalarSaturatingBinaryOpZx(context, SaturatingFlags.Accumulate);
        }

        public static void Usqadd_V(ArmEmitterContext context)
        {
            EmitVectorSaturatingBinaryOpZx(context, SaturatingFlags.Accumulate);
        }

        public static void Usubl_V(ArmEmitterContext context)
        {
            if (Optimizations.UseSse41)
            {
                OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

                Operand n = GetVec(op.Rn);
                Operand m = GetVec(op.Rm);

                if (op.RegisterSize == RegisterSize.Simd128)
                {
                    n = context.AddIntrinsic(Intrinsic.X86Psrldq, n, Const(8));
                    m = context.AddIntrinsic(Intrinsic.X86Psrldq, m, Const(8));
                }

                Intrinsic movInst = X86PmovzxInstruction[op.Size];

                n = context.AddIntrinsic(movInst, n);
                m = context.AddIntrinsic(movInst, m);

                Intrinsic subInst = X86PsubInstruction[op.Size + 1];

                context.Copy(GetVec(op.Rd), context.AddIntrinsic(subInst, n, m));
            }
            else
            {
                EmitVectorWidenRnRmBinaryOpZx(context, (op1, op2) => context.Subtract(op1, op2));
            }
        }

        public static void Usubw_V(ArmEmitterContext context)
        {
            if (Optimizations.UseSse41)
            {
                OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

                Operand n = GetVec(op.Rn);
                Operand m = GetVec(op.Rm);

                if (op.RegisterSize == RegisterSize.Simd128)
                {
                    m = context.AddIntrinsic(Intrinsic.X86Psrldq, m, Const(8));
                }

                Intrinsic movInst = X86PmovzxInstruction[op.Size];

                m = context.AddIntrinsic(movInst, m);

                Intrinsic subInst = X86PsubInstruction[op.Size + 1];

                context.Copy(GetVec(op.Rd), context.AddIntrinsic(subInst, n, m));
            }
            else
            {
                EmitVectorWidenRmBinaryOpZx(context, (op1, op2) => context.Subtract(op1, op2));
            }
        }

        private static Operand EmitAbs(ArmEmitterContext context, Operand value)
        {
            Operand isPositive = context.ICompareGreaterOrEqual(value, Const(value.Type, 0));

            return context.ConditionalSelect(isPositive, value, context.Negate(value));
        }

        private static void EmitAddLongPairwise(ArmEmitterContext context, bool signed, bool accumulate)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            Operand res = context.VectorZero();

            int pairs = op.GetPairsCount() >> op.Size;

            for (int index = 0; index < pairs; index++)
            {
                int pairIndex = index << 1;

                Operand ne0 = EmitVectorExtract(context, op.Rn, pairIndex,     op.Size, signed);
                Operand ne1 = EmitVectorExtract(context, op.Rn, pairIndex + 1, op.Size, signed);

                Operand e = context.Add(ne0, ne1);

                if (accumulate)
                {
                    Operand de = EmitVectorExtract(context, op.Rd, index, op.Size + 1, signed);

                    e = context.Add(e, de);
                }

                res = EmitVectorInsert(context, res, e, index, op.Size + 1);
            }

            context.Copy(GetVec(op.Rd), res);
        }

        private static Operand EmitDoublingMultiplyHighHalf(
            ArmEmitterContext context,
            Operand n,
            Operand m,
            bool round)
        {
            OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

            int eSize = 8 << op.Size;

            Operand res = context.Multiply(n, m);

            if (!round)
            {
                res = context.ShiftRightSI(res, Const(eSize - 1));
            }
            else
            {
                long roundConst = 1L << (eSize - 1);

                res = context.ShiftLeft(res, Const(1));

                res = context.Add(res, Const(roundConst));

                res = context.ShiftRightSI(res, Const(eSize));

                Operand isIntMin = context.ICompareEqual(res, Const((long)int.MinValue));

                res = context.ConditionalSelect(isIntMin, context.Negate(res), res);
            }

            return res;
        }

        private static void EmitHighNarrow(ArmEmitterContext context, Func2I emit, bool round)
        {
            OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

            int elems = 8 >> op.Size;
            int eSize = 8 << op.Size;

            int part = op.RegisterSize == RegisterSize.Simd128 ? elems : 0;

            Operand res = part == 0 ? context.VectorZero() : context.Copy(GetVec(op.Rd));

            long roundConst = 1L << (eSize - 1);

            for (int index = 0; index < elems; index++)
            {
                Operand ne = EmitVectorExtractZx(context, op.Rn, index, op.Size + 1);
                Operand me = EmitVectorExtractZx(context, op.Rm, index, op.Size + 1);

                Operand de = emit(ne, me);

                if (round)
                {
                    de = context.Add(de, Const(roundConst));
                }

                de = context.ShiftRightUI(de, Const(eSize));

                res = EmitVectorInsert(context, res, de, part + index, op.Size);
            }

            context.Copy(GetVec(op.Rd), res);
        }

        public static void EmitScalarRoundOpF(ArmEmitterContext context, FPRoundingMode roundMode)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            Operand n = GetVec(op.Rn);

            Intrinsic inst = (op.Size & 1) != 0 ? Intrinsic.X86Roundsd : Intrinsic.X86Roundss;

            Operand res = context.AddIntrinsic(inst, n, Const(X86GetRoundControl(roundMode)));

            if ((op.Size & 1) != 0)
            {
                res = context.VectorZeroUpper64(res);
            }
            else
            {
                res = context.VectorZeroUpper96(res);
            }

            context.Copy(GetVec(op.Rd), res);
        }

        public static void EmitVectorRoundOpF(ArmEmitterContext context, FPRoundingMode roundMode)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            Operand n = GetVec(op.Rn);

            Intrinsic inst = (op.Size & 1) != 0 ? Intrinsic.X86Roundpd : Intrinsic.X86Roundps;

            Operand res = context.AddIntrinsic(inst, n, Const(X86GetRoundControl(roundMode)));

            if (op.RegisterSize == RegisterSize.Simd64)
            {
                res = context.VectorZeroUpper64(res);
            }

            context.Copy(GetVec(op.Rd), res);
        }

        private enum AddSub
        {
            None,
            Add,
            Subtract
        }

        private static void EmitSse41Mul_AddSub(ArmEmitterContext context, AddSub addSub)
        {
            OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

            Operand n = GetVec(op.Rn);
            Operand m = GetVec(op.Rm);

            Operand res = null;

            if (op.Size == 0)
            {
                Operand ns8 = context.AddIntrinsic(Intrinsic.X86Psrlw, n, Const(8));
                Operand ms8 = context.AddIntrinsic(Intrinsic.X86Psrlw, m, Const(8));

                res = context.AddIntrinsic(Intrinsic.X86Pmullw, ns8, ms8);

                res = context.AddIntrinsic(Intrinsic.X86Psllw, res, Const(8));

                Operand res2 = context.AddIntrinsic(Intrinsic.X86Pmullw, n, m);

                Operand mask = X86GetAllElements(context, 0x00FF00FF);

                res = context.AddIntrinsic(Intrinsic.X86Pblendvb, res, res2, mask);
            }
            else if (op.Size == 1)
            {
                res = context.AddIntrinsic(Intrinsic.X86Pmullw, n, m);
            }
            else
            {
                res = context.AddIntrinsic(Intrinsic.X86Pmulld, n, m);
            }

            Operand d = GetVec(op.Rd);

            if (addSub == AddSub.Add)
            {
                switch (op.Size)
                {
                    case 0: res = context.AddIntrinsic(Intrinsic.X86Paddb, d, res); break;
                    case 1: res = context.AddIntrinsic(Intrinsic.X86Paddw, d, res); break;
                    case 2: res = context.AddIntrinsic(Intrinsic.X86Paddd, d, res); break;
                    case 3: res = context.AddIntrinsic(Intrinsic.X86Paddq, d, res); break;
                }
            }
            else if (addSub == AddSub.Subtract)
            {
                switch (op.Size)
                {
                    case 0: res = context.AddIntrinsic(Intrinsic.X86Psubb, d, res); break;
                    case 1: res = context.AddIntrinsic(Intrinsic.X86Psubw, d, res); break;
                    case 2: res = context.AddIntrinsic(Intrinsic.X86Psubd, d, res); break;
                    case 3: res = context.AddIntrinsic(Intrinsic.X86Psubq, d, res); break;
                }
            }

            if (op.RegisterSize == RegisterSize.Simd64)
            {
                res = context.VectorZeroUpper64(res);
            }

            context.Copy(d, res);
        }

        private static void EmitSse41Sabd(
            ArmEmitterContext context,
            OpCodeSimdReg op,
            Operand n,
            Operand m,
            bool isLong)
        {
            int size = isLong ? op.Size + 1 : op.Size;

            Intrinsic cmpgtInst = X86PcmpgtInstruction[size];

            Operand cmpMask = context.AddIntrinsic(cmpgtInst, n, m);

            Intrinsic subInst = X86PsubInstruction[size];

            Operand res = context.AddIntrinsic(subInst, n, m);

            res = context.AddIntrinsic(Intrinsic.X86Pand, cmpMask, res);

            Operand res2 = context.AddIntrinsic(subInst, m, n);

            res2 = context.AddIntrinsic(Intrinsic.X86Pandn, cmpMask, res2);

            res = context.AddIntrinsic(Intrinsic.X86Por, res, res2);

            if (!isLong && op.RegisterSize == RegisterSize.Simd64)
            {
                res = context.VectorZeroUpper64(res);
            }

            context.Copy(GetVec(op.Rd), res);
        }

        private static void EmitSse41Uabd(
            ArmEmitterContext context,
            OpCodeSimdReg op,
            Operand n,
            Operand m,
            bool isLong)
        {
            int size = isLong ? op.Size + 1 : op.Size;

            Intrinsic maxInst = X86PmaxuInstruction[size];

            Operand max = context.AddIntrinsic(maxInst, m, n);

            Intrinsic cmpeqInst = X86PcmpeqInstruction[size];

            Operand cmpMask = context.AddIntrinsic(cmpeqInst, max, m);

            Operand onesMask = X86GetAllElements(context, -1L);

            cmpMask = context.AddIntrinsic(Intrinsic.X86Pandn, cmpMask, onesMask);

            Intrinsic subInst = X86PsubInstruction[size];

            Operand res  = context.AddIntrinsic(subInst, n, m);
            Operand res2 = context.AddIntrinsic(subInst, m, n);

            res  = context.AddIntrinsic(Intrinsic.X86Pand,  cmpMask, res);
            res2 = context.AddIntrinsic(Intrinsic.X86Pandn, cmpMask, res2);

            res = context.AddIntrinsic(Intrinsic.X86Por, res, res2);

            if (!isLong && op.RegisterSize == RegisterSize.Simd64)
            {
                res = context.VectorZeroUpper64(res);
            }

            context.Copy(GetVec(op.Rd), res);
        }
    }
}
