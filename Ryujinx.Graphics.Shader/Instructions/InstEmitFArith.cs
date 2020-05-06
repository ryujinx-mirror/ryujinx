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
        public static void Dadd(EmitterContext context) => EmitFPAdd(context, Instruction.FP64);
        public static void Dfma(EmitterContext context) => EmitFPFma(context, Instruction.FP64);
        public static void Dmul(EmitterContext context) => EmitFPMultiply(context, Instruction.FP64);

        public static void Fadd(EmitterContext context) => EmitFPAdd(context, Instruction.FP32);

        public static void Fcmp(EmitterContext context)
        {
            OpCode op = context.CurrOp;

            Condition cmpOp = (Condition)op.RawOpCode.Extract(48, 4);

            Operand srcA = GetSrcA(context);
            Operand srcB = GetSrcB(context);
            Operand srcC = GetSrcC(context);

            Operand cmpRes = GetFPComparison(context, cmpOp, srcC, ConstF(0));

            Operand res = context.ConditionalSelect(cmpRes, srcA, srcB);

            context.Copy(GetDest(context), res);
        }

        public static void Ffma(EmitterContext context) => EmitFPFma(context, Instruction.FP32);

        public static void Ffma32i(EmitterContext context)
        {
            IOpCodeFArith op = (IOpCodeFArith)context.CurrOp;

            bool saturate = op.RawOpCode.Extract(55);
            bool negateA  = op.RawOpCode.Extract(56);
            bool negateC  = op.RawOpCode.Extract(57);

            Operand srcA = context.FPNegate(GetSrcA(context), negateA);
            Operand srcC = context.FPNegate(GetDest(context), negateC);

            Operand srcB = GetSrcB(context);

            Operand dest = GetDest(context);

            context.Copy(dest, context.FPSaturate(context.FPFusedMultiplyAdd(srcA, srcB, srcC), saturate));

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

        public static void Fmul(EmitterContext context) => EmitFPMultiply(context, Instruction.FP32);

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
                res = context.ConditionalSelect(res, ConstF(1), Const(0));

                context.Copy(dest, res);

                SetFPZnFlags(context, res, op.SetCondCode);
            }
            else
            {
                context.Copy(dest, res);

                SetZnFlags(context, res, op.SetCondCode, op.Extended);
            }

            // TODO: X
        }

        public static void Fsetp(EmitterContext context)
        {
            OpCodeSet op = (OpCodeSet)context.CurrOp;

            Condition cmpOp = (Condition)op.RawOpCode.Extract(48, 4);

            bool negateB   = op.RawOpCode.Extract(6);
            bool absoluteA = op.RawOpCode.Extract(7);
            bool negateA   = op.RawOpCode.Extract(43);
            bool absoluteB = op.RawOpCode.Extract(44);

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

        public static void Fswzadd(EmitterContext context)
        {
            OpCodeAlu op = (OpCodeAlu)context.CurrOp;

            int mask = op.RawOpCode.Extract(28, 8);

            Operand srcA = GetSrcA(context);
            Operand srcB = GetSrcB(context);

            Operand dest = GetDest(context);

            context.Copy(dest, context.FPSwizzleAdd(srcA, srcB, mask));

            SetFPZnFlags(context, dest, op.SetCondCode);
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

            bool saturate = op.RawOpCode.Extract(op is IOpCodeReg ? 32 : 52);

            Operand[] srcA = GetHalfSrcA(context, isAdd);
            Operand[] srcB = GetHalfSrcB(context, !isAdd);

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

        public static void Hset2(EmitterContext context)
        {
            OpCodeSet op = (OpCodeSet)context.CurrOp;

            bool isRegVariant = op is IOpCodeReg;

            bool boolFloat = isRegVariant
                ? op.RawOpCode.Extract(49)
                : op.RawOpCode.Extract(53);

            Condition cmpOp = isRegVariant
                ? (Condition)op.RawOpCode.Extract(35, 4)
                : (Condition)op.RawOpCode.Extract(49, 4);

            Operand[] srcA = GetHalfSrcA(context);
            Operand[] srcB = GetHalfSrcB(context);

            Operand[] res = new Operand[2];

            res[0] = GetFPComparison(context, cmpOp, srcA[0], srcB[0]);
            res[1] = GetFPComparison(context, cmpOp, srcA[1], srcB[1]);

            Operand pred = GetPredicate39(context);

            res[0] = GetPredLogicalOp(context, op.LogicalOp, res[0], pred);
            res[1] = GetPredLogicalOp(context, op.LogicalOp, res[1], pred);

            if (boolFloat)
            {
                res[0] = context.ConditionalSelect(res[0], ConstF(1), Const(0));
                res[1] = context.ConditionalSelect(res[1], ConstF(1), Const(0));

                context.Copy(GetDest(context), context.PackHalf2x16(res[0], res[1]));
            }
            else
            {
                Operand low  = context.BitwiseAnd(res[0], Const(0xffff));
                Operand high = context.ShiftLeft (res[1], Const(16));

                Operand packed = context.BitwiseOr(low, high);

                context.Copy(GetDest(context), packed);
            }
        }

        public static void Hsetp2(EmitterContext context)
        {
            OpCodeSet op = (OpCodeSet)context.CurrOp;

            bool isRegVariant = op is IOpCodeReg;

            bool hAnd = isRegVariant
                ? op.RawOpCode.Extract(49)
                : op.RawOpCode.Extract(53);

            Condition cmpOp = isRegVariant
                ? (Condition)op.RawOpCode.Extract(35, 4)
                : (Condition)op.RawOpCode.Extract(49, 4);

            Operand[] srcA = GetHalfSrcA(context);
            Operand[] srcB = GetHalfSrcB(context);

            Operand p0Res = GetFPComparison(context, cmpOp, srcA[0], srcB[0]);
            Operand p1Res = GetFPComparison(context, cmpOp, srcA[1], srcB[1]);

            if (hAnd)
            {
                p0Res = context.BitwiseAnd(p0Res, p1Res);
                p1Res = context.BitwiseNot(p0Res);
            }

            Operand pred = GetPredicate39(context);

            p0Res = GetPredLogicalOp(context, op.LogicalOp, p0Res, pred);
            p1Res = GetPredLogicalOp(context, op.LogicalOp, p1Res, pred);

            context.Copy(Register(op.Predicate3), p0Res);
            context.Copy(Register(op.Predicate0), p1Res);
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

        private static void EmitFPAdd(EmitterContext context, Instruction fpType)
        {
            IOpCodeFArith op = (IOpCodeFArith)context.CurrOp;

            bool isFP64 = fpType == Instruction.FP64;

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

            Operand srcA = context.FPAbsNeg(GetSrcA(context, isFP64), absoluteA, negateA, fpType);
            Operand srcB = context.FPAbsNeg(GetSrcB(context, isFP64), absoluteB, negateB, fpType);

            Operand res = context.FPSaturate(context.FPAdd(srcA, srcB, fpType), op.Saturate, fpType);

            SetDest(context, res, isFP64);

            SetFPZnFlags(context, res, op.SetCondCode, fpType);
        }

        private static void EmitFPFma(EmitterContext context, Instruction fpType)
        {
            IOpCodeFArith op = (IOpCodeFArith)context.CurrOp;

            bool isFP64 = fpType == Instruction.FP64;

            bool negateB = op.RawOpCode.Extract(48);
            bool negateC = op.RawOpCode.Extract(49);

            Operand srcA = GetSrcA(context, isFP64);

            Operand srcB = context.FPNegate(GetSrcB(context, isFP64), negateB, fpType);
            Operand srcC = context.FPNegate(GetSrcC(context, isFP64), negateC, fpType);

            Operand res = context.FPSaturate(context.FPFusedMultiplyAdd(srcA, srcB, srcC, fpType), op.Saturate, fpType);

            SetDest(context, res, isFP64);

            SetFPZnFlags(context, res, op.SetCondCode, fpType);
        }

        private static void EmitFPMultiply(EmitterContext context, Instruction fpType)
        {
            IOpCodeFArith op = (IOpCodeFArith)context.CurrOp;

            bool isFP64 = fpType == Instruction.FP64;

            bool isImm32 = op is OpCodeFArithImm32;

            bool negateB = !isImm32 && op.RawOpCode.Extract(48);

            Operand srcA = GetSrcA(context, isFP64);

            Operand srcB = context.FPNegate(GetSrcB(context, isFP64), negateB, fpType);

            if (op.Scale != FPMultiplyScale.None)
            {
                Operand scale = op.Scale switch
                {
                    FPMultiplyScale.Divide2 => ConstF(0.5f),
                    FPMultiplyScale.Divide4 => ConstF(0.25f),
                    FPMultiplyScale.Divide8 => ConstF(0.125f),
                    FPMultiplyScale.Multiply2 => ConstF(2f),
                    FPMultiplyScale.Multiply4 => ConstF(4f),
                    FPMultiplyScale.Multiply8 => ConstF(8f),
                    _ => ConstF(1) // Invalid, behave as if it had no scale.
                };

                if (scale.AsFloat() == 1)
                {
                    context.Config.GpuAccessor.Log($"Invalid FP multiply scale \"{op.Scale}\".");
                }

                if (isFP64)
                {
                    scale = context.FP32ConvertToFP64(scale);
                }

                srcA = context.FPMultiply(srcA, scale, fpType);
            }

            bool saturate = isImm32 ? op.RawOpCode.Extract(55) : op.Saturate;

            Operand res = context.FPSaturate(context.FPMultiply(srcA, srcB, fpType), saturate, fpType);

            SetDest(context, res, isFP64);

            SetFPZnFlags(context, res, op.SetCondCode, fpType);
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

                res = context.Add(inst | Instruction.FP32, Local(), srcA, srcB);

                if ((cond & Condition.Nan) != 0)
                {
                    res = context.BitwiseOr(res, context.IsNan(srcA));
                    res = context.BitwiseOr(res, context.IsNan(srcB));
                }
            }

            return res;
        }

        private static Operand[] GetHfmaSrcA(EmitterContext context)
        {
            IOpCodeHfma op = (IOpCodeHfma)context.CurrOp;

            return GetHalfUnpacked(context, GetSrcA(context), op.SwizzleA);
        }

        private static Operand[] GetHfmaSrcB(EmitterContext context)
        {
            IOpCodeHfma op = (IOpCodeHfma)context.CurrOp;

            Operand[] operands = GetHalfUnpacked(context, GetSrcB(context), op.SwizzleB);

            return FPAbsNeg(context, operands, false, op.NegateB);
        }

        private static Operand[] GetHfmaSrcC(EmitterContext context)
        {
            IOpCodeHfma op = (IOpCodeHfma)context.CurrOp;

            Operand[] operands = GetHalfUnpacked(context, GetSrcC(context), op.SwizzleC);

            return FPAbsNeg(context, operands, false, op.NegateC);
        }

        private static void SetDest(EmitterContext context, Operand value, bool isFP64)
        {
            if (isFP64)
            {
                IOpCodeRd op = (IOpCodeRd)context.CurrOp;

                context.Copy(Register(op.Rd.Index, op.Rd.Type), context.UnpackDouble2x32Low(value));
                context.Copy(Register(op.Rd.Index | 1, op.Rd.Type), context.UnpackDouble2x32High(value));
            }
            else
            {
                context.Copy(GetDest(context), value);
            }
        }
    }
}