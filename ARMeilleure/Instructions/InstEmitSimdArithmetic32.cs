using ARMeilleure.Decoders;
using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.Translation;
using System;

using static ARMeilleure.Instructions.InstEmitFlowHelper;
using static ARMeilleure.Instructions.InstEmitHelper;
using static ARMeilleure.Instructions.InstEmitSimdHelper;
using static ARMeilleure.Instructions.InstEmitSimdHelper32;
using static ARMeilleure.IntermediateRepresentation.OperandHelper;

namespace ARMeilleure.Instructions
{
    static partial class InstEmit32
    {
        public static void Vabs_S(ArmEmitterContext context)
        {
            EmitScalarUnaryOpF32(context, (op1) => EmitUnaryMathCall(context, MathF.Abs, Math.Abs, op1));
        }

        public static void Vabs_V(ArmEmitterContext context)
        {
            OpCode32Simd op = (OpCode32Simd)context.CurrOp;

            if (op.F)
            {
                EmitVectorUnaryOpF32(context, (op1) => EmitUnaryMathCall(context, MathF.Abs, Math.Abs, op1));
            } 
            else
            {
                EmitVectorUnaryOpSx32(context, (op1) => EmitAbs(context, op1));
            }
        }

        private static Operand EmitAbs(ArmEmitterContext context, Operand value)
        {
            Operand isPositive = context.ICompareGreaterOrEqual(value, Const(value.Type, 0));

            return context.ConditionalSelect(isPositive, value, context.Negate(value));
        }

        public static void Vadd_S(ArmEmitterContext context)
        {
            if (Optimizations.FastFP)
            {
                EmitScalarBinaryOpF32(context, (op1, op2) => context.Add(op1, op2));
            }
            else
            {
                EmitScalarBinaryOpF32(context, (op1, op2) => EmitSoftFloatCall(context, SoftFloat32.FPAdd, SoftFloat64.FPAdd, op1, op2));
            }
        }

        public static void Vadd_V(ArmEmitterContext context)
        {
            if (Optimizations.FastFP)
            {
                EmitVectorBinaryOpF32(context, (op1, op2) => context.Add(op1, op2));
            } 
            else
            {
                EmitVectorBinaryOpF32(context, (op1, op2) => EmitSoftFloatCallDefaultFpscr(context, SoftFloat32.FPAddFpscr, SoftFloat64.FPAddFpscr, op1, op2));
            }
        }

        public static void Vadd_I(ArmEmitterContext context)
        {
            EmitVectorBinaryOpZx32(context, (op1, op2) => context.Add(op1, op2));
        }

        public static void Vdup(ArmEmitterContext context)
        {
            OpCode32SimdDupGP op = (OpCode32SimdDupGP)context.CurrOp;

            Operand insert = GetIntA32(context, op.Rt);

            // Zero extend into an I64, then replicate. Saves the most time over elementwise inserts.
            switch (op.Size)
            {
                case 2:
                    insert = context.Multiply(context.ZeroExtend32(OperandType.I64, insert), Const(0x0000000100000001u));
                    break;
                case 1:
                    insert = context.Multiply(context.ZeroExtend16(OperandType.I64, insert), Const(0x0001000100010001u));
                    break;
                case 0:
                    insert = context.Multiply(context.ZeroExtend8(OperandType.I64, insert), Const(0x0101010101010101u));
                    break;
                default:
                    throw new InvalidOperationException("Unknown Vdup Size.");
            }

            InsertScalar(context, op.Vd, insert);
            if (op.Q)
            {
                InsertScalar(context, op.Vd + 1, insert);
            }
        }

        public static void Vdup_1(ArmEmitterContext context)
        {
            OpCode32SimdDupElem op = (OpCode32SimdDupElem)context.CurrOp;

            Operand insert = EmitVectorExtractZx32(context, op.Vm >> 1, ((op.Vm & 1) << (3 - op.Size)) + op.Index, op.Size);

            // Zero extend into an I64, then replicate. Saves the most time over elementwise inserts.
            switch (op.Size)
            {
                case 2:
                    insert = context.Multiply(context.ZeroExtend32(OperandType.I64, insert), Const(0x0000000100000001u));
                    break;
                case 1:
                    insert = context.Multiply(context.ZeroExtend16(OperandType.I64, insert), Const(0x0001000100010001u));
                    break;
                case 0:
                    insert = context.Multiply(context.ZeroExtend8(OperandType.I64, insert), Const(0x0101010101010101u));
                    break;
                default:
                    throw new InvalidOperationException("Unknown Vdup Size.");
            }

            InsertScalar(context, op.Vd, insert);
            if (op.Q)
            {
                InsertScalar(context, op.Vd | 1, insert);
            }
        }

