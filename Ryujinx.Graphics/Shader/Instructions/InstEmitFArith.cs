using Ryujinx.Graphics.Shader.Decoders;
using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using Ryujinx.Graphics.Shader.Translation;
using System;

using static Ryujinx.Graphics.Shader.Instructions.InstEmitAluHelper;
using static Ryujinx.Graphics.Shader.Instructions.InstEmitHelper;
using static Ryujinx.Graphics.Shader.IntermediateRepresentation.OperandHelper;

namespace Ryujinx.Graphics.Shader.Instructions
{
    static partial class InstEmit
    {
        public static void Fadd(EmitterContext context)
        {
            IOpCodeFArith op = (IOpCodeFArith)context.CurrOp;

            bool absoluteA = op.AbsoluteA, absoluteB, negateA, negateB;

            if (op is OpCodeFArithImm32)
            {
                negateB   = op.RawOpCode.Extract(53);
                negateA   = op.RawOpCode.Extract(56);
                absoluteB = op.RawOpCode.Extract(57);
            }
            else
            {
                negateB   = op.RawOpCode.Extract(45);
                negateA   = op.RawOpCode.Extract(48);
                absoluteB = op.RawOpCode.Extract(49);
            }

            Operand srcA = context.FPAbsNeg(GetSrcA(context), absoluteA, negateA);
            Operand srcB = context.FPAbsNeg(GetSrcB(context), absoluteB, negateB);

            Operand dest = GetDest(context);

            context.Copy(dest, context.FPSaturate(context.FPAdd(srcA, srcB), op.Saturate));

            SetFPZnFlags(context, dest, op.SetCondCode);
        }

        public static void Ffma(EmitterContext context)
        {
            IOpCodeFArith op = (IOpCodeFArith)context.CurrOp;

            bool negateB = op.RawOpCode.Extract(48);
            bool negateC = op.RawOpCode.Extract(49);

            Operand srcA = GetSrcA(context);

            Operand srcB = context.FPNegate(GetSrcB(context), negateB);
            Operand srcC = context.FPNegate(GetSrcC(context), negateC);

            Operand dest = GetDest(context);

            context.Copy(dest, context.FPSaturate(context.FPFusedMultiplyAdd(srcA, srcB, srcC), op.Saturate));

            SetFPZnFlags(context, dest, op.SetCondCode);
        }

        public static void Fmnmx(EmitterContext context)
        {
            IOpCodeFArith op = (IOpCodeFArith)context.CurrOp;

            bool absoluteA = op.AbsoluteA;
            bool negateB   = op.RawOpCode.Extract(45);
            bool negateA   = op.RawOpCode.Extract(48);
            bool absoluteB = op.RawOpCode.Extract(49);

            Operand srcA = context.FPAbsNeg(GetSrcA(context), absoluteA, negateA);
            Operand srcB = context.FPAbsNeg(GetSrcB(context), absoluteB, negateB);

            Operand resMin = context.FPMinimum(srcA, srcB);
            Operand resMax = context.FPMaximum(srcA, srcB);

            Operand pred = GetPredicate39(context);

            Operand dest = GetDest(context);

            context.Copy(dest, context.ConditionalSelect(pred, resMin, resMax));

            SetFPZnFlags(context, dest, op.SetCondCode);
        }

        public static void Fmul(EmitterContext context)
        {
            IOpCodeFArith op = (IOpCodeFArith)context.CurrOp;

            bool negateB = !(op is OpCodeFArithImm32) && op.RawOpCode.Extract(48);

            Operand srcA = GetSrcA(context);

            Operand srcB = context.FPNegate(GetSrcB(context), negateB);

            switch (op.Scale)
            {
                case FmulScale.None: break;

                case FmulScale.Divide2:   srcA = context.FPDivide  (srcA, ConstF(2)); break;
                case FmulScale.Divide4:   srcA = context.FPDivide  (srcA, ConstF(4)); break;
                case FmulScale.Divide8:   srcA = context.FPDivide  (srcA, ConstF(8)); break;
                case FmulScale.Multiply2: srcA = context.FPMultiply(srcA, ConstF(2)); break;
                case FmulScale.Multiply4: srcA = context.FPMultiply(srcA, ConstF(4)); break;
                case FmulScale.Multiply8: srcA = context.FPMultiply(srcA, ConstF(8)); break;

                default: break; //TODO: Warning.
            }

            Operand dest = GetDest(context);

            context.Copy(dest, context.FPSaturate(context.FPMultiply(srcA, srcB), op.Saturate));

            SetFPZnFlags(context, dest, op.SetCondCode);
        }

