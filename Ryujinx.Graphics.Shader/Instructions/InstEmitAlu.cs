using Ryujinx.Graphics.Shader.Decoders;
using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using Ryujinx.Graphics.Shader.Translation;
using System;

using static Ryujinx.Graphics.Shader.Instructions.InstEmitHelper;
using static Ryujinx.Graphics.Shader.Instructions.InstEmitAluHelper;
using static Ryujinx.Graphics.Shader.IntermediateRepresentation.OperandHelper;

namespace Ryujinx.Graphics.Shader.Instructions
{
    static partial class InstEmit
    {
        public static void Bfe(EmitterContext context)
        {
            OpCodeAlu op = (OpCodeAlu)context.CurrOp;

            bool isReverse = op.RawOpCode.Extract(40);
            bool isSigned  = op.RawOpCode.Extract(48);

            Operand srcA = GetSrcA(context);
            Operand srcB = GetSrcB(context);

            if (isReverse)
            {
                srcA = context.BitfieldReverse(srcA);
            }

            Operand position = context.BitwiseAnd(srcB, Const(0xff));

            Operand size = context.BitfieldExtractU32(srcB, Const(8), Const(8));

            Operand res = isSigned
                ? context.BitfieldExtractS32(srcA, position, size)
                : context.BitfieldExtractU32(srcA, position, size);

            context.Copy(GetDest(context), res);

            // TODO: CC, X, corner cases
        }

        public static void Bfi(EmitterContext context)
        {
            OpCodeAlu op = (OpCodeAlu)context.CurrOp;

            Operand srcA = GetSrcA(context);
            Operand srcB = GetSrcB(context);
            Operand srcC = GetSrcC(context);

            Operand position = context.BitwiseAnd(srcB, Const(0xff));

            Operand size = context.BitfieldExtractU32(srcB, Const(8), Const(8));

            Operand res = context.BitfieldInsert(srcC, srcA, position, size);

            context.Copy(GetDest(context), res);
        }

        public static void Csetp(EmitterContext context)
        {
            OpCodePset op = (OpCodePset)context.CurrOp;

            // TODO: Implement that properly

            Operand p0Res = Const(IrConsts.True);

            Operand p1Res = context.BitwiseNot(p0Res);

            Operand pred = GetPredicate39(context);

            p0Res = GetPredLogicalOp(context, op.LogicalOp, p0Res, pred);
            p1Res = GetPredLogicalOp(context, op.LogicalOp, p1Res, pred);

            context.Copy(Register(op.Predicate3), p0Res);
            context.Copy(Register(op.Predicate0), p1Res);
        }

        public static void Flo(EmitterContext context)
        {
            OpCodeAlu op = (OpCodeAlu)context.CurrOp;

            bool invert     = op.RawOpCode.Extract(40);
            bool countZeros = op.RawOpCode.Extract(41);
            bool isSigned   = op.RawOpCode.Extract(48);

            Operand srcB = context.BitwiseNot(GetSrcB(context), invert);

            Operand res = isSigned
                ? context.FindFirstSetS32(srcB)
                : context.FindFirstSetU32(srcB);

            if (countZeros)
            {
                res = context.BitwiseExclusiveOr(res, Const(31));
            }

            context.Copy(GetDest(context), res);
        }

        public static void Iadd(EmitterContext context)
        {
            OpCodeAlu op = (OpCodeAlu)context.CurrOp;

            bool negateA = false, negateB = false;

            if (!(op is OpCodeAluImm32))
            {
                negateB = op.RawOpCode.Extract(48);
                negateA = op.RawOpCode.Extract(49);
            }
            else
            {
                // TODO: Other IADD32I variant without the negate.
                negateA = op.RawOpCode.Extract(56);
            }

            Operand srcA = context.INegate(GetSrcA(context), negateA);
            Operand srcB = context.INegate(GetSrcB(context), negateB);

            Operand res = context.IAdd(srcA, srcB);

            if (op.Extended)
            {
                res = context.IAdd(res, context.BitwiseAnd(GetCF(), Const(1)));
            }

            SetIaddFlags(context, res, srcA, srcB, op.SetCondCode, op.Extended);

            context.Copy(GetDest(context), res);
        }

