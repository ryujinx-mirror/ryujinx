using ChocolArm64.Decoder;
using ChocolArm64.Translation;

using static ChocolArm64.Instruction.AInstEmitSimdHelper;

namespace ChocolArm64.Instruction
{
    static partial class AInstEmit
    {
#region "Sha1"
        public static void Sha1c_V(AILEmitterCtx Context)
        {
            AOpCodeSimdReg Op = (AOpCodeSimdReg)Context.CurrOp;

            Context.EmitLdvec(Op.Rd);
            EmitVectorExtractZx(Context, Op.Rn, 0, 2);
            Context.EmitLdvec(Op.Rm);

            ASoftFallback.EmitCall(Context, nameof(ASoftFallback.HashChoose));

            Context.EmitStvec(Op.Rd);
        }

        public static void Sha1h_V(AILEmitterCtx Context)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            EmitVectorExtractZx(Context, Op.Rn, 0, 2);

            ASoftFallback.EmitCall(Context, nameof(ASoftFallback.FixedRotate));

            EmitScalarSet(Context, Op.Rd, 2);
        }

        public static void Sha1m_V(AILEmitterCtx Context)
        {
            AOpCodeSimdReg Op = (AOpCodeSimdReg)Context.CurrOp;

            Context.EmitLdvec(Op.Rd);
            EmitVectorExtractZx(Context, Op.Rn, 0, 2);
            Context.EmitLdvec(Op.Rm);

            ASoftFallback.EmitCall(Context, nameof(ASoftFallback.HashMajority));

            Context.EmitStvec(Op.Rd);
        }

        public static void Sha1p_V(AILEmitterCtx Context)
        {
            AOpCodeSimdReg Op = (AOpCodeSimdReg)Context.CurrOp;

            Context.EmitLdvec(Op.Rd);
            EmitVectorExtractZx(Context, Op.Rn, 0, 2);
            Context.EmitLdvec(Op.Rm);

            ASoftFallback.EmitCall(Context, nameof(ASoftFallback.HashParity));

            Context.EmitStvec(Op.Rd);
        }

        public static void Sha1su0_V(AILEmitterCtx Context)
        {
            AOpCodeSimdReg Op = (AOpCodeSimdReg)Context.CurrOp;

            Context.EmitLdvec(Op.Rd);
            Context.EmitLdvec(Op.Rn);
            Context.EmitLdvec(Op.Rm);

            ASoftFallback.EmitCall(Context, nameof(ASoftFallback.Sha1SchedulePart1));

            Context.EmitStvec(Op.Rd);
        }

        public static void Sha1su1_V(AILEmitterCtx Context)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            Context.EmitLdvec(Op.Rd);
            Context.EmitLdvec(Op.Rn);

            ASoftFallback.EmitCall(Context, nameof(ASoftFallback.Sha1SchedulePart2));

            Context.EmitStvec(Op.Rd);
        }
#endregion

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

            ASoftFallback.EmitCall(Context, nameof(ASoftFallback.Sha256SchedulePart1));

            Context.EmitStvec(Op.Rd);
        }

        public static void Sha256su1_V(AILEmitterCtx Context)
        {
            AOpCodeSimdReg Op = (AOpCodeSimdReg)Context.CurrOp;

            Context.EmitLdvec(Op.Rd);
            Context.EmitLdvec(Op.Rn);
            Context.EmitLdvec(Op.Rm);

            ASoftFallback.EmitCall(Context, nameof(ASoftFallback.Sha256SchedulePart2));

            Context.EmitStvec(Op.Rd);
        }
#endregion
    }
}