        public static void Vext(ArmEmitterContext context)
        {
            OpCode32SimdExt op = (OpCode32SimdExt)context.CurrOp;

            int elems = op.GetBytesCount();
            int byteOff = op.Immediate;

            Operand res = GetVecA32(op.Qd);

            for (int index = 0; index < elems; index++)
            {
                Operand extract;

                if (byteOff >= elems)
                {
                    extract = EmitVectorExtractZx32(context, op.Qm, op.Im + (byteOff - elems), op.Size);
                }
                else
                {
                    extract = EmitVectorExtractZx32(context, op.Qn, op.In + byteOff, op.Size);
                }
                byteOff++;

                res = EmitVectorInsert(context, res, extract, op.Id + index, op.Size);
            }

            context.Copy(GetVecA32(op.Qd), res);
        }

        public static void Vmov_S(ArmEmitterContext context)
        {
            EmitScalarUnaryOpF32(context, (op1) => op1);
        }

        public static void Vmovn(ArmEmitterContext context)
        {
            EmitVectorUnaryNarrowOp32(context, (op1) => op1);
        }

        public static void Vneg_S(ArmEmitterContext context)
        {
            EmitScalarUnaryOpF32(context, (op1) => context.Negate(op1));
        }

        public static void Vnmul_S(ArmEmitterContext context)
        {
            EmitScalarBinaryOpF32(context, (op1, op2) => context.Negate(context.Multiply(op1, op2)));
        }

        public static void Vnmla_S(ArmEmitterContext context)
        {
            if (Optimizations.FastFP)
            {
                EmitScalarTernaryOpF32(context, (op1, op2, op3) =>
                {
                    return context.Negate(context.Add(op1, context.Multiply(op2, op3)));
                });
            }
            else
            {
                EmitScalarTernaryOpF32(context, (op1, op2, op3) =>
                {
                    return EmitSoftFloatCall(context, SoftFloat32.FPNegMulAdd, SoftFloat64.FPNegMulAdd, op1, op2, op3);
                });
            }
        }

        public static void Vnmls_S(ArmEmitterContext context)
        {
            if (Optimizations.FastFP)
            {
                EmitScalarTernaryOpF32(context, (op1, op2, op3) =>
                {
                    return context.Add(context.Negate(op1), context.Multiply(op2, op3));
                });
            }
            else
            {
                EmitScalarTernaryOpF32(context, (op1, op2, op3) =>
                {
                    return EmitSoftFloatCall(context, SoftFloat32.FPNegMulSub, SoftFloat64.FPNegMulSub, op1, op2, op3);
                });
            }
        }

        public static void Vneg_V(ArmEmitterContext context)
        {
            if ((context.CurrOp as OpCode32Simd).F)
            {
                EmitVectorUnaryOpF32(context, (op1) => context.Negate(op1));
            } 
            else
            {
                EmitVectorUnaryOpSx32(context, (op1) => context.Negate(op1));
            }
        }

        public static void Vdiv_S(ArmEmitterContext context)
        {
            if (Optimizations.FastFP)
            {
                EmitScalarBinaryOpF32(context, (op1, op2) => context.Divide(op1, op2));
            }
            else
            {
                EmitScalarBinaryOpF32(context, (op1, op2) =>
                {
                    return EmitSoftFloatCall(context, SoftFloat32.FPDiv, SoftFloat64.FPDiv, op1, op2);
                });
            }
        }

        public static void Vmaxnm_S(ArmEmitterContext context)
        {
            EmitScalarBinaryOpF32(context, (op1, op2) => EmitSoftFloatCall(context, SoftFloat32.FPMaxNum, SoftFloat64.FPMaxNum, op1, op2));
        }

        public static void Vmaxnm_V(ArmEmitterContext context)
        {
            EmitVectorBinaryOpSx32(context, (op1, op2) => EmitSoftFloatCallDefaultFpscr(context, SoftFloat32.FPMaxNumFpscr, SoftFloat64.FPMaxNumFpscr, op1, op2));
        }

        public static void Vminnm_S(ArmEmitterContext context)
        {
            EmitScalarBinaryOpF32(context, (op1, op2) => EmitSoftFloatCall(context, SoftFloat32.FPMinNum, SoftFloat64.FPMinNum, op1, op2));
        }

        public static void Vminnm_V(ArmEmitterContext context)
        {
            EmitVectorBinaryOpSx32(context, (op1, op2) => EmitSoftFloatCallDefaultFpscr(context, SoftFloat32.FPMinNumFpscr, SoftFloat64.FPMinNumFpscr, op1, op2));
        }