        public static void Iadd3(EmitterContext context)
        {
            OpCodeAlu op = (OpCodeAlu)context.CurrOp;

            IntegerHalfPart partC = (IntegerHalfPart)op.RawOpCode.Extract(31, 2);
            IntegerHalfPart partB = (IntegerHalfPart)op.RawOpCode.Extract(33, 2);
            IntegerHalfPart partA = (IntegerHalfPart)op.RawOpCode.Extract(35, 2);

            IntegerShift mode = (IntegerShift)op.RawOpCode.Extract(37, 2);

            bool negateC = op.RawOpCode.Extract(49);
            bool negateB = op.RawOpCode.Extract(50);
            bool negateA = op.RawOpCode.Extract(51);

            Operand Extend(Operand src, IntegerHalfPart part)
            {
                if (!(op is OpCodeAluReg) || part == IntegerHalfPart.B32)
                {
                    return src;
                }

                if (part == IntegerHalfPart.H0)
                {
                    return context.BitwiseAnd(src, Const(0xffff));
                }
                else if (part == IntegerHalfPart.H1)
                {
                    return context.ShiftRightU32(src, Const(16));
                }
                else
                {
                    // TODO: Warning.
                }

                return src;
            }

            Operand srcA = context.INegate(Extend(GetSrcA(context), partA), negateA);
            Operand srcB = context.INegate(Extend(GetSrcB(context), partB), negateB);
            Operand srcC = context.INegate(Extend(GetSrcC(context), partC), negateC);

            Operand res = context.IAdd(srcA, srcB);

            if (op is OpCodeAluReg && mode != IntegerShift.NoShift)
            {
                if (mode == IntegerShift.ShiftLeft)
                {
                    res = context.ShiftLeft(res, Const(16));
                }
                else if (mode == IntegerShift.ShiftRight)
                {
                    res = context.ShiftRightU32(res, Const(16));
                }
                else
                {
                    // TODO: Warning.
                }
            }

            res = context.IAdd(res, srcC);

            context.Copy(GetDest(context), res);

            // TODO: CC, X, corner cases
        }

        public static void Icmp(EmitterContext context)
        {
            OpCode op = context.CurrOp;

            bool isSigned = op.RawOpCode.Extract(48);

            IntegerCondition cmpOp = (IntegerCondition)op.RawOpCode.Extract(49, 3);

            Operand srcA = GetSrcA(context);
            Operand srcB = GetSrcB(context);
            Operand srcC = GetSrcC(context);

            Operand cmpRes = GetIntComparison(context, cmpOp, srcC, Const(0), isSigned);

            Operand res = context.ConditionalSelect(cmpRes, srcA, srcB);

            context.Copy(GetDest(context), res);
        }

        public static void Imad(EmitterContext context)
        {
            bool signedA = context.CurrOp.RawOpCode.Extract(48);
            bool signedB = context.CurrOp.RawOpCode.Extract(53);
            bool high    = context.CurrOp.RawOpCode.Extract(54);

            Operand srcA = GetSrcA(context);
            Operand srcB = GetSrcB(context);
            Operand srcC = GetSrcC(context);

            Operand res;

            if (high)
            {
                if (signedA && signedB)
                {
                    res = context.MultiplyHighS32(srcA, srcB);
                }
                else
                {
                    res = context.MultiplyHighU32(srcA, srcB);

                    if (signedA)
                    {
                        res = context.IAdd(res, context.IMultiply(srcB, context.ShiftRightS32(srcA, Const(31))));
                    }
                    else if (signedB)
                    {
                        res = context.IAdd(res, context.IMultiply(srcA, context.ShiftRightS32(srcB, Const(31))));
                    }
                }
            }
            else
            {
                res = context.IMultiply(srcA, srcB);
            }

            res = context.IAdd(res, srcC);

            // TODO: CC, X, SAT, and more?

            context.Copy(GetDest(context), res);
        }

