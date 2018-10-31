using ChocolArm64.Decoders;
using ChocolArm64.Translation;

namespace ChocolArm64.Instructions
{
    static partial class InstEmit
    {
        public static void Aesd_V(ILEmitterCtx context)
        {
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            context.EmitLdvec(op.Rd);
            context.EmitLdvec(op.Rn);

            SoftFallback.EmitCall(context, nameof(SoftFallback.Decrypt));

            context.EmitStvec(op.Rd);
        }

        public static void Aese_V(ILEmitterCtx context)
        {
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            context.EmitLdvec(op.Rd);
            context.EmitLdvec(op.Rn);

            SoftFallback.EmitCall(context, nameof(SoftFallback.Encrypt));

            context.EmitStvec(op.Rd);
        }

        public static void Aesimc_V(ILEmitterCtx context)
        {
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            context.EmitLdvec(op.Rn);

            SoftFallback.EmitCall(context, nameof(SoftFallback.InverseMixColumns));

            context.EmitStvec(op.Rd);
        }

        public static void Aesmc_V(ILEmitterCtx context)
        {
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            context.EmitLdvec(op.Rn);

            SoftFallback.EmitCall(context, nameof(SoftFallback.MixColumns));

            context.EmitStvec(op.Rd);
        }
    }
}
