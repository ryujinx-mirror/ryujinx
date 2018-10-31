using ChocolArm64.Decoders;
using ChocolArm64.Translation;

using static ChocolArm64.Instructions.InstEmitSimdHelper;

namespace ChocolArm64.Instructions
{
    static partial class InstEmit
    {
#region "Sha1"
        public static void Sha1c_V(ILEmitterCtx context)
        {
            OpCodeSimdReg64 op = (OpCodeSimdReg64)context.CurrOp;

            context.EmitLdvec(op.Rd);
            EmitVectorExtractZx(context, op.Rn, 0, 2);
            context.EmitLdvec(op.Rm);

            SoftFallback.EmitCall(context, nameof(SoftFallback.HashChoose));

            context.EmitStvec(op.Rd);
        }

        public static void Sha1h_V(ILEmitterCtx context)
        {
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            EmitVectorExtractZx(context, op.Rn, 0, 2);

            SoftFallback.EmitCall(context, nameof(SoftFallback.FixedRotate));

            EmitScalarSet(context, op.Rd, 2);
        }

        public static void Sha1m_V(ILEmitterCtx context)
        {
            OpCodeSimdReg64 op = (OpCodeSimdReg64)context.CurrOp;

            context.EmitLdvec(op.Rd);
            EmitVectorExtractZx(context, op.Rn, 0, 2);
            context.EmitLdvec(op.Rm);

            SoftFallback.EmitCall(context, nameof(SoftFallback.HashMajority));

            context.EmitStvec(op.Rd);
        }

        public static void Sha1p_V(ILEmitterCtx context)
        {
            OpCodeSimdReg64 op = (OpCodeSimdReg64)context.CurrOp;

            context.EmitLdvec(op.Rd);
            EmitVectorExtractZx(context, op.Rn, 0, 2);
            context.EmitLdvec(op.Rm);

            SoftFallback.EmitCall(context, nameof(SoftFallback.HashParity));

            context.EmitStvec(op.Rd);
        }

        public static void Sha1su0_V(ILEmitterCtx context)
        {
            OpCodeSimdReg64 op = (OpCodeSimdReg64)context.CurrOp;

            context.EmitLdvec(op.Rd);
            context.EmitLdvec(op.Rn);
            context.EmitLdvec(op.Rm);

            SoftFallback.EmitCall(context, nameof(SoftFallback.Sha1SchedulePart1));

            context.EmitStvec(op.Rd);
        }

        public static void Sha1su1_V(ILEmitterCtx context)
        {
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            context.EmitLdvec(op.Rd);
            context.EmitLdvec(op.Rn);

            SoftFallback.EmitCall(context, nameof(SoftFallback.Sha1SchedulePart2));

            context.EmitStvec(op.Rd);
        }
#endregion

#region "Sha256"
        public static void Sha256h_V(ILEmitterCtx context)
        {
            OpCodeSimdReg64 op = (OpCodeSimdReg64)context.CurrOp;

            context.EmitLdvec(op.Rd);
            context.EmitLdvec(op.Rn);
            context.EmitLdvec(op.Rm);

            SoftFallback.EmitCall(context, nameof(SoftFallback.HashLower));

            context.EmitStvec(op.Rd);
        }

        public static void Sha256h2_V(ILEmitterCtx context)
        {
            OpCodeSimdReg64 op = (OpCodeSimdReg64)context.CurrOp;

            context.EmitLdvec(op.Rd);
            context.EmitLdvec(op.Rn);
            context.EmitLdvec(op.Rm);

            SoftFallback.EmitCall(context, nameof(SoftFallback.HashUpper));

            context.EmitStvec(op.Rd);
        }

        public static void Sha256su0_V(ILEmitterCtx context)
        {
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            context.EmitLdvec(op.Rd);
            context.EmitLdvec(op.Rn);

            SoftFallback.EmitCall(context, nameof(SoftFallback.Sha256SchedulePart1));

            context.EmitStvec(op.Rd);
        }

        public static void Sha256su1_V(ILEmitterCtx context)
        {
            OpCodeSimdReg64 op = (OpCodeSimdReg64)context.CurrOp;

            context.EmitLdvec(op.Rd);
            context.EmitLdvec(op.Rn);
            context.EmitLdvec(op.Rm);

            SoftFallback.EmitCall(context, nameof(SoftFallback.Sha256SchedulePart2));

            context.EmitStvec(op.Rd);
        }
#endregion
    }
}