        public static void Imnmx(EmitterContext context)
        {
            OpCodeAlu op = (OpCodeAlu)context.CurrOp;

            bool isSignedInt = op.RawOpCode.Extract(48);

            Operand srcA = GetSrcA(context);
            Operand srcB = GetSrcB(context);

            Operand resMin = isSignedInt
                ? context.IMinimumS32(srcA, srcB)
                : context.IMinimumU32(srcA, srcB);

            Operand resMax = isSignedInt
                ? context.IMaximumS32(srcA, srcB)
                : context.IMaximumU32(srcA, srcB);

            Operand pred = GetPredicate39(context);

            Operand dest = GetDest(context);

            context.Copy(dest, context.ConditionalSelect(pred, resMin, resMax));

            SetZnFlags(context, dest, op.SetCondCode);

            // TODO: X flags.
        }

        public static void Iscadd(EmitterContext context)
        {
            OpCodeAlu op = (OpCodeAlu)context.CurrOp;

            bool negateA = false, negateB = false;

            if (!(op is OpCodeAluImm32))
            {
                negateB = op.RawOpCode.Extract(48);
                negateA = op.RawOpCode.Extract(49);
            }

            int shift = op is OpCodeAluImm32
                ? op.RawOpCode.Extract(53, 5)
                : op.RawOpCode.Extract(39, 5);

            Operand srcA = GetSrcA(context);
            Operand srcB = GetSrcB(context);

            srcA = context.ShiftLeft(srcA, Const(shift));

            srcA = context.INegate(srcA, negateA);
            srcB = context.INegate(srcB, negateB);

            Operand res = context.IAdd(srcA, srcB);

            SetIaddFlags(context, res, srcA, srcB, op.SetCondCode, false);

            context.Copy(GetDest(context), res);
        }

        public static void Iset(EmitterContext context)
        {
            OpCodeSet op = (OpCodeSet)context.CurrOp;

            bool boolFloat = op.RawOpCode.Extract(44);
            bool isSigned  = op.RawOpCode.Extract(48);

            IntegerCondition cmpOp = (IntegerCondition)op.RawOpCode.Extract(49, 3);

            Operand srcA = GetSrcA(context);
            Operand srcB = GetSrcB(context);

            Operand res = GetIntComparison(context, cmpOp, srcA, srcB, isSigned, op.Extended);

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
        }

        public static void Isetp(EmitterContext context)
        {
            OpCodeSet op = (OpCodeSet)context.CurrOp;

            bool isSigned = op.RawOpCode.Extract(48);

            IntegerCondition cmpOp = (IntegerCondition)op.RawOpCode.Extract(49, 3);

            Operand srcA = GetSrcA(context);
            Operand srcB = GetSrcB(context);

            Operand p0Res = GetIntComparison(context, cmpOp, srcA, srcB, isSigned, op.Extended);

            Operand p1Res = context.BitwiseNot(p0Res);

            Operand pred = GetPredicate39(context);

            p0Res = GetPredLogicalOp(context, op.LogicalOp, p0Res, pred);
            p1Res = GetPredLogicalOp(context, op.LogicalOp, p1Res, pred);

            context.Copy(Register(op.Predicate3), p0Res);
            context.Copy(Register(op.Predicate0), p1Res);
        }

        public static void Lea(EmitterContext context)
        {
            OpCodeAlu op = (OpCodeAlu)context.CurrOp;

            bool negateA = op.RawOpCode.Extract(45);

            int shift = op.RawOpCode.Extract(39, 5);

            Operand srcA = GetSrcA(context);
            Operand srcB = GetSrcB(context);

            srcA = context.ShiftLeft(srcA, Const(shift));
            srcA = context.INegate(srcA, negateA);

            Operand res = context.IAdd(srcA, srcB);

            context.Copy(GetDest(context), res);

            // TODO: CC, X
        }