        public static void Fset(EmitterContext context)
        {
            OpCodeSet op = (OpCodeSet)context.CurrOp;

            Condition cmpOp = (Condition)op.RawOpCode.Extract(48, 4);

            bool negateA   = op.RawOpCode.Extract(43);
            bool absoluteB = op.RawOpCode.Extract(44);
            bool boolFloat = op.RawOpCode.Extract(52);
            bool negateB   = op.RawOpCode.Extract(53);
            bool absoluteA = op.RawOpCode.Extract(54);

            Operand srcA = context.FPAbsNeg(GetSrcA(context), absoluteA, negateA);
            Operand srcB = context.FPAbsNeg(GetSrcB(context), absoluteB, negateB);

            Operand res = GetFPComparison(context, cmpOp, srcA, srcB);

            Operand pred = GetPredicate39(context);

            res = GetPredLogicalOp(context, op.LogicalOp, res, pred);

            Operand dest = GetDest(context);

            if (boolFloat)
            {
                context.Copy(dest, context.ConditionalSelect(res, ConstF(1), Const(0)));
            }
            else
            {
                context.Copy(dest, res);
            }

            // TODO: CC, X
        }

        public static void Fsetp(EmitterContext context)
        {
            OpCodeSet op = (OpCodeSet)context.CurrOp;

            Condition cmpOp = (Condition)op.RawOpCode.Extract(48, 4);

            bool absoluteA = op.RawOpCode.Extract(7);
            bool negateA   = op.RawOpCode.Extract(43);
            bool absoluteB = op.RawOpCode.Extract(44);
            bool negateB   = op.RawOpCode.Extract(6);

            Operand srcA = context.FPAbsNeg(GetSrcA(context), absoluteA, negateA);
            Operand srcB = context.FPAbsNeg(GetSrcB(context), absoluteB, negateB);

            Operand p0Res = GetFPComparison(context, cmpOp, srcA, srcB);

            Operand p1Res = context.BitwiseNot(p0Res);

            Operand pred = GetPredicate39(context);

            p0Res = GetPredLogicalOp(context, op.LogicalOp, p0Res, pred);
            p1Res = GetPredLogicalOp(context, op.LogicalOp, p1Res, pred);

            context.Copy(Register(op.Predicate3), p0Res);
            context.Copy(Register(op.Predicate0), p1Res);
        }

        public static void Hadd2(EmitterContext context)
        {
            Hadd2Hmul2Impl(context, isAdd: true);
        }

        public static void Hfma2(EmitterContext context)
        {
            IOpCodeHfma op = (IOpCodeHfma)context.CurrOp;

            Operand[] srcA = GetHfmaSrcA(context);
            Operand[] srcB = GetHfmaSrcB(context);
            Operand[] srcC = GetHfmaSrcC(context);

            Operand[] res = new Operand[2];

            for (int index = 0; index < res.Length; index++)
            {
                res[index] = context.FPFusedMultiplyAdd(srcA[index], srcB[index], srcC[index]);

                res[index] = context.FPSaturate(res[index], op.Saturate);
            }

            context.Copy(GetDest(context), GetHalfPacked(context, res));
        }

        public static void Hmul2(EmitterContext context)
        {
            Hadd2Hmul2Impl(context, isAdd: false);
        }

        private static void Hadd2Hmul2Impl(EmitterContext context, bool isAdd)
        {
            OpCode op = context.CurrOp;

            bool saturate = op.RawOpCode.Extract(op is OpCodeAluImm32 ? 52 : 32);

            Operand[] srcA = GetHalfSrcA(context);
            Operand[] srcB = GetHalfSrcB(context);

            Operand[] res = new Operand[2];

            for (int index = 0; index < res.Length; index++)
            {
                if (isAdd)
                {
                    res[index] = context.FPAdd(srcA[index], srcB[index]);
                }
                else
                {
                    res[index] = context.FPMultiply(srcA[index], srcB[index]);
                }

                res[index] = context.FPSaturate(res[index], saturate);
            }

            context.Copy(GetDest(context), GetHalfPacked(context, res));
        }

