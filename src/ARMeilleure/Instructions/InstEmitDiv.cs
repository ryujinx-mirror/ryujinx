using ARMeilleure.Decoders;
using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.Translation;
using static ARMeilleure.Instructions.InstEmitHelper;
using static ARMeilleure.IntermediateRepresentation.Operand.Factory;

namespace ARMeilleure.Instructions
{
    static partial class InstEmit
    {
        public static void Sdiv(ArmEmitterContext context) => EmitDiv(context, unsigned: false);
        public static void Udiv(ArmEmitterContext context) => EmitDiv(context, unsigned: true);

        private static void EmitDiv(ArmEmitterContext context, bool unsigned)
        {
            OpCodeAluBinary op = (OpCodeAluBinary)context.CurrOp;

            // If Rm == 0, Rd = 0 (division by zero).
            Operand n = GetIntOrZR(context, op.Rn);
            Operand m = GetIntOrZR(context, op.Rm);

            Operand divisorIsZero = context.ICompareEqual(m, Const(m.Type, 0));

            Operand lblBadDiv = Label();
            Operand lblEnd = Label();

            context.BranchIfTrue(lblBadDiv, divisorIsZero);

            if (!unsigned)
            {
                // If Rn == INT_MIN && Rm == -1, Rd = INT_MIN (overflow).
                bool is32Bits = op.RegisterSize == RegisterSize.Int32;

                Operand intMin = is32Bits ? Const(int.MinValue) : Const(long.MinValue);
                Operand minus1 = is32Bits ? Const(-1) : Const(-1L);

                Operand nIsIntMin = context.ICompareEqual(n, intMin);
                Operand mIsMinus1 = context.ICompareEqual(m, minus1);

                Operand lblGoodDiv = Label();

                context.BranchIfFalse(lblGoodDiv, context.BitwiseAnd(nIsIntMin, mIsMinus1));

                SetAluDOrZR(context, intMin);

                context.Branch(lblEnd);

                context.MarkLabel(lblGoodDiv);
            }

            Operand d = unsigned
                ? context.DivideUI(n, m)
                : context.Divide(n, m);

            SetAluDOrZR(context, d);

            context.Branch(lblEnd);

            context.MarkLabel(lblBadDiv);

            SetAluDOrZR(context, Const(op.GetOperandType(), 0));

            context.MarkLabel(lblEnd);
        }
    }
}
