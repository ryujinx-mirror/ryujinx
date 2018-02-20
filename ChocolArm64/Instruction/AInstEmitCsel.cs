using ChocolArm64.Decoder;
using ChocolArm64.Translation;
using System.Reflection.Emit;

namespace ChocolArm64.Instruction
{
    static partial class AInstEmit
    {
        private enum CselOperation
        {
            None,
            Increment,
            Invert,
            Negate
        }

        public static void Csel(AILEmitterCtx Context)  => EmitCsel(Context, CselOperation.None);
        public static void Csinc(AILEmitterCtx Context) => EmitCsel(Context, CselOperation.Increment);
        public static void Csinv(AILEmitterCtx Context) => EmitCsel(Context, CselOperation.Invert);
        public static void Csneg(AILEmitterCtx Context) => EmitCsel(Context, CselOperation.Negate);

        private static void EmitCsel(AILEmitterCtx Context, CselOperation CselOp)
        {
            AOpCodeCsel Op = (AOpCodeCsel)Context.CurrOp;

            AILLabel LblTrue = new AILLabel();
            AILLabel LblEnd  = new AILLabel();

            Context.EmitCondBranch(LblTrue, Op.Cond);
            Context.EmitLdintzr(Op.Rm);

            if (CselOp == CselOperation.Increment)
            {
                Context.EmitLdc_I(1);

                Context.Emit(OpCodes.Add);
            }
            else if (CselOp == CselOperation.Invert)
            {
                Context.Emit(OpCodes.Not);
            }
            else if (CselOp == CselOperation.Negate)
            {
                Context.Emit(OpCodes.Neg);
            }

            Context.EmitStintzr(Op.Rd);

            Context.Emit(OpCodes.Br_S, LblEnd);

            Context.MarkLabel(LblTrue);

            Context.EmitLdintzr(Op.Rn);
            Context.EmitStintzr(Op.Rd);

            Context.MarkLabel(LblEnd);
        }
    }
}