        public static void Lea_Hi(EmitterContext context)
        {
            OpCodeAlu op = (OpCodeAlu)context.CurrOp;

            bool isReg = op is OpCodeAluReg;
            bool negateA;
            int shift;

            if (isReg)
            {
                negateA = op.RawOpCode.Extract(37);
                shift   = op.RawOpCode.Extract(28, 5);
            }
            else
            {
                negateA = op.RawOpCode.Extract(56);
                shift   = op.RawOpCode.Extract(51, 5);
            }

            Operand srcA = GetSrcA(context);
            Operand srcB = GetSrcB(context);
            Operand srcC = GetSrcC(context);

            Operand aLow = context.ShiftLeft(srcA, Const(shift));
            Operand aHigh = shift == 0 ? Const(0) : context.ShiftRightU32(srcA, Const(32 - shift));
            aHigh = context.BitwiseOr(aHigh, context.ShiftLeft(srcC, Const(shift)));

            if (negateA)
            {
                // Perform 64-bit negation by doing bitwise not of the value,
                // then adding 1 and carrying over from low to high.
                aLow = context.BitwiseNot(aLow);
                aHigh = context.BitwiseNot(aHigh);

                aLow = AddWithCarry(context, aLow, Const(1), out Operand aLowCOut);
                aHigh = context.IAdd(aHigh, aLowCOut);
            }

            Operand res = context.IAdd(aHigh, srcB);

            context.Copy(GetDest(context), res);

            // TODO: CC, X
        }

        public static void Lop(EmitterContext context)
        {
            IOpCodeLop op = (IOpCodeLop)context.CurrOp;

            Operand srcA = context.BitwiseNot(GetSrcA(context), op.InvertA);
            Operand srcB = context.BitwiseNot(GetSrcB(context), op.InvertB);

            Operand res = srcB;

            switch (op.LogicalOp)
            {
                case LogicalOperation.And:         res = context.BitwiseAnd        (srcA, srcB); break;
                case LogicalOperation.Or:          res = context.BitwiseOr         (srcA, srcB); break;
                case LogicalOperation.ExclusiveOr: res = context.BitwiseExclusiveOr(srcA, srcB); break;
            }

            EmitLopPredWrite(context, op, res, (ConditionalOperation)context.CurrOp.RawOpCode.Extract(44, 2));

            Operand dest = GetDest(context);

            context.Copy(dest, res);

            SetZnFlags(context, dest, op.SetCondCode, op.Extended);
        }

        public static void Lop3(EmitterContext context)
        {
            IOpCodeLop op = (IOpCodeLop)context.CurrOp;

            Operand srcA = GetSrcA(context);
            Operand srcB = GetSrcB(context);
            Operand srcC = GetSrcC(context);

            bool regVariant = op is OpCodeLopReg;

            int truthTable = regVariant
                ? op.RawOpCode.Extract(28, 8)
                : op.RawOpCode.Extract(48, 8);

            Operand res = Lop3Expression.GetFromTruthTable(context, srcA, srcB, srcC, truthTable);

            if (regVariant)
            {
                EmitLopPredWrite(context, op, res, (ConditionalOperation)context.CurrOp.RawOpCode.Extract(36, 2));
            }

            Operand dest = GetDest(context);

            context.Copy(dest, res);

            SetZnFlags(context, dest, op.SetCondCode, op.Extended);
        }

        public static void Popc(EmitterContext context)
        {
            OpCodeAlu op = (OpCodeAlu)context.CurrOp;

            bool invert = op.RawOpCode.Extract(40);

            Operand srcB = context.BitwiseNot(GetSrcB(context), invert);

            Operand res = context.BitCount(srcB);

            context.Copy(GetDest(context), res);
        }

