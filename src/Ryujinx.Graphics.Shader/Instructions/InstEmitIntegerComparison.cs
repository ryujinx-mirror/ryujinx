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
        public static void IcmpR(EmitterContext context)
        {
            InstIcmpR op = context.GetOp<InstIcmpR>();

            var srcA = GetSrcReg(context, op.SrcA);
            var srcB = GetSrcReg(context, op.SrcB);
            var srcC = GetSrcReg(context, op.SrcC);

            EmitIcmp(context, op.IComp, srcA, srcB, srcC, op.Dest, op.Signed);
        }

        public static void IcmpI(EmitterContext context)
        {
            InstIcmpI op = context.GetOp<InstIcmpI>();

            var srcA = GetSrcReg(context, op.SrcA);
            var srcB = GetSrcImm(context, Imm20ToSInt(op.Imm20));
            var srcC = GetSrcReg(context, op.SrcC);

            EmitIcmp(context, op.IComp, srcA, srcB, srcC, op.Dest, op.Signed);
        }

        public static void IcmpC(EmitterContext context)
        {
            InstIcmpC op = context.GetOp<InstIcmpC>();

            var srcA = GetSrcReg(context, op.SrcA);
            var srcB = GetSrcCbuf(context, op.CbufSlot, op.CbufOffset);
            var srcC = GetSrcReg(context, op.SrcC);

            EmitIcmp(context, op.IComp, srcA, srcB, srcC, op.Dest, op.Signed);
        }

        public static void IcmpRc(EmitterContext context)
        {
            InstIcmpRc op = context.GetOp<InstIcmpRc>();

            var srcA = GetSrcReg(context, op.SrcA);
            var srcB = GetSrcReg(context, op.SrcC);
            var srcC = GetSrcCbuf(context, op.CbufSlot, op.CbufOffset);

            EmitIcmp(context, op.IComp, srcA, srcB, srcC, op.Dest, op.Signed);
        }

        public static void IsetR(EmitterContext context)
        {
            InstIsetR op = context.GetOp<InstIsetR>();

            var srcA = GetSrcReg(context, op.SrcA);
            var srcB = GetSrcReg(context, op.SrcB);

            EmitIset(context, op.IComp, op.Bop, srcA, srcB, op.SrcPred, op.SrcPredInv, op.Dest, op.BVal, op.Signed, op.X, op.WriteCC);
        }

        public static void IsetI(EmitterContext context)
        {
            InstIsetI op = context.GetOp<InstIsetI>();

            var srcA = GetSrcReg(context, op.SrcA);
            var srcB = GetSrcImm(context, Imm20ToSInt(op.Imm20));

            EmitIset(context, op.IComp, op.Bop, srcA, srcB, op.SrcPred, op.SrcPredInv, op.Dest, op.BVal, op.Signed, op.X, op.WriteCC);
        }

        public static void IsetC(EmitterContext context)
        {
            InstIsetC op = context.GetOp<InstIsetC>();

            var srcA = GetSrcReg(context, op.SrcA);
            var srcB = GetSrcCbuf(context, op.CbufSlot, op.CbufOffset);

            EmitIset(context, op.IComp, op.Bop, srcA, srcB, op.SrcPred, op.SrcPredInv, op.Dest, op.BVal, op.Signed, op.X, op.WriteCC);
        }

        public static void IsetpR(EmitterContext context)
        {
            InstIsetpR op = context.GetOp<InstIsetpR>();

            var srcA = GetSrcReg(context, op.SrcA);
            var srcB = GetSrcReg(context, op.SrcB);

            EmitIsetp(context, op.IComp, op.Bop, srcA, srcB, op.SrcPred, op.SrcPredInv, op.DestPred, op.DestPredInv, op.Signed, op.X);
        }

        public static void IsetpI(EmitterContext context)
        {
            InstIsetpI op = context.GetOp<InstIsetpI>();

            var srcA = GetSrcReg(context, op.SrcA);
            var srcB = GetSrcImm(context, Imm20ToSInt(op.Imm20));

            EmitIsetp(context, op.IComp, op.Bop, srcA, srcB, op.SrcPred, op.SrcPredInv, op.DestPred, op.DestPredInv, op.Signed, op.X);
        }

        public static void IsetpC(EmitterContext context)
        {
            InstIsetpC op = context.GetOp<InstIsetpC>();

            var srcA = GetSrcReg(context, op.SrcA);
            var srcB = GetSrcCbuf(context, op.CbufSlot, op.CbufOffset);

            EmitIsetp(context, op.IComp, op.Bop, srcA, srcB, op.SrcPred, op.SrcPredInv, op.DestPred, op.DestPredInv, op.Signed, op.X);
        }

        private static void EmitIcmp(
            EmitterContext context,
            IComp cmpOp,
            Operand srcA,
            Operand srcB,
            Operand srcC,
            int rd,
            bool isSigned)
        {
            Operand cmpRes = GetIntComparison(context, cmpOp, srcC, Const(0), isSigned);

            Operand res = context.ConditionalSelect(cmpRes, srcA, srcB);

            context.Copy(GetDest(rd), res);
        }

        private static void EmitIset(
            EmitterContext context,
            IComp cmpOp,
            BoolOp logicOp,
            Operand srcA,
            Operand srcB,
            int srcPred,
            bool srcPredInv,
            int rd,
            bool boolFloat,
            bool isSigned,
            bool extended,
            bool writeCC)
        {
            Operand res = GetIntComparison(context, cmpOp, srcA, srcB, isSigned, extended);
            Operand pred = GetPredicate(context, srcPred, srcPredInv);

            res = GetPredLogicalOp(context, logicOp, res, pred);

            Operand dest = GetDest(rd);

            if (boolFloat)
            {
                res = context.ConditionalSelect(res, ConstF(1), Const(0));

                context.Copy(dest, res);

                SetFPZnFlags(context, res, writeCC);
            }
            else
            {
                context.Copy(dest, res);

                SetZnFlags(context, res, writeCC, extended);
            }
        }

        private static void EmitIsetp(
            EmitterContext context,
            IComp cmpOp,
            BoolOp logicOp,
            Operand srcA,
            Operand srcB,
            int srcPred,
            bool srcPredInv,
            int destPred,
            int destPredInv,
            bool isSigned,
            bool extended)
        {
            Operand p0Res = GetIntComparison(context, cmpOp, srcA, srcB, isSigned, extended);
            Operand p1Res = context.BitwiseNot(p0Res);
            Operand pred = GetPredicate(context, srcPred, srcPredInv);

            p0Res = GetPredLogicalOp(context, logicOp, p0Res, pred);
            p1Res = GetPredLogicalOp(context, logicOp, p1Res, pred);

            context.Copy(Register(destPred, RegisterType.Predicate), p0Res);
            context.Copy(Register(destPredInv, RegisterType.Predicate), p1Res);
        }

        private static Operand GetIntComparison(
            EmitterContext context,
            IComp cond,
            Operand srcA,
            Operand srcB,
            bool isSigned,
            bool extended)
        {
            return extended
                ? GetIntComparisonExtended(context, cond, srcA, srcB, isSigned)
                : GetIntComparison(context, cond, srcA, srcB, isSigned);
        }

        private static Operand GetIntComparisonExtended(EmitterContext context, IComp cond, Operand srcA, Operand srcB, bool isSigned)
        {
            Operand res;

            if (cond == IComp.T)
            {
                res = Const(IrConsts.True);
            }
            else if (cond == IComp.F)
            {
                res = Const(IrConsts.False);
            }
            else
            {
                res = context.ISubtract(srcA, srcB);
#pragma warning disable IDE0059 // Remove unnecessary value assignment
                res = context.IAdd(res, context.BitwiseNot(GetCF()));
#pragma warning restore IDE0059

                switch (cond)
                {
                    case IComp.Eq: // r = xh == yh && xl == yl
                        res = context.BitwiseAnd(context.ICompareEqual(srcA, srcB), GetZF());
                        break;
                    case IComp.Lt: // r = xh < yh || (xh == yh && xl < yl)
                        Operand notC = context.BitwiseNot(GetCF());
                        Operand prevLt = context.BitwiseAnd(context.ICompareEqual(srcA, srcB), notC);
                        res = isSigned
                            ? context.BitwiseOr(context.ICompareLess(srcA, srcB), prevLt)
                            : context.BitwiseOr(context.ICompareLessUnsigned(srcA, srcB), prevLt);
                        break;
                    case IComp.Le: // r = xh < yh || (xh == yh && xl <= yl)
                        Operand zOrNotC = context.BitwiseOr(GetZF(), context.BitwiseNot(GetCF()));
                        Operand prevLe = context.BitwiseAnd(context.ICompareEqual(srcA, srcB), zOrNotC);
                        res = isSigned
                            ? context.BitwiseOr(context.ICompareLess(srcA, srcB), prevLe)
                            : context.BitwiseOr(context.ICompareLessUnsigned(srcA, srcB), prevLe);
                        break;
                    case IComp.Gt: // r = xh > yh || (xh == yh && xl > yl)
                        Operand notZAndC = context.BitwiseAnd(context.BitwiseNot(GetZF()), GetCF());
                        Operand prevGt = context.BitwiseAnd(context.ICompareEqual(srcA, srcB), notZAndC);
                        res = isSigned
                            ? context.BitwiseOr(context.ICompareGreater(srcA, srcB), prevGt)
                            : context.BitwiseOr(context.ICompareGreaterUnsigned(srcA, srcB), prevGt);
                        break;
                    case IComp.Ge: // r = xh > yh || (xh == yh && xl >= yl)
                        Operand prevGe = context.BitwiseAnd(context.ICompareEqual(srcA, srcB), GetCF());
                        res = isSigned
                            ? context.BitwiseOr(context.ICompareGreater(srcA, srcB), prevGe)
                            : context.BitwiseOr(context.ICompareGreaterUnsigned(srcA, srcB), prevGe);
                        break;
                    case IComp.Ne: // r = xh != yh || xl != yl
                        res = context.BitwiseOr(context.ICompareNotEqual(srcA, srcB), context.BitwiseNot(GetZF()));
                        break;
                    default:
                        throw new ArgumentException($"Unexpected condition \"{cond}\".");
                }
            }

            return res;
        }

        private static Operand GetIntComparison(EmitterContext context, IComp cond, Operand srcA, Operand srcB, bool isSigned)
        {
            Operand res;

            if (cond == IComp.T)
            {
                res = Const(IrConsts.True);
            }
            else if (cond == IComp.F)
            {
                res = Const(IrConsts.False);
            }
            else
            {
                var inst = cond switch
                {
                    IComp.Lt => Instruction.CompareLessU32,
                    IComp.Eq => Instruction.CompareEqual,
                    IComp.Le => Instruction.CompareLessOrEqualU32,
                    IComp.Gt => Instruction.CompareGreaterU32,
                    IComp.Ne => Instruction.CompareNotEqual,
                    IComp.Ge => Instruction.CompareGreaterOrEqualU32,
                    _ => throw new InvalidOperationException($"Unexpected condition \"{cond}\"."),
                };

                if (isSigned)
                {
                    switch (cond)
                    {
                        case IComp.Lt:
                            inst = Instruction.CompareLess;
                            break;
                        case IComp.Le:
                            inst = Instruction.CompareLessOrEqual;
                            break;
                        case IComp.Gt:
                            inst = Instruction.CompareGreater;
                            break;
                        case IComp.Ge:
                            inst = Instruction.CompareGreaterOrEqual;
                            break;
                    }
                }

                res = context.Add(inst, Local(), srcA, srcB);
            }

            return res;
        }
    }
}
