using ChocolArm64.Translation;
using System.Reflection.Emit;

namespace ChocolArm64.Instructions
{
    static class InstEmitFlowHelper
    {
        public static void EmitCall(ILEmitterCtx context, long imm)
        {
            if (context.TryOptEmitSubroutineCall())
            {
                //Note: the return value of the called method will be placed
                //at the Stack, the return value is always a Int64 with the
                //return address of the function. We check if the address is
                //correct, if it isn't we keep returning until we reach the dispatcher.
                context.Emit(OpCodes.Dup);

                context.EmitLdc_I8(context.CurrOp.Position + 4);

                ILLabel lblContinue = new ILLabel();

                context.Emit(OpCodes.Beq_S, lblContinue);
                context.Emit(OpCodes.Ret);

                context.MarkLabel(lblContinue);

                context.Emit(OpCodes.Pop);

                context.EmitLoadState();
            }
            else
            {
                context.EmitLdc_I8(imm);

                context.Emit(OpCodes.Ret);
            }
        }
    }
}