        public static void Pset(EmitterContext context)
        {
            OpCodePset op = (OpCodePset)context.CurrOp;

            bool boolFloat = op.RawOpCode.Extract(44);

            Operand srcA = context.BitwiseNot(Register(op.Predicate12), op.InvertA);
            Operand srcB = context.BitwiseNot(Register(op.Predicate29), op.InvertB);
            Operand srcC = context.BitwiseNot(Register(op.Predicate39), op.InvertP);

            Operand res = GetPredLogicalOp(context, op.LogicalOpAB, srcA, srcB);

            res = GetPredLogicalOp(context, op.LogicalOp, res, srcC);

            Operand dest = GetDest(context);

            if (boolFloat)
            {
                context.Copy(dest, context.ConditionalSelect(res, ConstF(1), Const(0)));
            }
            else
            {
                context.Copy(dest, res);
            }
        }

        public static void Psetp(EmitterContext context)
        {
            OpCodePset op = (OpCodePset)context.CurrOp;

            Operand srcA = context.BitwiseNot(Register(op.Predicate12), op.InvertA);
            Operand srcB = context.BitwiseNot(Register(op.Predicate29), op.InvertB);

            Operand p0Res = GetPredLogicalOp(context, op.LogicalOpAB, srcA, srcB);

            Operand p1Res = context.BitwiseNot(p0Res);

            Operand pred = GetPredicate39(context);

            p0Res = GetPredLogicalOp(context, op.LogicalOp, p0Res, pred);
            p1Res = GetPredLogicalOp(context, op.LogicalOp, p1Res, pred);

            context.Copy(Register(op.Predicate3), p0Res);
            context.Copy(Register(op.Predicate0), p1Res);
        }

        public static void Rro(EmitterContext context)
        {
            // This is the range reduction operator,
            // we translate it as a simple move, as it
            // should be always followed by a matching
            // MUFU instruction.
            OpCodeAlu op = (OpCodeAlu)context.CurrOp;

            bool negateB   = op.RawOpCode.Extract(45);
            bool absoluteB = op.RawOpCode.Extract(49);

            Operand srcB = GetSrcB(context);

            srcB = context.FPAbsNeg(srcB, absoluteB, negateB);

            context.Copy(GetDest(context), srcB);
        }

        public static void Shl(EmitterContext context)
        {
            OpCodeAlu op = (OpCodeAlu)context.CurrOp;

            bool isMasked = op.RawOpCode.Extract(39);

            Operand srcB = GetSrcB(context);

            if (isMasked)
            {
                srcB = context.BitwiseAnd(srcB, Const(0x1f));
            }

            Operand res = context.ShiftLeft(GetSrcA(context), srcB);

            if (!isMasked)
            {
                // Clamped shift value.
                Operand isLessThan32 = context.ICompareLessUnsigned(srcB, Const(32));

                res = context.ConditionalSelect(isLessThan32, res, Const(0));
            }

            // TODO: X, CC

            context.Copy(GetDest(context), res);
        }

        public static void Shr(EmitterContext context)
        {
            OpCodeAlu op = (OpCodeAlu)context.CurrOp;

            bool isMasked  = op.RawOpCode.Extract(39);
            bool isReverse = op.RawOpCode.Extract(40);
            bool isSigned  = op.RawOpCode.Extract(48);

            Operand srcA = GetSrcA(context);
            Operand srcB = GetSrcB(context);

            if (isReverse)
            {
                srcA = context.BitfieldReverse(srcA);
            }

            if (isMasked)
            {
                srcB = context.BitwiseAnd(srcB, Const(0x1f));
            }

            Operand res = isSigned
                ? context.ShiftRightS32(srcA, srcB)
                : context.ShiftRightU32(srcA, srcB);

            if (!isMasked)
            {
                // Clamped shift value.
                Operand resShiftBy32;

                if (isSigned)
                {
                    resShiftBy32 = context.ShiftRightS32(srcA, Const(31));
                }
                else
                {
                    resShiftBy32 = Const(0);
                }

                Operand isLessThan32 = context.ICompareLessUnsigned(srcB, Const(32));

                res = context.ConditionalSelect(isLessThan32, res, resShiftBy32);
            }

            // TODO: X, CC

            context.Copy(GetDest(context), res);
        }

