using Ryujinx.Graphics.Shader.Decoders;
using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using Ryujinx.Graphics.Shader.Translation;

using static Ryujinx.Graphics.Shader.Instructions.InstEmitHelper;
using static Ryujinx.Graphics.Shader.IntermediateRepresentation.OperandHelper;

namespace Ryujinx.Graphics.Shader.Instructions
{
    static partial class InstEmit
    {
        public static void Vmnmx(EmitterContext context)
        {
            InstVmnmx op = context.GetOp<InstVmnmx>();

            Operand srcA = Extend(context, GetSrcReg(context, op.SrcA), op.ASelect);
            Operand srcC = GetSrcReg(context, op.SrcC);

            Operand srcB;

            if (op.BVideo)
            {
                srcB = Extend(context, GetSrcReg(context, op.SrcB), op.BSelect);
            }
            else
            {
                int imm = op.Imm16;

                if ((op.BSelect & VectorSelect.S8B0) != 0)
                {
                    imm = (imm << 16) >> 16;
                }

                srcB = Const(imm);
            }

            Operand res;

            bool resSigned;

            if ((op.ASelect & VectorSelect.S8B0) != (op.BSelect & VectorSelect.S8B0))
            {
                // Signedness is different, but for max, result will always fit a U32,
                // since one of the inputs can't be negative, and the result is the one
                // with highest value. For min, it will always fit on a S32, since
                // one of the input can't be greater than INT_MAX and we want the lowest value.
                resSigned = !op.Mn;

                res = op.Mn ? context.IMaximumU32(srcA, srcB) : context.IMinimumS32(srcA, srcB);

                if ((op.ASelect & VectorSelect.S8B0) != 0)
                {
                    Operand isBGtIntMax = context.ICompareLess(srcB, Const(0));

                    res = context.ConditionalSelect(isBGtIntMax, srcB, res);
                }
                else
                {
                    Operand isAGtIntMax = context.ICompareLess(srcA, Const(0));

                    res = context.ConditionalSelect(isAGtIntMax, srcA, res);
                }
            }
            else
            {
                // Ra and Rb have the same signedness, so doesn't matter which one we test.
                resSigned = (op.ASelect & VectorSelect.S8B0) != 0;

                if (op.Mn)
                {
                    res = resSigned
                        ? context.IMaximumS32(srcA, srcB)
                        : context.IMaximumU32(srcA, srcB);
                }
                else
                {
                    res = resSigned
                        ? context.IMinimumS32(srcA, srcB)
                        : context.IMinimumU32(srcA, srcB);
                }
            }

            if (op.Sat)
            {
                if (op.DFormat && !resSigned)
                {
                    res = context.IMinimumU32(res, Const(int.MaxValue));
                }
                else if (!op.DFormat && resSigned)
                {
                    res = context.IMaximumS32(res, Const(0));
                }
            }

            switch (op.VideoOp)
            {
                case VideoOp.Acc:
                    res = context.IAdd(res, srcC);
                    break;
                case VideoOp.Max:
                    res = op.DFormat ? context.IMaximumS32(res, srcC) : context.IMaximumU32(res, srcC);
                    break;
                case VideoOp.Min:
                    res = op.DFormat ? context.IMinimumS32(res, srcC) : context.IMinimumU32(res, srcC);
                    break;
                case VideoOp.Mrg16h:
                    res = context.BitfieldInsert(srcC, res, Const(16), Const(16));
                    break;
                case VideoOp.Mrg16l:
                    res = context.BitfieldInsert(srcC, res, Const(0), Const(16));
                    break;
                case VideoOp.Mrg8b0:
                    res = context.BitfieldInsert(srcC, res, Const(0), Const(8));
                    break;
                case VideoOp.Mrg8b2:
                    res = context.BitfieldInsert(srcC, res, Const(16), Const(8));
                    break;
            }

            context.Copy(GetDest(op.Dest), res);
        }

        private static Operand Extend(EmitterContext context, Operand src, VectorSelect type)
        {
            return type switch
            {
                VectorSelect.U8B0 => ZeroExtendTo32(context, context.ShiftRightU32(src, Const(0)), 8),
                VectorSelect.U8B1 => ZeroExtendTo32(context, context.ShiftRightU32(src, Const(8)), 8),
                VectorSelect.U8B2 => ZeroExtendTo32(context, context.ShiftRightU32(src, Const(16)), 8),
                VectorSelect.U8B3 => ZeroExtendTo32(context, context.ShiftRightU32(src, Const(24)), 8),
                VectorSelect.U16H0 => ZeroExtendTo32(context, context.ShiftRightU32(src, Const(0)), 16),
                VectorSelect.U16H1 => ZeroExtendTo32(context, context.ShiftRightU32(src, Const(16)), 16),
                VectorSelect.S8B0 => SignExtendTo32(context, context.ShiftRightU32(src, Const(0)), 8),
                VectorSelect.S8B1 => SignExtendTo32(context, context.ShiftRightU32(src, Const(8)), 8),
                VectorSelect.S8B2 => SignExtendTo32(context, context.ShiftRightU32(src, Const(16)), 8),
                VectorSelect.S8B3 => SignExtendTo32(context, context.ShiftRightU32(src, Const(24)), 8),
                VectorSelect.S16H0 => SignExtendTo32(context, context.ShiftRightU32(src, Const(0)), 16),
                VectorSelect.S16H1 => SignExtendTo32(context, context.ShiftRightU32(src, Const(16)), 16),
                _ => src
            };
        }
    }
}