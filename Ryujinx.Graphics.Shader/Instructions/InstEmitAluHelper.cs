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
        public static long GetIntMin(IntegerType type)
        {
            switch (type)
            {
                case IntegerType.U8:  return byte.MinValue;
                case IntegerType.S8:  return sbyte.MinValue;
                case IntegerType.U16: return ushort.MinValue;
                case IntegerType.S16: return short.MinValue;
                case IntegerType.U32: return uint.MinValue;
                case IntegerType.S32: return int.MinValue;
            }

            throw new ArgumentException($"The type \"{type}\" is not a supported int type.");
        }

        public static long GetIntMax(IntegerType type)
        {
            switch (type)
            {
                case IntegerType.U8:  return byte.MaxValue;
                case IntegerType.S8:  return sbyte.MaxValue;
                case IntegerType.U16: return ushort.MaxValue;
                case IntegerType.S16: return short.MaxValue;
                case IntegerType.U32: return uint.MaxValue;
                case IntegerType.S32: return int.MaxValue;
            }

            throw new ArgumentException($"The type \"{type}\" is not a supported int type.");
        }

        public static Operand GetPredLogicalOp(
            EmitterContext   context,
            LogicalOperation logicalOp,
            Operand          input,
            Operand          pred)
        {
            switch (logicalOp)
            {
                case LogicalOperation.And:         return context.BitwiseAnd        (input, pred);
                case LogicalOperation.Or:          return context.BitwiseOr         (input, pred);
                case LogicalOperation.ExclusiveOr: return context.BitwiseExclusiveOr(input, pred);
            }

            return input;
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
                context.Copy(GetNF(), context.FPCompareLess (dest, zero, fpType));
            }
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