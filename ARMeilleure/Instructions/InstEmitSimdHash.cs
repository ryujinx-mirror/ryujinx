using ARMeilleure.Decoders;
using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.Translation;

using static ARMeilleure.Instructions.InstEmitHelper;

namespace ARMeilleure.Instructions
{
    static partial class InstEmit
    {
#region "Sha1"
        public static void Sha1c_V(ArmEmitterContext context)
        {
            OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

            Operand d = GetVec(op.Rd);

            Operand ne = context.VectorExtract(OperandType.I32, GetVec(op.Rn), 0);

            Operand m = GetVec(op.Rm);

            Operand res = context.Call(new _V128_V128_U32_V128(SoftFallback.HashChoose), d, ne, m);

            context.Copy(GetVec(op.Rd), res);
        }

        public static void Sha1h_V(ArmEmitterContext context)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            Operand ne = context.VectorExtract(OperandType.I32, GetVec(op.Rn), 0);

            Operand res = context.Call(new _U32_U32(SoftFallback.FixedRotate), ne);

            context.Copy(GetVec(op.Rd), context.VectorCreateScalar(res));
        }

        public static void Sha1m_V(ArmEmitterContext context)
        {
            OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

            Operand d = GetVec(op.Rd);

            Operand ne = context.VectorExtract(OperandType.I32, GetVec(op.Rn), 0);

            Operand m = GetVec(op.Rm);

            Operand res = context.Call(new _V128_V128_U32_V128(SoftFallback.HashMajority), d, ne, m);

            context.Copy(GetVec(op.Rd), res);
        }

        public static void Sha1p_V(ArmEmitterContext context)
        {
            OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

            Operand d = GetVec(op.Rd);

            Operand ne = context.VectorExtract(OperandType.I32, GetVec(op.Rn), 0);

            Operand m = GetVec(op.Rm);

            Operand res = context.Call(new _V128_V128_U32_V128(SoftFallback.HashParity), d, ne, m);

            context.Copy(GetVec(op.Rd), res);
        }

        public static void Sha1su0_V(ArmEmitterContext context)
        {
            OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

            Operand d = GetVec(op.Rd);
            Operand n = GetVec(op.Rn);
            Operand m = GetVec(op.Rm);

            Operand res = context.Call(new _V128_V128_V128_V128(SoftFallback.Sha1SchedulePart1), d, n, m);

            context.Copy(GetVec(op.Rd), res);
        }

        public static void Sha1su1_V(ArmEmitterContext context)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            Operand d = GetVec(op.Rd);
            Operand n = GetVec(op.Rn);

            Operand res = context.Call(new _V128_V128_V128(SoftFallback.Sha1SchedulePart2), d, n);

            context.Copy(GetVec(op.Rd), res);
        }
#endregion

#region "Sha256"
        public static void Sha256h_V(ArmEmitterContext context)
        {
            OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

            Operand d = GetVec(op.Rd);
            Operand n = GetVec(op.Rn);
            Operand m = GetVec(op.Rm);

            Operand res = context.Call(new _V128_V128_V128_V128(SoftFallback.HashLower), d, n, m);

            context.Copy(GetVec(op.Rd), res);
        }

        public static void Sha256h2_V(ArmEmitterContext context)
        {
            OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

            Operand d = GetVec(op.Rd);
            Operand n = GetVec(op.Rn);
            Operand m = GetVec(op.Rm);

            Operand res = context.Call(new _V128_V128_V128_V128(SoftFallback.HashUpper), d, n, m);

            context.Copy(GetVec(op.Rd), res);
        }

        public static void Sha256su0_V(ArmEmitterContext context)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            Operand d = GetVec(op.Rd);
            Operand n = GetVec(op.Rn);

            Operand res = context.Call(new _V128_V128_V128(SoftFallback.Sha256SchedulePart1), d, n);

            context.Copy(GetVec(op.Rd), res);
        }

        public static void Sha256su1_V(ArmEmitterContext context)
        {
            OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

            Operand d = GetVec(op.Rd);
            Operand n = GetVec(op.Rn);
            Operand m = GetVec(op.Rm);

            Operand res = context.Call(new _V128_V128_V128_V128(SoftFallback.Sha256SchedulePart2), d, n, m);

            context.Copy(GetVec(op.Rd), res);
        }
#endregion
    }
}
