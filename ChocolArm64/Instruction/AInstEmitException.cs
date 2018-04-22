using ChocolArm64.Decoder;
using ChocolArm64.State;
using ChocolArm64.Translation;
using System.Reflection;
using System.Reflection.Emit;

namespace ChocolArm64.Instruction
{
    static partial class AInstEmit
    {
        public static void Brk(AILEmitterCtx Context)
        {
            EmitExceptionCall(Context, nameof(AThreadState.OnBreak));
        }

        public static void Svc(AILEmitterCtx Context)
        {
            EmitExceptionCall(Context, nameof(AThreadState.OnSvcCall));
        }

        private static void EmitExceptionCall(AILEmitterCtx Context, string MthdName)
        {
            AOpCodeException Op = (AOpCodeException)Context.CurrOp;

            Context.EmitStoreState();

            Context.EmitLdarg(ATranslatedSub.StateArgIdx);

            Context.EmitLdc_I4(Op.Id);

            Context.EmitPrivateCall(typeof(AThreadState), MthdName);

            //Check if the thread should still be running, if it isn't then we return 0
            //to force a return to the dispatcher and then exit the thread.
            Context.EmitLdarg(ATranslatedSub.StateArgIdx);

            Context.EmitCallPropGet(typeof(AThreadState), nameof(AThreadState.Running));

            AILLabel LblEnd = new AILLabel();

            Context.Emit(OpCodes.Brtrue_S, LblEnd);

            Context.EmitLdc_I8(0);

            Context.Emit(OpCodes.Ret);

            Context.MarkLabel(LblEnd);

            if (Context.CurrBlock.Next != null)
            {
                Context.EmitLoadState(Context.CurrBlock.Next);
            }
            else
            {
                Context.EmitLdc_I8(Op.Position + 4);

                Context.Emit(OpCodes.Ret);
            }
        }

        public static void Und(AILEmitterCtx Context)
        {
            AOpCode Op = Context.CurrOp;

            Context.EmitStoreState();

            Context.EmitLdarg(ATranslatedSub.StateArgIdx);

            Context.EmitLdc_I8(Op.Position);
            Context.EmitLdc_I4(Op.RawOpCode);

            Context.EmitPrivateCall(typeof(AThreadState), nameof(AThreadState.OnUndefined));

            if (Context.CurrBlock.Next != null)
            {
                Context.EmitLoadState(Context.CurrBlock.Next);
            }
            else
            {
                Context.EmitLdc_I8(Op.Position + 4);

                Context.Emit(OpCodes.Ret);
            }
        }
    }
}