        public static void Xmad(EmitterContext context)
        {
            OpCodeAlu op = (OpCodeAlu)context.CurrOp;

            bool signedA = context.CurrOp.RawOpCode.Extract(48);
            bool signedB = context.CurrOp.RawOpCode.Extract(49);
            bool highA   = context.CurrOp.RawOpCode.Extract(53);

            bool isReg = (op is OpCodeAluReg) && !(op is OpCodeAluRegCbuf);
            bool isImm = (op is OpCodeAluImm);

            XmadCMode mode = isReg || isImm
                ? (XmadCMode)context.CurrOp.RawOpCode.Extract(50, 3)
                : (XmadCMode)context.CurrOp.RawOpCode.Extract(50, 2);

            bool highB = false;

            if (isReg)
            {
                highB = context.CurrOp.RawOpCode.Extract(35);
            }
            else if (!isImm)
            {
                highB = context.CurrOp.RawOpCode.Extract(52);
            }

            Operand srcA = GetSrcA(context);
            Operand srcB = GetSrcB(context);
            Operand srcC = GetSrcC(context);

            // XMAD immediates are 16-bits unsigned integers.
            if (srcB.Type == OperandType.Constant)
            {
                srcB = Const(srcB.Value & 0xffff);
            }

            Operand Extend16To32(Operand src, bool high, bool signed)
            {
                if (signed && high)
                {
                    return context.ShiftRightS32(src, Const(16));
                }
                else if (signed)
                {
                    return context.BitfieldExtractS32(src, Const(0), Const(16));
                }
                else if (high)
                {
                    return context.ShiftRightU32(src, Const(16));
                }
                else
                {
                    return context.BitwiseAnd(src, Const(0xffff));
                }
            }

            srcA = Extend16To32(srcA, highA, signedA);
            srcB = Extend16To32(srcB, highB, signedB);

            bool productShiftLeft = false;
            bool merge            = false;

            if (op is OpCodeAluCbuf)
            {
                productShiftLeft = context.CurrOp.RawOpCode.Extract(55);
                merge            = context.CurrOp.RawOpCode.Extract(56);
            }
            else if (!(op is OpCodeAluRegCbuf))
            {
                productShiftLeft = context.CurrOp.RawOpCode.Extract(36);
                merge            = context.CurrOp.RawOpCode.Extract(37);
            }

            bool extended;

            if ((op is OpCodeAluReg) || (op is OpCodeAluImm))
            {
                extended = context.CurrOp.RawOpCode.Extract(38);
            }
            else
            {
                extended = context.CurrOp.RawOpCode.Extract(54);
            }

            Operand res = context.IMultiply(srcA, srcB);

            if (productShiftLeft)
            {
                res = context.ShiftLeft(res, Const(16));
            }

            switch (mode)
            {
                case XmadCMode.Cfull: break;

                case XmadCMode.Clo: srcC = Extend16To32(srcC, high: false, signed: false); break;
                case XmadCMode.Chi: srcC = Extend16To32(srcC, high: true,  signed: false); break;

                case XmadCMode.Cbcc:
                {
                    srcC = context.IAdd(srcC, context.ShiftLeft(GetSrcB(context), Const(16)));

                    break;
                }

                case XmadCMode.Csfu:
                {
                    Operand signAdjustA = context.ShiftLeft(context.ShiftRightU32(srcA, Const(31)), Const(16));
                    Operand signAdjustB = context.ShiftLeft(context.ShiftRightU32(srcB, Const(31)), Const(16));

                    srcC = context.ISubtract(srcC, context.IAdd(signAdjustA, signAdjustB));

                    break;
                }

                default: /* TODO: Warning */ break;
            }

            Operand product = res;

            if (extended)
            {
                // Add with carry.
                res = context.IAdd(res, context.BitwiseAnd(GetCF(), Const(1)));
            }
            else
            {
                // Add (no carry in).
                res = context.IAdd(res, srcC);
            }

            SetIaddFlags(context, res, product, srcC, op.SetCondCode, extended);

            if (merge)
            {
                res = context.BitwiseAnd(res, Const(0xffff));
                res = context.BitwiseOr(res, context.ShiftLeft(GetSrcB(context), Const(16)));
            }

            context.Copy(GetDest(context), res);
        }

