using ARMeilleure.Decoders;
using ARMeilleure.Translation;

using static ARMeilleure.Instructions.InstEmitSimdHelper32;

namespace ARMeilleure.Instructions
{
    static partial class InstEmit32
    {
        public static void Vand_I(ArmEmitterContext context)
        {
            EmitVectorBinaryOpZx32(context, (op1, op2) => context.BitwiseAnd(op1, op2));
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
            EmitVectorTernaryOpZx32(context, (op1, op2, op3) =>
            {
                return context.BitwiseExclusiveOr(
                    context.BitwiseAnd(op1,
                    context.BitwiseExclusiveOr(op2, op3)), op3);
            });
        }

        public static void Vorr_I(ArmEmitterContext context)
        {
            EmitVectorBinaryOpZx32(context, (op1, op2) => context.BitwiseOr(op1, op2));
        }

        private static void EmitBifBit(ArmEmitterContext context, bool notRm)
        {
            OpCode32SimdReg op = (OpCode32SimdReg)context.CurrOp;

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