        public static void Vmax_V(ArmEmitterContext context)
        {
            EmitVectorBinaryOpF32(context, (op1, op2) =>
            {
                return EmitSoftFloatCallDefaultFpscr(context, SoftFloat32.FPMaxFpscr, SoftFloat64.FPMaxFpscr, op1, op2);
            });
        }

        public static void Vmax_I(ArmEmitterContext context)
        {
            OpCode32SimdReg op = (OpCode32SimdReg)context.CurrOp;
            if (op.U)
            {
                EmitVectorBinaryOpZx32(context, (op1, op2) => context.ConditionalSelect(context.ICompareGreaterUI(op1, op2), op1, op2));
            } 
            else
            {
                EmitVectorBinaryOpSx32(context, (op1, op2) => context.ConditionalSelect(context.ICompareGreater(op1, op2), op1, op2));
            }
        }

        public static void Vmin_V(ArmEmitterContext context)
        {
            EmitVectorBinaryOpF32(context, (op1, op2) =>
            {
                return EmitSoftFloatCallDefaultFpscr(context, SoftFloat32.FPMinFpscr, SoftFloat64.FPMinFpscr, op1, op2);
            });
        }

        public static void Vmin_I(ArmEmitterContext context)
        {
            OpCode32SimdReg op = (OpCode32SimdReg)context.CurrOp;
            if (op.U)
            {
                EmitVectorBinaryOpZx32(context, (op1, op2) => context.ConditionalSelect(context.ICompareLessUI(op1, op2), op1, op2));
            }
            else
            {
                EmitVectorBinaryOpSx32(context, (op1, op2) => context.ConditionalSelect(context.ICompareLess(op1, op2), op1, op2));
            }
        }

        public static void Vmul_S(ArmEmitterContext context)
        {
            if (Optimizations.FastFP)
            {
                EmitScalarBinaryOpF32(context, (op1, op2) => context.Multiply(op1, op2));
            }
            else
            {
                EmitScalarBinaryOpF32(context, (op1, op2) =>
                {
                    return EmitSoftFloatCall(context, SoftFloat32.FPMul, SoftFloat64.FPMul, op1, op2);
                });
            }
        }

        public static void Vmul_V(ArmEmitterContext context)
        {
            if (Optimizations.FastFP)
            {
                EmitVectorBinaryOpF32(context, (op1, op2) => context.Multiply(op1, op2));
            }
            else
            {
                EmitVectorBinaryOpF32(context, (op1, op2) =>
                {
                    return EmitSoftFloatCallDefaultFpscr(context, SoftFloat32.FPMulFpscr, SoftFloat64.FPMulFpscr, op1, op2);
                });
            }
        }

        public static void Vmul_I(ArmEmitterContext context)
        {
            if ((context.CurrOp as OpCode32SimdReg).U) throw new NotImplementedException("Polynomial mode not implemented");
            EmitVectorBinaryOpSx32(context, (op1, op2) => context.Multiply(op1, op2));
        }

        public static void Vmul_1(ArmEmitterContext context)
        {
            OpCode32SimdRegElem op = (OpCode32SimdRegElem)context.CurrOp;

            if (op.F)
            {
                if (Optimizations.FastFP)
                {
                    EmitVectorByScalarOpF32(context, (op1, op2) => context.Multiply(op1, op2));
                }
                else
                {
                    EmitVectorByScalarOpF32(context, (op1, op2) => EmitSoftFloatCallDefaultFpscr(context, SoftFloat32.FPMulFpscr, SoftFloat64.FPMulFpscr, op1, op2));
                }
            } 
            else
            {
                EmitVectorByScalarOpI32(context, (op1, op2) => context.Multiply(op1, op2), false);
            }
        }

        public static void Vmla_S(ArmEmitterContext context)
        {
            if (Optimizations.FastFP)
            {
                EmitScalarTernaryOpF32(context, (op1, op2, op3) =>
                {
                    return context.Add(op1, context.Multiply(op2, op3));
                });
            }
            else
            {
                EmitScalarTernaryOpF32(context, (op1, op2, op3) =>
                {
                    return EmitSoftFloatCall(context, SoftFloat32.FPMulAdd, SoftFloat64.FPMulAdd, op1, op2, op3);
                });
            }
        }

        public static void Vmla_V(ArmEmitterContext context)
        {
            if (Optimizations.FastFP)
            {
                EmitVectorTernaryOpF32(context, (op1, op2, op3) => context.Add(op1, context.Multiply(op2, op3)));
            }
            else
            {
                EmitVectorTernaryOpF32(context, (op1, op2, op3) =>
                {
                    return EmitSoftFloatCallDefaultFpscr(context, SoftFloat32.FPMulAddFpscr, SoftFloat64.FPMulAddFpscr, op1, op2, op3);
                });
            }
        }

