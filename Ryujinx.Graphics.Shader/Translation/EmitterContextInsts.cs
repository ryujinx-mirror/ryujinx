using Ryujinx.Graphics.Shader.IntermediateRepresentation;

using static Ryujinx.Graphics.Shader.IntermediateRepresentation.OperandHelper;

namespace Ryujinx.Graphics.Shader.Translation
{
    static class EmitterContextInsts
    {
        public static Operand BitfieldExtractS32(this EmitterContext context, Operand a, Operand b, Operand c)
        {
            return context.Add(Instruction.BitfieldExtractS32, Local(), a, b, c);
        }

        public static Operand BitfieldExtractU32(this EmitterContext context, Operand a, Operand b, Operand c)
        {
            return context.Add(Instruction.BitfieldExtractU32, Local(), a, b, c);
        }

        public static Operand BitfieldInsert(this EmitterContext context, Operand a, Operand b, Operand c, Operand d)
        {
            return context.Add(Instruction.BitfieldInsert, Local(), a, b, c, d);
        }

        public static Operand BitfieldReverse(this EmitterContext context, Operand a)
        {
            return context.Add(Instruction.BitfieldReverse, Local(), a);
        }

        public static Operand BitwiseAnd(this EmitterContext context, Operand a, Operand b)
        {
            return context.Add(Instruction.BitwiseAnd, Local(), a, b);
        }

        public static Operand BitwiseExclusiveOr(this EmitterContext context, Operand a, Operand b)
        {
            return context.Add(Instruction.BitwiseExclusiveOr, Local(), a, b);
        }

        public static Operand BitwiseNot(this EmitterContext context, Operand a, bool invert)
        {
            if (invert)
            {
                a = context.BitwiseNot(a);
            }

            return a;
        }

        public static Operand BitwiseNot(this EmitterContext context, Operand a)
        {
            return context.Add(Instruction.BitwiseNot, Local(), a);
        }

        public static Operand BitwiseOr(this EmitterContext context, Operand a, Operand b)
        {
            return context.Add(Instruction.BitwiseOr, Local(), a, b);
        }

        public static Operand Branch(this EmitterContext context, Operand d)
        {
            return context.Add(Instruction.Branch, d);
        }

        public static Operand BranchIfFalse(this EmitterContext context, Operand d, Operand a)
        {
            return context.Add(Instruction.BranchIfFalse, d, a);
        }

        public static Operand BranchIfTrue(this EmitterContext context, Operand d, Operand a)
        {
            return context.Add(Instruction.BranchIfTrue, d, a);
        }

        public static Operand ConditionalSelect(this EmitterContext context, Operand a, Operand b, Operand c)
        {
            return context.Add(Instruction.ConditionalSelect, Local(), a, b, c);
        }

        public static Operand Copy(this EmitterContext context, Operand a)
        {
            return context.Add(Instruction.Copy, Local(), a);
        }

        public static void Copy(this EmitterContext context, Operand d, Operand a)
        {
            if (d.Type == OperandType.Constant)
            {
                return;
            }

            context.Add(Instruction.Copy, d, a);
        }

        public static Operand Discard(this EmitterContext context)
        {
            return context.Add(Instruction.Discard);
        }

        public static Operand EmitVertex(this EmitterContext context)
        {
            return context.Add(Instruction.EmitVertex);
        }

        public static Operand EndPrimitive(this EmitterContext context)
        {
            return context.Add(Instruction.EndPrimitive);
        }

        public static Operand FPAbsNeg(this EmitterContext context, Operand a, bool abs, bool neg)
        {
            return context.FPNegate(context.FPAbsolute(a, abs), neg);
        }

        public static Operand FPAbsolute(this EmitterContext context, Operand a, bool abs)
        {
            if (abs)
            {
                a = context.FPAbsolute(a);
            }

            return a;
        }

        public static Operand FPAbsolute(this EmitterContext context, Operand a)
        {
            return context.Add(Instruction.FP | Instruction.Absolute, Local(), a);
        }

        public static Operand FPAdd(this EmitterContext context, Operand a, Operand b)
        {
            return context.Add(Instruction.FP | Instruction.Add, Local(), a, b);
        }

        public static Operand FPCeiling(this EmitterContext context, Operand a)
        {
            return context.Add(Instruction.FP | Instruction.Ceiling, Local(), a);
        }

        public static Operand FPCompareEqual(this EmitterContext context, Operand a, Operand b)
        {
            return context.Add(Instruction.FP | Instruction.CompareEqual, Local(), a, b);
        }

        public static Operand FPCompareLess(this EmitterContext context, Operand a, Operand b)
        {
            return context.Add(Instruction.FP | Instruction.CompareLess, Local(), a, b);
        }