        private static Operand GetIntComparison(
            EmitterContext   context,
            IntegerCondition cond,
            Operand          srcA,
            Operand          srcB,
            bool             isSigned,
            bool             extended)
        {
            return extended
                ? GetIntComparisonExtended(context, cond, srcA, srcB, isSigned)
                : GetIntComparison        (context, cond, srcA, srcB, isSigned);
        }

        private static Operand GetIntComparisonExtended(
            EmitterContext   context,
            IntegerCondition cond,
            Operand          srcA,
            Operand          srcB,
            bool             isSigned)
        {
            Operand res;

            if (cond == IntegerCondition.Always)
            {
                res = Const(IrConsts.True);
            }
            else if (cond == IntegerCondition.Never)
            {
                res = Const(IrConsts.False);
            }
            else
            {
                res = context.ISubtract(srcA, srcB);
                res = context.IAdd(res, context.BitwiseNot(GetCF()));

                switch (cond)
                {
                    case Decoders.IntegerCondition.Equal: // r = xh == yh && xl == yl
                        res = context.BitwiseAnd(context.ICompareEqual(srcA, srcB), GetZF());
                        break;
                    case Decoders.IntegerCondition.Less: // r = xh < yh || (xh == yh && xl < yl)
                        Operand notC = context.BitwiseNot(GetCF());
                        Operand prevLt = context.BitwiseAnd(context.ICompareEqual(srcA, srcB), notC);
                        res = isSigned
                            ? context.BitwiseOr(context.ICompareLess(srcA, srcB), prevLt)
                            : context.BitwiseOr(context.ICompareLessUnsigned(srcA, srcB), prevLt);
                        break;
                    case Decoders.IntegerCondition.LessOrEqual: // r = xh < yh || (xh == yh && xl <= yl)
                        Operand zOrNotC = context.BitwiseOr(GetZF(), context.BitwiseNot(GetCF()));
                        Operand prevLe = context.BitwiseAnd(context.ICompareEqual(srcA, srcB), zOrNotC);
                        res = isSigned
                            ? context.BitwiseOr(context.ICompareLess(srcA, srcB), prevLe)
                            : context.BitwiseOr(context.ICompareLessUnsigned(srcA, srcB), prevLe);
                        break;
                    case Decoders.IntegerCondition.Greater: // r = xh > yh || (xh == yh && xl > yl)
                        Operand notZAndC = context.BitwiseAnd(context.BitwiseNot(GetZF()), GetCF());
                        Operand prevGt = context.BitwiseAnd(context.ICompareEqual(srcA, srcB), notZAndC);
                        res = isSigned
                            ? context.BitwiseOr(context.ICompareGreater(srcA, srcB), prevGt)
                            : context.BitwiseOr(context.ICompareGreaterUnsigned(srcA, srcB), prevGt);
                        break;
                    case Decoders.IntegerCondition.GreaterOrEqual: // r = xh > yh || (xh == yh && xl >= yl)
                        Operand prevGe = context.BitwiseAnd(context.ICompareEqual(srcA, srcB), GetCF());
                        res = isSigned
                            ? context.BitwiseOr(context.ICompareGreater(srcA, srcB), prevGe)
                            : context.BitwiseOr(context.ICompareGreaterUnsigned(srcA, srcB), prevGe);
                        break;
                    case Decoders.IntegerCondition.NotEqual: // r = xh != yh || xl != yl
                        context.BitwiseOr(context.ICompareNotEqual(srcA, srcB), context.BitwiseNot(GetZF()));
                        break;
                    default:
                        throw new InvalidOperationException($"Unexpected condition \"{cond}\".");
                }
            }

            return res;
        }

