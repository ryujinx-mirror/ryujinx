using Ryujinx.Graphics.Shader.Decoders;
using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using Ryujinx.Graphics.Shader.Translation;
using System;
using static Ryujinx.Graphics.Shader.Instructions.InstEmitHelper;
using static Ryujinx.Graphics.Shader.IntermediateRepresentation.OperandHelper;

namespace Ryujinx.Graphics.Shader.Instructions
{
    static class InstEmitAluHelper
    {
        public static long GetIntMin(IDstFmt type)
        {
            return type switch
            {
                IDstFmt.U16 => ushort.MinValue,
                IDstFmt.S16 => short.MinValue,
                IDstFmt.U32 => uint.MinValue,
                IDstFmt.S32 => int.MinValue,
                _ => throw new ArgumentException($"The type \"{type}\" is not a supported integer type."),
            };
        }

        public static long GetIntMax(IDstFmt type)
        {
            return type switch
            {
                IDstFmt.U16 => ushort.MaxValue,
                IDstFmt.S16 => short.MaxValue,
                IDstFmt.U32 => uint.MaxValue,
                IDstFmt.S32 => int.MaxValue,
                _ => throw new ArgumentException($"The type \"{type}\" is not a supported integer type."),
            };
        }

        public static long GetIntMin(ISrcDstFmt type)
        {
            return type switch
            {
                ISrcDstFmt.U8 => byte.MinValue,
                ISrcDstFmt.S8 => sbyte.MinValue,
                ISrcDstFmt.U16 => ushort.MinValue,
                ISrcDstFmt.S16 => short.MinValue,
                ISrcDstFmt.U32 => uint.MinValue,
                ISrcDstFmt.S32 => int.MinValue,
                _ => throw new ArgumentException($"The type \"{type}\" is not a supported integer type."),
            };
        }

        public static long GetIntMax(ISrcDstFmt type)
        {
            return type switch
            {
                ISrcDstFmt.U8 => byte.MaxValue,
                ISrcDstFmt.S8 => sbyte.MaxValue,
                ISrcDstFmt.U16 => ushort.MaxValue,
                ISrcDstFmt.S16 => short.MaxValue,
                ISrcDstFmt.U32 => uint.MaxValue,
                ISrcDstFmt.S32 => int.MaxValue,
                _ => throw new ArgumentException($"The type \"{type}\" is not a supported integer type."),
            };
        }

        public static Operand GetPredLogicalOp(EmitterContext context, BoolOp logicOp, Operand input, Operand pred)
        {
            return logicOp switch
            {
                BoolOp.And => context.BitwiseAnd(input, pred),
                BoolOp.Or => context.BitwiseOr(input, pred),
                BoolOp.Xor => context.BitwiseExclusiveOr(input, pred),
                _ => input,
            };
        }

        public static Operand Extend(EmitterContext context, Operand src, VectorSelect type)
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
                _ => src,
            };
        }

        public static void SetZnFlags(EmitterContext context, Operand dest, bool setCC, bool extended = false)
        {
            if (!setCC)
            {
                return;
            }

            if (extended)
            {
                // When the operation is extended, it means we are doing
                // the operation on a long word with any number of bits,
                // so we need to AND the zero flag from result with the
                // previous result when extended is specified, to ensure
                // we have ZF set only if all words are zero, and not just
                // the last one.
                Operand oldZF = GetZF();

                Operand res = context.BitwiseAnd(context.ICompareEqual(dest, Const(0)), oldZF);

                context.Copy(GetZF(), res);
            }
            else
            {
                context.Copy(GetZF(), context.ICompareEqual(dest, Const(0)));
            }

            context.Copy(GetNF(), context.ICompareLess(dest, Const(0)));
        }

        public static void SetFPZnFlags(EmitterContext context, Operand dest, bool setCC, Instruction fpType = Instruction.FP32)
        {
            if (setCC)
            {
                Operand zero = ConstF(0);

                if (fpType == Instruction.FP64)
                {
                    zero = context.FP32ConvertToFP64(zero);
                }

                context.Copy(GetZF(), context.FPCompareEqual(dest, zero, fpType));
                context.Copy(GetNF(), context.FPCompareLess(dest, zero, fpType));
            }
        }

        public static (Operand, Operand) NegateLong(EmitterContext context, Operand low, Operand high)
        {
            low = context.BitwiseNot(low);
            high = context.BitwiseNot(high);
            low = AddWithCarry(context, low, Const(1), out Operand carryOut);
            high = context.IAdd(high, carryOut);
            return (low, high);
        }

        public static Operand AddWithCarry(EmitterContext context, Operand lhs, Operand rhs, out Operand carryOut)
        {
            Operand result = context.IAdd(lhs, rhs);

            // C = Rd < Rn
            carryOut = context.INegate(context.ICompareLessUnsigned(result, lhs));

            return result;
        }
    }
}
