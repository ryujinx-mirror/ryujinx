using ChocolArm64.Decoder;
using ChocolArm64.State;
using ChocolArm64.Translation;
using System;

namespace ChocolArm64.Instruction
{
    static partial class AInstEmit
    {
        public static void Svc(AILEmitterCtx Context)
        {
            AOpCodeException Op = (AOpCodeException)Context.CurrOp;

            Context.EmitStoreState();

            Context.EmitLdarg(ATranslatedSub.RegistersArgIdx);

            Context.EmitLdc_I4(Op.Id);

            Context.EmitCall(typeof(ARegisters), nameof(ARegisters.OnSvcCall));

            if (Context.CurrBlock.Next != null)
            {
                Context.EmitLoadState(Context.CurrBlock.Next);
            }
        }

        public static void Und(AILEmitterCtx Context)
        {
            throw new NotImplementedException($"Undefined instruction at {Context.CurrOp.Position:x16}");
        }
    }
}