        private static Operand GetIntComparison(
            EmitterContext   context,
            IntegerCondition cond,
            Operand          srcA,
            Operand          srcB,
            bool             isSigned)
        {
            Operand res;

            if (cond == IntegerCondition.Always)
            {
                res = Const(IrConsts.True);
            }
            else if (cond == IntegerCondition.Never)
            {
                res = Const(IrConsts.False);
            }
            else
            {
                var inst = cond switch
                {
                    IntegerCondition.Less => Instruction.CompareLessU32,
                    IntegerCondition.Equal => Instruction.CompareEqual,
                    IntegerCondition.LessOrEqual => Instruction.CompareLessOrEqualU32,
                    IntegerCondition.Greater => Instruction.CompareGreaterU32,
                    IntegerCondition.NotEqual => Instruction.CompareNotEqual,
                    IntegerCondition.GreaterOrEqual => Instruction.CompareGreaterOrEqualU32,
                    _ => throw new InvalidOperationException($"Unexpected condition \"{cond}\".")
                };

                if (isSigned)
                {
                    switch (cond)
                    {
                        case IntegerCondition.Less:           inst = Instruction.CompareLess;           break;
                        case IntegerCondition.LessOrEqual:    inst = Instruction.CompareLessOrEqual;    break;
                        case IntegerCondition.Greater:        inst = Instruction.CompareGreater;        break;
                        case IntegerCondition.GreaterOrEqual: inst = Instruction.CompareGreaterOrEqual; break;
                    }
                }

                res = context.Add(inst, Local(), srcA, srcB);
            }

            return res;
        }

        private static void EmitLopPredWrite(EmitterContext context, IOpCodeLop op, Operand result, ConditionalOperation condOp)
        {
            if (op is OpCodeLop opLop && !opLop.Predicate48.IsPT)
            {
                Operand pRes;

                if (condOp == ConditionalOperation.False)
                {
                    pRes = Const(IrConsts.False);
                }
                else if (condOp == ConditionalOperation.True)
                {
                    pRes = Const(IrConsts.True);
                }
                else if (condOp == ConditionalOperation.Zero)
                {
                    pRes = context.ICompareEqual(result, Const(0));
                }
                else /* if (opLop.CondOp == ConditionalOperation.NotZero) */
                {
                    pRes = context.ICompareNotEqual(result, Const(0));
                }

                context.Copy(Register(opLop.Predicate48), pRes);
            }
        }

        private static void SetIaddFlags(
            EmitterContext context,
            Operand        res,
            Operand        srcA,
            Operand        srcB,
            bool           setCC,
            bool           extended)
        {
            if (!setCC)
            {
                return;
            }

            if (extended)
            {
                // C = (d == a && CIn) || d < a
                Operand tempC0 = context.ICompareEqual       (res, srcA);
                Operand tempC1 = context.ICompareLessUnsigned(res, srcA);

                tempC0 = context.BitwiseAnd(tempC0, GetCF());

                context.Copy(GetCF(), context.BitwiseOr(tempC0, tempC1));
            }
            else
            {
                // C = d < a
                context.Copy(GetCF(), context.ICompareLessUnsigned(res, srcA));
            }

            // V = (d ^ a) & ~(a ^ b) < 0
            Operand tempV0 = context.BitwiseExclusiveOr(res,  srcA);
            Operand tempV1 = context.BitwiseExclusiveOr(srcA, srcB);

            tempV1 = context.BitwiseNot(tempV1);

            Operand tempV = context.BitwiseAnd(tempV0, tempV1);

            context.Copy(GetVF(), context.ICompareLess(tempV, Const(0)));

            SetZnFlags(context, res, setCC: true, extended: extended);
        }
    }
}