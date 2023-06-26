using ARMeilleure.Decoders;
using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.Translation;
using static ARMeilleure.Instructions.InstEmitFlowHelper;
using static ARMeilleure.Instructions.InstEmitHelper;
using static ARMeilleure.IntermediateRepresentation.Operand.Factory;

namespace ARMeilleure.Instructions
{
    static partial class InstEmit
    {
        private enum CselOperation
        {
            None,
            Increment,
            Invert,
            Negate,
        }

        public static void Csel(ArmEmitterContext context) => EmitCsel(context, CselOperation.None);
        public static void Csinc(ArmEmitterContext context) => EmitCsel(context, CselOperation.Increment);
        public static void Csinv(ArmEmitterContext context) => EmitCsel(context, CselOperation.Invert);
        public static void Csneg(ArmEmitterContext context) => EmitCsel(context, CselOperation.Negate);

        private static void EmitCsel(ArmEmitterContext context, CselOperation cselOp)
        {
            OpCodeCsel op = (OpCodeCsel)context.CurrOp;

            Operand n = GetIntOrZR(context, op.Rn);
            Operand m = GetIntOrZR(context, op.Rm);

            if (cselOp == CselOperation.Increment)
            {
                m = context.Add(m, Const(m.Type, 1));
            }
            else if (cselOp == CselOperation.Invert)
            {
                m = context.BitwiseNot(m);
            }
            else if (cselOp == CselOperation.Negate)
            {
                m = context.Negate(m);
            }

            Operand condTrue = GetCondTrue(context, op.Cond);

            Operand d = context.ConditionalSelect(condTrue, n, m);

            SetIntOrZR(context, op.Rd, d);
        }
    }
}
