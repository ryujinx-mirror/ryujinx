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
            // TODO: Implement properly.
            context.Copy(GetDest(context), GetSrcC(context));
        }

        public static void Vmnmx(EmitterContext context)
        {
            OpCodeVideo op = (OpCodeVideo)context.CurrOp;

            bool max = op.RawOpCode.Extract(56);

            Operand srcA = Extend(context, GetSrcA(context), op.RaSelection, op.RaType);
            Operand srcC = GetSrcC(context);

            Operand srcB;

            if (op.HasRb)
            {
                srcB = Extend(context, Register(op.Rb), op.RbSelection, op.RbType);
            }
            else
            {
                srcB = Const(op.Immediate);
            }

            Operand res;

            bool resSigned;

            if ((op.RaType & VideoType.Signed) != (op.RbType & VideoType.Signed))
            {
                // Signedness is different, but for max, result will always fit a U32,
                // since one of the inputs can't be negative, and the result is the one
                // with highest value. For min, it will always fit on a S32, since
                // one of the input can't be greater than INT_MAX and we want the lowest value.
                resSigned = !max;

                res = max ? context.IMaximumU32(srcA, srcB) : context.IMinimumS32(srcA, srcB);

                if ((op.RaType & VideoType.Signed) != 0)
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
                resSigned = (op.RaType & VideoType.Signed) != 0;

                if (max)
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

            if (op.Saturate)
            {
                if (op.DstSigned && !resSigned)
                {
                    res = context.IMinimumU32(res, Const(int.MaxValue));
                }
                else if (!op.DstSigned && resSigned)
                {
                    res = context.IMaximumS32(res, Const(0));
                }
            }

            switch (op.PostOp)
            {
                case VideoPostOp.Acc:
                    res = context.IAdd(res, srcC);
                    break;
                case VideoPostOp.Max:
                    res = op.DstSigned ? context.IMaximumS32(res, srcC) : context.IMaximumU32(res, srcC);
                    break;
                case VideoPostOp.Min:
                    res = op.DstSigned ? context.IMinimumS32(res, srcC) : context.IMinimumU32(res, srcC);
                    break;
                case VideoPostOp.Mrg16h:
                    res = context.BitfieldInsert(srcC, res, Const(16), Const(16));
                    break;
                case VideoPostOp.Mrg16l:
                    res = context.BitfieldInsert(srcC, res, Const(0), Const(16));
                    break;
                case VideoPostOp.Mrg8b0:
                    res = context.BitfieldInsert(srcC, res, Const(0), Const(8));
                    break;
                case VideoPostOp.Mrg8b2:
                    res = context.BitfieldInsert(srcC, res, Const(16), Const(8));
                    break;
            }

            context.Copy(GetDest(context), res);
        }

        private static Operand Extend(EmitterContext context, Operand src, int sel, VideoType type)
        {
            return type switch
            {
                VideoType.U8  => ZeroExtendTo32(context, context.ShiftRightU32(src, Const(sel * 8)),  8),
                VideoType.U16 => ZeroExtendTo32(context, context.ShiftRightU32(src, Const(sel * 16)), 16),
                VideoType.S8  => SignExtendTo32(context, context.ShiftRightU32(src, Const(sel * 8)),  8),
                VideoType.S16 => SignExtendTo32(context, context.ShiftRightU32(src, Const(sel * 16)), 16),
                _ => src
            };
        }
    }
}