        public static void Vmla_I(ArmEmitterContext context)
        {
            EmitVectorTernaryOpZx32(context, (op1, op2, op3) => context.Add(op1, context.Multiply(op2, op3)));
        }

        public static void Vmla_1(ArmEmitterContext context)
        {
            OpCode32SimdRegElem op = (OpCode32SimdRegElem)context.CurrOp;

            if (op.F)
            {
                if (Optimizations.FastFP)
                {
                    EmitVectorsByScalarOpF32(context, (op1, op2, op3) => context.Add(op1, context.Multiply(op2, op3)));
                }
                else
                {
                    EmitVectorsByScalarOpF32(context, (op1, op2, op3) => EmitSoftFloatCallDefaultFpscr(context, SoftFloat32.FPMulAddFpscr, SoftFloat64.FPMulAddFpscr, op1, op2, op3));
                }
            }
            else
            {
                EmitVectorsByScalarOpI32(context, (op1, op2, op3) => context.Add(op1, context.Multiply(op2, op3)), false);
            }
        }

        public static void Vmls_S(ArmEmitterContext context)
        {
            if (Optimizations.FastFP)
            {
                EmitScalarTernaryOpF32(context, (op1, op2, op3) =>
                {
                    return context.Subtract(op1, context.Multiply(op2, op3));
                });
            }
            else
            {
                EmitScalarTernaryOpF32(context, (op1, op2, op3) =>
                {
                    return EmitSoftFloatCall(context, SoftFloat32.FPMulSub, SoftFloat64.FPMulSub, op1, op2, op3);
                });
            }
        }

        public static void Vmls_V(ArmEmitterContext context)
        {
            if (Optimizations.FastFP)
            {
                EmitVectorTernaryOpF32(context, (op1, op2, op3) => context.Subtract(op1, context.Multiply(op2, op3)));
            }
            else
            {
                EmitVectorTernaryOpF32(context, (op1, op2, op3) =>
                {
                    return EmitSoftFloatCallDefaultFpscr(context, SoftFloat32.FPMulSubFpscr, SoftFloat64.FPMulSubFpscr, op1, op2, op3);
                });
            }
        }

        public static void Vmls_I(ArmEmitterContext context)
        {
            EmitVectorTernaryOpZx32(context, (op1, op2, op3) => context.Subtract(op1, context.Multiply(op2, op3)));
        }

        public static void Vmls_1(ArmEmitterContext context)
        {
            OpCode32SimdRegElem op = (OpCode32SimdRegElem)context.CurrOp;

            if (op.F)
            {
                if (Optimizations.FastFP)
                {
                    EmitVectorsByScalarOpF32(context, (op1, op2, op3) => context.Subtract(op1, context.Multiply(op2, op3)));
                }
                else
                {
                    EmitVectorsByScalarOpF32(context, (op1, op2, op3) => EmitSoftFloatCallDefaultFpscr(context, SoftFloat32.FPMulSubFpscr, SoftFloat64.FPMulSubFpscr, op1, op2, op3));
                }
            }
            else
            {
                EmitVectorsByScalarOpI32(context, (op1, op2, op3) => context.Subtract(op1, context.Multiply(op2, op3)), false);
            }
        }

        public static void Vpadd_V(ArmEmitterContext context)
        {
            EmitVectorPairwiseOpF32(context, (op1, op2) => context.Add(op1, op2));
        }

        public static void Vpadd_I(ArmEmitterContext context)
        {
            OpCode32SimdReg op = (OpCode32SimdReg)context.CurrOp;

            EmitVectorPairwiseOpI32(context, (op1, op2) => context.Add(op1, op2), !op.U);
        }

