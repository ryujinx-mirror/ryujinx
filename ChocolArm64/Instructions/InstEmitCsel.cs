using ChocolArm64.Decoders;
using ChocolArm64.Translation;
using System.Reflection.Emit;

namespace ChocolArm64.Instructions
{
    static partial class InstEmit
    {
        private enum CselOperation
        {
            None,
            Increment,
            Invert,
            Negate
        }

        public static void Csel(ILEmitterCtx context)  => EmitCsel(context, CselOperation.None);
        public static void Csinc(ILEmitterCtx context) => EmitCsel(context, CselOperation.Increment);
        public static void Csinv(ILEmitterCtx context) => EmitCsel(context, CselOperation.Invert);
        public static void Csneg(ILEmitterCtx context) => EmitCsel(context, CselOperation.Negate);

        private static void EmitCsel(ILEmitterCtx context, CselOperation cselOp)
        {
            OpCodeCsel64 op = (OpCodeCsel64)context.CurrOp;

            ILLabel lblTrue = new ILLabel();
            ILLabel lblEnd  = new ILLabel();

            context.EmitCondBranch(lblTrue, op.Cond);
            context.EmitLdintzr(op.Rm);

            if (cselOp == CselOperation.Increment)
            {
                context.EmitLdc_I(1);

                context.Emit(OpCodes.Add);
            }
            else if (cselOp == CselOperation.Invert)
            {
                context.Emit(OpCodes.Not);
            }
            else if (cselOp == CselOperation.Negate)
            {
                context.Emit(OpCodes.Neg);
            }

            context.Emit(OpCodes.Br_S, lblEnd);

            context.MarkLabel(lblTrue);

            context.EmitLdintzr(op.Rn);

            context.MarkLabel(lblEnd);

            context.EmitStintzr(op.Rd);
        }
    }
}