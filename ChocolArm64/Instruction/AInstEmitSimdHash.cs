using ChocolArm64.Decoder;
using ChocolArm64.Translation;

namespace ChocolArm64.Instruction
{
    static partial class AInstEmit
    {
#region "Sha256"
        public static void Sha256h_V(AILEmitterCtx Context)
        {
            AOpCodeSimdReg Op = (AOpCodeSimdReg)Context.CurrOp;

            Context.EmitLdvec(Op.Rd);
            Context.EmitLdvec(Op.Rn);
            Context.EmitLdvec(Op.Rm);

            ASoftFallback.EmitCall(Context, nameof(ASoftFallback.HashLower));

            Context.EmitStvec(Op.Rd);
        }

        public static void Sha256h2_V(AILEmitterCtx Context)
        {
            AOpCodeSimdReg Op = (AOpCodeSimdReg)Context.CurrOp;

            Context.EmitLdvec(Op.Rd);
            Context.EmitLdvec(Op.Rn);
            Context.EmitLdvec(Op.Rm);

            ASoftFallback.EmitCall(Context, nameof(ASoftFallback.HashUpper));

            Context.EmitStvec(Op.Rd);
        }

        public static void Sha256su0_V(AILEmitterCtx Context)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            Context.EmitLdvec(Op.Rd);
            Context.EmitLdvec(Op.Rn);

            ASoftFallback.EmitCall(Context, nameof(ASoftFallback.SchedulePart1));

            Context.EmitStvec(Op.Rd);
        }

        public static void Sha256su1_V(AILEmitterCtx Context)
        {
            AOpCodeSimdReg Op = (AOpCodeSimdReg)Context.CurrOp;

            Context.EmitLdvec(Op.Rd);
            Context.EmitLdvec(Op.Rn);
            Context.EmitLdvec(Op.Rm);

            ASoftFallback.EmitCall(Context, nameof(ASoftFallback.SchedulePart2));

            Context.EmitStvec(Op.Rd);
        }
#endregion
    }
}