        public static void Vrev(ArmEmitterContext context)
        {
            OpCode32Simd op = (OpCode32Simd)context.CurrOp;

            EmitVectorUnaryOpZx32(context, (op1) =>
            {
                switch (op.Opc)
                {
                    case 0:
                        switch (op.Size) // Swap bytes.
                        {
                            default:
                                return op1;
                            case 1:
                                return InstEmitAluHelper.EmitReverseBytes16_32Op(context, op1);
                            case 2:
                            case 3:
                                return context.ByteSwap(op1);
                        }
                    case 1:
                        switch (op.Size)
                        {
                            default:
                                return op1;
                            case 2:
                                return context.BitwiseOr(context.ShiftRightUI(context.BitwiseAnd(op1, Const(0xffff0000)), Const(16)),
                                                            context.ShiftLeft(context.BitwiseAnd(op1, Const(0x0000ffff)), Const(16)));
                            case 3:
                                return context.BitwiseOr(
                                    context.BitwiseOr(context.ShiftRightUI(context.BitwiseAnd(op1, Const(0xffff000000000000ul)), Const(48)),
                                                         context.ShiftLeft(context.BitwiseAnd(op1, Const(0x000000000000fffful)), Const(48))),
                                    context.BitwiseOr(context.ShiftRightUI(context.BitwiseAnd(op1, Const(0x0000ffff00000000ul)), Const(16)),
                                                         context.ShiftLeft(context.BitwiseAnd(op1, Const(0x00000000ffff0000ul)), Const(16))));
                        }
                    case 2:
                        // Swap upper and lower halves.
                        return context.BitwiseOr(context.ShiftRightUI(context.BitwiseAnd(op1, Const(0xffffffff00000000ul)), Const(32)),
                                                    context.ShiftLeft(context.BitwiseAnd(op1, Const(0x00000000fffffffful)), Const(32)));
                }

                return op1;
            });
        }

        public static void Vrecpe(ArmEmitterContext context)
        {
            OpCode32SimdSqrte op = (OpCode32SimdSqrte)context.CurrOp;

            if (op.F)
            {
                EmitVectorUnaryOpF32(context, (op1) =>
                {
                    return EmitSoftFloatCallDefaultFpscr(context, SoftFloat32.FPRecipEstimateFpscr, SoftFloat64.FPRecipEstimateFpscr, op1);
                });
            } 
            else
            {
                throw new NotImplementedException("Integer Vrecpe not currently implemented.");
            }
        }

        public static void Vrecps(ArmEmitterContext context)
        {
            EmitVectorBinaryOpF32(context, (op1, op2) =>
            {
                return EmitSoftFloatCall(context, SoftFloat32.FPRecipStep, SoftFloat64.FPRecipStep, op1, op2);
            });
        }

        public static void Vrsqrte(ArmEmitterContext context)
        {
            OpCode32SimdSqrte op = (OpCode32SimdSqrte)context.CurrOp;

            if (op.F)
            {
                EmitVectorUnaryOpF32(context, (op1) =>
                {
                    return EmitSoftFloatCallDefaultFpscr(context, SoftFloat32.FPRSqrtEstimateFpscr, SoftFloat64.FPRSqrtEstimateFpscr, op1);
                });
            } 
            else
            {
                throw new NotImplementedException("Integer Vrsqrte not currently implemented.");
            }
        }

        public static void Vrsqrts(ArmEmitterContext context)
        {
            EmitVectorBinaryOpF32(context, (op1, op2) =>
            {
                return EmitSoftFloatCall(context, SoftFloat32.FPRSqrtStep, SoftFloat64.FPRSqrtStep, op1, op2);
            });
        }

        public static void Vsel(ArmEmitterContext context)
        {
            OpCode32SimdSel op = (OpCode32SimdSel)context.CurrOp;

            Operand condition = null;
            switch (op.Cc)
            {
                case OpCode32SimdSelMode.Eq:
                    condition = GetCondTrue(context, Condition.Eq);
                    break;
                case OpCode32SimdSelMode.Ge:
                    condition = GetCondTrue(context, Condition.Ge);
                    break;
                case OpCode32SimdSelMode.Gt:
                    condition = GetCondTrue(context, Condition.Gt);
                    break;
                case OpCode32SimdSelMode.Vs:
                    condition = GetCondTrue(context, Condition.Vs);
                    break;
            }

            EmitScalarBinaryOpI32(context, (op1, op2) =>
            {
                return context.ConditionalSelect(condition, op1, op2);
            });
        }

        public static void Vsqrt_S(ArmEmitterContext context)
        {
            EmitScalarUnaryOpF32(context, (op1) =>
            {
                return EmitSoftFloatCall(context, SoftFloat32.FPSqrt, SoftFloat64.FPSqrt, op1);
            });
        }

        public static void Vsub_S(ArmEmitterContext context)
        {
            EmitScalarBinaryOpF32(context, (op1, op2) => context.Subtract(op1, op2));
        }

        public static void Vsub_V(ArmEmitterContext context)
        {
            EmitVectorBinaryOpF32(context, (op1, op2) => context.Subtract(op1, op2));
        }

        public static void Vsub_I(ArmEmitterContext context)
        {
            EmitVectorBinaryOpZx32(context, (op1, op2) => context.Subtract(op1, op2));
        }
    }
}
