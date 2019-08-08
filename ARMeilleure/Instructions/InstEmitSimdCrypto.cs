using ARMeilleure.Decoders;
using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.Translation;

using static ARMeilleure.Instructions.InstEmitHelper;

namespace ARMeilleure.Instructions
{
    static partial class InstEmit
    {
        public static void Aesd_V(ArmEmitterContext context)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            Operand d = GetVec(op.Rd);
            Operand n = GetVec(op.Rn);

            context.Copy(d, context.Call(new _V128_V128_V128(SoftFallback.Decrypt), d, n));
        }

        public static void Aese_V(ArmEmitterContext context)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            Operand d = GetVec(op.Rd);
            Operand n = GetVec(op.Rn);

            context.Copy(d, context.Call(new _V128_V128_V128(SoftFallback.Encrypt), d, n));
        }

        public static void Aesimc_V(ArmEmitterContext context)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            Operand n = GetVec(op.Rn);

            context.Copy(GetVec(op.Rd), context.Call(new _V128_V128(SoftFallback.InverseMixColumns), n));
        }

        public static void Aesmc_V(ArmEmitterContext context)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            Operand n = GetVec(op.Rn);

            context.Copy(GetVec(op.Rd), context.Call(new _V128_V128(SoftFallback.MixColumns), n));
        }
    }
}
