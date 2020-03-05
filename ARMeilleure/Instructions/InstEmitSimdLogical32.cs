using ARMeilleure.Decoders;
using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.Translation;

using static ARMeilleure.Instructions.InstEmitSimdHelper32;

namespace ARMeilleure.Instructions
{
    static partial class InstEmit32
    {
        public static void Vand_I(ArmEmitterContext context)
        {
            if (Optimizations.UseSse2)
            {
                EmitVectorBinaryOpF32(context, Intrinsic.X86Pand, Intrinsic.X86Pand);
            }
            else
            {
                EmitVectorBinaryOpZx32(context, (op1, op2) => context.BitwiseAnd(op1, op2));
            }
        }

        public static void Vbif(ArmEmitterContext context)
        {
            EmitBifBit(context, true);
        }

        public static void Vbit(ArmEmitterContext context)
        {
            EmitBifBit(context, false);
        }

        public static void Vbsl(ArmEmitterContext context)
        {
            if (Optimizations.UseSse2)
            {
                EmitVectorTernaryOpSimd32(context, (d, n, m) =>
                {
                    Operand res = context.AddIntrinsic(Intrinsic.X86Pxor, n, m);
                    res = context.AddIntrinsic(Intrinsic.X86Pand, res, d);
                    return context.AddIntrinsic(Intrinsic.X86Pxor, res, m);
                });
            }
            else
            {
                EmitVectorTernaryOpZx32(context, (op1, op2, op3) =>
                {
                    return context.BitwiseExclusiveOr(
                        context.BitwiseAnd(op1,
                        context.BitwiseExclusiveOr(op2, op3)), op3);
                });
            }
        }

        public static void Vorr_I(ArmEmitterContext context)
        {
            if (Optimizations.UseSse2)
            {
                EmitVectorBinaryOpF32(context, Intrinsic.X86Por, Intrinsic.X86Por);
            }
            else
            {
                EmitVectorBinaryOpZx32(context, (op1, op2) => context.BitwiseOr(op1, op2));
            }
        }

        private static void EmitBifBit(ArmEmitterContext context, bool notRm)
        {
            OpCode32SimdReg op = (OpCode32SimdReg)context.CurrOp;

            if (Optimizations.UseSse2)
            {
                EmitVectorTernaryOpSimd32(context, (d, n, m) =>
                {
                    Operand res = context.AddIntrinsic(Intrinsic.X86Pxor, n, d);
                    res = context.AddIntrinsic((notRm) ? Intrinsic.X86Pandn : Intrinsic.X86Pand, m, res);
                    return context.AddIntrinsic(Intrinsic.X86Pxor, d, res);
                });
            }
            else
            {
                EmitVectorTernaryOpZx32(context, (d, n, m) =>
                {
                    if (notRm)
                    {
                        m = context.BitwiseNot(m);
                    }
                    return context.BitwiseExclusiveOr(
                        context.BitwiseAnd(m,
                        context.BitwiseExclusiveOr(d, n)), d);
                });
            }
        }
    }
}
