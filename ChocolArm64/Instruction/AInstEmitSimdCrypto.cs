using ChocolArm64.Decoder;
using ChocolArm64.Translation;

namespace ChocolArm64.Instruction
{
    static partial class AInstEmit
    {
        public static void Aesd_V(AILEmitterCtx Context)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            Context.EmitLdvec(Op.Rd);
            Context.EmitLdvec(Op.Rn);

            ASoftFallback.EmitCall(Context, nameof(ASoftFallback.Decrypt));

            Context.EmitStvec(Op.Rd);
        }

        public static void Aese_V(AILEmitterCtx Context)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            Context.EmitLdvec(Op.Rd);
            Context.EmitLdvec(Op.Rn);

            ASoftFallback.EmitCall(Context, nameof(ASoftFallback.Encrypt));

            Context.EmitStvec(Op.Rd);
        }

        public static void Aesimc_V(AILEmitterCtx Context)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            Context.EmitLdvec(Op.Rn);

            ASoftFallback.EmitCall(Context, nameof(ASoftFallback.InverseMixColumns));

            Context.EmitStvec(Op.Rd);
        }

        public static void Aesmc_V(AILEmitterCtx Context)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            Context.EmitLdvec(Op.Rn);

            ASoftFallback.EmitCall(Context, nameof(ASoftFallback.MixColumns));

            Context.EmitStvec(Op.Rd);
        }
    }
}