        public static Operand FPConvertToS32(this EmitterContext context, Operand a)
        {
            return context.Add(Instruction.ConvertFPToS32, Local(), a);
        }

        public static Operand FPCosine(this EmitterContext context, Operand a)
        {
            return context.Add(Instruction.FP | Instruction.Cosine, Local(), a);
        }

        public static Operand FPDivide(this EmitterContext context, Operand a, Operand b)
        {
            return context.Add(Instruction.FP | Instruction.Divide, Local(), a, b);
        }

        public static Operand FPExponentB2(this EmitterContext context, Operand a)
        {
            return context.Add(Instruction.FP | Instruction.ExponentB2, Local(), a);
        }

        public static Operand FPFloor(this EmitterContext context, Operand a)
        {
            return context.Add(Instruction.FP | Instruction.Floor, Local(), a);
        }

        public static Operand FPFusedMultiplyAdd(this EmitterContext context, Operand a, Operand b, Operand c)
        {
            return context.Add(Instruction.FusedMultiplyAdd, Local(), a, b, c);
        }

        public static Operand FPLogarithmB2(this EmitterContext context, Operand a)
        {
            return context.Add(Instruction.FP | Instruction.LogarithmB2, Local(), a);
        }

        public static Operand FPMaximum(this EmitterContext context, Operand a, Operand b)
        {
            return context.Add(Instruction.FP | Instruction.Maximum, Local(), a, b);
        }

        public static Operand FPMinimum(this EmitterContext context, Operand a, Operand b)
        {
            return context.Add(Instruction.FP | Instruction.Minimum, Local(), a, b);
        }

        public static Operand FPMultiply(this EmitterContext context, Operand a, Operand b)
        {
            return context.Add(Instruction.FP | Instruction.Multiply, Local(), a, b);
        }

        public static Operand FPNegate(this EmitterContext context, Operand a, bool neg)
        {
            if (neg)
            {
                a = context.FPNegate(a);
            }

            return a;
        }

        public static Operand FPNegate(this EmitterContext context, Operand a)
        {
            return context.Add(Instruction.FP | Instruction.Negate, Local(), a);
        }

        public static Operand FPReciprocal(this EmitterContext context, Operand a)
        {
            return context.FPDivide(ConstF(1), a);
        }

        public static Operand FPReciprocalSquareRoot(this EmitterContext context, Operand a)
        {
            return context.Add(Instruction.FP | Instruction.ReciprocalSquareRoot, Local(), a);
        }

        public static Operand FPRound(this EmitterContext context, Operand a)
        {
            return context.Add(Instruction.FP | Instruction.Round, Local(), a);
        }

        public static Operand FPSaturate(this EmitterContext context, Operand a, bool sat)
        {
            if (sat)
            {
                a = context.FPSaturate(a);
            }

            return a;
        }

        public static Operand FPSaturate(this EmitterContext context, Operand a)
        {
            return context.Add(Instruction.FP | Instruction.Clamp, Local(), a, ConstF(0), ConstF(1));
        }

        public static Operand FPSine(this EmitterContext context, Operand a)
        {
            return context.Add(Instruction.FP | Instruction.Sine, Local(), a);
        }

        public static Operand FPSquareRoot(this EmitterContext context, Operand a)
        {
            return context.Add(Instruction.FP | Instruction.SquareRoot, Local(), a);
        }

        public static Operand FPTruncate(this EmitterContext context, Operand a)
        {
            return context.Add(Instruction.Truncate, Local(), a);
        }

        public static Operand IAbsNeg(this EmitterContext context, Operand a, bool abs, bool neg)
        {
            return context.INegate(context.IAbsolute(a, abs), neg);
        }

        public static Operand IAbsolute(this EmitterContext context, Operand a, bool abs)
        {
            if (abs)
            {
                a = context.IAbsolute(a);
            }

            return a;
        }

        public static Operand IAbsolute(this EmitterContext context, Operand a)
        {
            return context.Add(Instruction.Absolute, Local(), a);
        }

        public static Operand IAdd(this EmitterContext context, Operand a, Operand b)
        {
            return context.Add(Instruction.Add, Local(), a, b);
        }

        public static Operand IClampS32(this EmitterContext context, Operand a, Operand b, Operand c)
        {
            return context.Add(Instruction.Clamp, Local(), a, b, c);
        }

        public static Operand IClampU32(this EmitterContext context, Operand a, Operand b, Operand c)
        {
            return context.Add(Instruction.ClampU32, Local(), a, b, c);
        }

        public static Operand ICompareEqual(this EmitterContext context, Operand a, Operand b)
        {
            return context.Add(Instruction.CompareEqual, Local(), a, b);
        }

