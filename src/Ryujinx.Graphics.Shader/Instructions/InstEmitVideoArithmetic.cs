using Ryujinx.Graphics.Shader.Decoders;
using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using Ryujinx.Graphics.Shader.Translation;

using static Ryujinx.Graphics.Shader.Instructions.InstEmitHelper;
using static Ryujinx.Graphics.Shader.IntermediateRepresentation.OperandHelper;

namespace Ryujinx.Graphics.Shader.Instructions
{
    static partial class InstEmit
    {
        public static void Vmad(EmitterContext context)
        {
            InstVmad op = context.GetOp<InstVmad>();

            bool aSigned = (op.ASelect & VectorSelect.S8B0) != 0;
            bool bSigned = (op.BSelect & VectorSelect.S8B0) != 0;

            Operand srcA = InstEmitAluHelper.Extend(context, GetSrcReg(context, op.SrcA), op.ASelect);
            Operand srcC = context.INegate(GetSrcReg(context, op.SrcC), op.AvgMode == AvgMode.NegB);
            Operand srcB;

            if (op.BVideo)
            {
                srcB = InstEmitAluHelper.Extend(context, GetSrcReg(context, op.SrcB), op.BSelect);
            }
            else
            {
                int imm = op.Imm16;

                if (bSigned)
                {
                    imm = (imm << 16) >> 16;
                }

                srcB = Const(imm);
            }

            Operand productLow = context.IMultiply(srcA, srcB);
            Operand productHigh;

            if (aSigned == bSigned)
            {
                productHigh = aSigned
                    ? context.MultiplyHighS32(srcA, srcB)
                    : context.MultiplyHighU32(srcA, srcB);
            }
            else
            {
                Operand temp = aSigned
                    ? context.IMultiply(srcB, context.ShiftRightS32(srcA, Const(31)))
                    : context.IMultiply(srcA, context.ShiftRightS32(srcB, Const(31)));

                productHigh = context.IAdd(temp, context.MultiplyHighU32(srcA, srcB));
            }

            if (op.AvgMode == AvgMode.NegA)
            {
                (productLow, productHigh) = InstEmitAluHelper.NegateLong(context, productLow, productHigh);
            }

            Operand resLow = InstEmitAluHelper.AddWithCarry(context, productLow, srcC, out Operand sumCarry);
            Operand resHigh = context.IAdd(productHigh, sumCarry);

            if (op.AvgMode == AvgMode.PlusOne)
            {
                resLow = InstEmitAluHelper.AddWithCarry(context, resLow, Const(1), out Operand poCarry);
                resHigh = context.IAdd(resHigh, poCarry);
            }

            bool resSigned = op.ASelect == VectorSelect.S32 ||
                             op.BSelect == VectorSelect.S32 ||
                             op.AvgMode == AvgMode.NegB ||
                             op.AvgMode == AvgMode.NegA;

            int shift = op.VideoScale switch
            {
                VideoScale.Shr7 => 7,
                VideoScale.Shr15 => 15,
                _ => 0
            };

            if (shift != 0)
            {
                // Low = (Low >> Shift) | (High << (32 - Shift))
                // High >>= Shift
                resLow = context.ShiftRightU32(resLow, Const(shift));
                resLow = context.BitwiseOr(resLow, context.ShiftLeft(resHigh, Const(32 - shift)));
                resHigh = resSigned
                    ? context.ShiftRightS32(resHigh, Const(shift))
                    : context.ShiftRightU32(resHigh, Const(shift));
            }

            Operand res = resLow;

            if (op.Sat)
            {
                Operand sign = context.ShiftRightS32(resHigh, Const(31));

                if (resSigned)
                {
                    Operand overflow = context.ICompareNotEqual(resHigh, context.ShiftRightS32(resLow, Const(31)));
                    Operand clampValue = context.ConditionalSelect(sign, Const(int.MinValue), Const(int.MaxValue));
                    res = context.ConditionalSelect(overflow, clampValue, resLow);
                }
                else
                {
                    Operand overflow = context.ICompareNotEqual(resHigh, Const(0));
                    res = context.ConditionalSelect(overflow, context.BitwiseNot(sign), resLow);
                }
            }

            context.Copy(GetDest(op.Dest), res);

            // TODO: CC.
        }
    }
}