        public static void Mufu(EmitterContext context)
        {
            IOpCodeFArith op = (IOpCodeFArith)context.CurrOp;

            bool negateB = op.RawOpCode.Extract(48);

            Operand res = context.FPAbsNeg(GetSrcA(context), op.AbsoluteA, negateB);

            MufuOperation subOp = (MufuOperation)context.CurrOp.RawOpCode.Extract(20, 4);

            switch (subOp)
            {
                case MufuOperation.Cosine:
                    res = context.FPCosine(res);
                    break;

                case MufuOperation.Sine:
                    res = context.FPSine(res);
                    break;

                case MufuOperation.ExponentB2:
                    res = context.FPExponentB2(res);
                    break;

                case MufuOperation.LogarithmB2:
                    res = context.FPLogarithmB2(res);
                    break;

                case MufuOperation.Reciprocal:
                    res = context.FPReciprocal(res);
                    break;

                case MufuOperation.ReciprocalSquareRoot:
                    res = context.FPReciprocalSquareRoot(res);
                    break;

                case MufuOperation.SquareRoot:
                    res = context.FPSquareRoot(res);
                    break;

                default: /* TODO */ break;
            }

            context.Copy(GetDest(context), context.FPSaturate(res, op.Saturate));
        }

        private static Operand GetFPComparison(
            EmitterContext context,
            Condition      cond,
            Operand        srcA,
            Operand        srcB)
        {
            Operand res;

            if (cond == Condition.Always)
            {
                res = Const(IrConsts.True);
            }
            else if (cond == Condition.Never)
            {
                res = Const(IrConsts.False);
            }
            else if (cond == Condition.Nan || cond == Condition.Number)
            {
                res = context.BitwiseOr(context.IsNan(srcA), context.IsNan(srcB));

                if (cond == Condition.Number)
                {
                    res = context.BitwiseNot(res);
                }
            }
            else
            {
                Instruction inst;

                switch (cond & ~Condition.Nan)
                {
                    case Condition.Less:           inst = Instruction.CompareLess;           break;
                    case Condition.Equal:          inst = Instruction.CompareEqual;          break;
                    case Condition.LessOrEqual:    inst = Instruction.CompareLessOrEqual;    break;
                    case Condition.Greater:        inst = Instruction.CompareGreater;        break;
                    case Condition.NotEqual:       inst = Instruction.CompareNotEqual;       break;
                    case Condition.GreaterOrEqual: inst = Instruction.CompareGreaterOrEqual; break;

                    default: throw new InvalidOperationException($"Unexpected condition \"{cond}\".");
                }

                res = context.Add(inst | Instruction.FP, Local(), srcA, srcB);

                if ((cond & Condition.Nan) != 0)
                {
                    res = context.BitwiseOr(res, context.IsNan(srcA));
                    res = context.BitwiseOr(res, context.IsNan(srcB));
                }
            }

            return res;
        }

        private static void SetFPZnFlags(EmitterContext context, Operand dest, bool setCC)
        {
            if (setCC)
            {
                context.Copy(GetZF(context), context.FPCompareEqual(dest, ConstF(0)));
                context.Copy(GetNF(context), context.FPCompareLess (dest, ConstF(0)));
            }
        }

        private static Operand[] GetHfmaSrcA(EmitterContext context)
        {
            IOpCodeHfma op = (IOpCodeHfma)context.CurrOp;

            return GetHalfSources(context, GetSrcA(context), op.SwizzleA);
        }

        private static Operand[] GetHfmaSrcB(EmitterContext context)
        {
            IOpCodeHfma op = (IOpCodeHfma)context.CurrOp;

            Operand[] operands = GetHalfSources(context, GetSrcB(context), op.SwizzleB);

            return FPAbsNeg(context, operands, false, op.NegateB);
        }

        private static Operand[] GetHfmaSrcC(EmitterContext context)
        {
            IOpCodeHfma op = (IOpCodeHfma)context.CurrOp;

            Operand[] operands = GetHalfSources(context, GetSrcC(context), op.SwizzleC);

            return FPAbsNeg(context, operands, false, op.NegateC);
        }
    }
}