        public static Operand ICompareLess(this EmitterContext context, Operand a, Operand b)
        {
            return context.Add(Instruction.CompareLess, Local(), a, b);
        }

        public static Operand ICompareLessUnsigned(this EmitterContext context, Operand a, Operand b)
        {
            return context.Add(Instruction.CompareLessU32, Local(), a, b);
        }

        public static Operand ICompareNotEqual(this EmitterContext context, Operand a, Operand b)
        {
            return context.Add(Instruction.CompareNotEqual, Local(), a, b);
        }

        public static Operand IConvertS32ToFP(this EmitterContext context, Operand a)
        {
            return context.Add(Instruction.ConvertS32ToFP, Local(), a);
        }

        public static Operand IConvertU32ToFP(this EmitterContext context, Operand a)
        {
            return context.Add(Instruction.ConvertU32ToFP, Local(), a);
        }

        public static Operand IMaximumS32(this EmitterContext context, Operand a, Operand b)
        {
            return context.Add(Instruction.Maximum, Local(), a, b);
        }

        public static Operand IMaximumU32(this EmitterContext context, Operand a, Operand b)
        {
            return context.Add(Instruction.MaximumU32, Local(), a, b);
        }

        public static Operand IMinimumS32(this EmitterContext context, Operand a, Operand b)
        {
            return context.Add(Instruction.Minimum, Local(), a, b);
        }

        public static Operand IMinimumU32(this EmitterContext context, Operand a, Operand b)
        {
            return context.Add(Instruction.MinimumU32, Local(), a, b);
        }

        public static Operand IMultiply(this EmitterContext context, Operand a, Operand b)
        {
            return context.Add(Instruction.Multiply, Local(), a, b);
        }

        public static Operand INegate(this EmitterContext context, Operand a, bool neg)
        {
            if (neg)
            {
                a = context.INegate(a);
            }

            return a;
        }

        public static Operand INegate(this EmitterContext context, Operand a)
        {
            return context.Add(Instruction.Negate, Local(), a);
        }

        public static Operand ISubtract(this EmitterContext context, Operand a, Operand b)
        {
            return context.Add(Instruction.Subtract, Local(), a, b);
        }

        public static Operand IsNan(this EmitterContext context, Operand a)
        {
            return context.Add(Instruction.IsNan, Local(), a);
        }

        public static Operand LoadAttribute(this EmitterContext context, Operand a, Operand b)
        {
            return context.Add(Instruction.LoadAttribute, Local(), a, b);
        }

        public static Operand LoadConstant(this EmitterContext context, Operand a, Operand b)
        {
            return context.Add(Instruction.LoadConstant, Local(), a, b);
        }

        public static Operand LoadGlobal(this EmitterContext context, Operand a)
        {
            return context.Add(Instruction.LoadGlobal, Local(), a);
        }

        public static Operand LoadLocal(this EmitterContext context, Operand a)
        {
            return context.Add(Instruction.LoadLocal, Local(), a);
        }

        public static Operand PackHalf2x16(this EmitterContext context, Operand a, Operand b)
        {
            return context.Add(Instruction.PackHalf2x16, Local(), a, b);
        }

        public static Operand Return(this EmitterContext context)
        {
            context.PrepareForReturn();

            return context.Add(Instruction.Return);
        }

        public static Operand ShiftLeft(this EmitterContext context, Operand a, Operand b)
        {
            return context.Add(Instruction.ShiftLeft, Local(), a, b);
        }

        public static Operand ShiftRightS32(this EmitterContext context, Operand a, Operand b)
        {
            return context.Add(Instruction.ShiftRightS32, Local(), a, b);
        }

        public static Operand ShiftRightU32(this EmitterContext context, Operand a, Operand b)
        {
            return context.Add(Instruction.ShiftRightU32, Local(), a, b);
        }

        public static Operand StoreGlobal(this EmitterContext context, Operand a, Operand b)
        {
            return context.Add(Instruction.StoreGlobal, null, a, b);
        }

        public static Operand StoreLocal(this EmitterContext context, Operand a, Operand b)
        {
            return context.Add(Instruction.StoreLocal, null, a, b);
        }

        public static Operand UnpackHalf2x16High(this EmitterContext context, Operand a)
        {
            return UnpackHalf2x16(context, a, 1);
        }

        public static Operand UnpackHalf2x16Low(this EmitterContext context, Operand a)
        {
            return UnpackHalf2x16(context, a, 0);
        }

        private static Operand UnpackHalf2x16(this EmitterContext context, Operand a, int index)
        {
            Operand dest = Local();

            context.Add(new Operation(Instruction.UnpackHalf2x16, index, dest, a));

            return dest;
        }
    }
}