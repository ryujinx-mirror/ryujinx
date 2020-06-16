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

            Operand res = context.Call(typeof(SoftFallback).GetMethod(nameof(SoftFallback.HashChoose)), d, ne, m);

            context.Copy(GetVec(op.Rd), res);
        }

        public static void Sha1h_V(ArmEmitterContext context)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            Operand ne = context.VectorExtract(OperandType.I32, GetVec(op.Rn), 0);

            Operand res = context.Call(typeof(SoftFallback).GetMethod(nameof(SoftFallback.FixedRotate)), ne);

            context.Copy(GetVec(op.Rd), context.VectorCreateScalar(res));
        }

        public static void Sha1m_V(ArmEmitterContext context)
        {
            OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

            Operand d = GetVec(op.Rd);

            Operand ne = context.VectorExtract(OperandType.I32, GetVec(op.Rn), 0);

            Operand m = GetVec(op.Rm);

            Operand res = context.Call(typeof(SoftFallback).GetMethod(nameof(SoftFallback.HashMajority)), d, ne, m);

            context.Copy(GetVec(op.Rd), res);
        }

        public static void Sha1p_V(ArmEmitterContext context)
        {
            OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

            Operand d = GetVec(op.Rd);

            Operand ne = context.VectorExtract(OperandType.I32, GetVec(op.Rn), 0);

            Operand m = GetVec(op.Rm);

            Operand res = context.Call(typeof(SoftFallback).GetMethod(nameof(SoftFallback.HashParity)), d, ne, m);

            context.Copy(GetVec(op.Rd), res);
        }

        public static void Sha1su0_V(ArmEmitterContext context)
        {
            OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

            Operand d = GetVec(op.Rd);
            Operand n = GetVec(op.Rn);
            Operand m = GetVec(op.Rm);

            Operand res = context.Call(typeof(SoftFallback).GetMethod(nameof(SoftFallback.Sha1SchedulePart1)), d, n, m);

            context.Copy(GetVec(op.Rd), res);
        }

        public static void Sha1su1_V(ArmEmitterContext context)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            Operand d = GetVec(op.Rd);
            Operand n = GetVec(op.Rn);

            Operand res = context.Call(typeof(SoftFallback).GetMethod(nameof(SoftFallback.Sha1SchedulePart2)), d, n);

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

            Operand res = context.Call(typeof(SoftFallback).GetMethod(nameof(SoftFallback.HashLower)), d, n, m);

            context.Copy(GetVec(op.Rd), res);
        }

        public static void Sha256h2_V(ArmEmitterContext context)
        {
            OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

            Operand d = GetVec(op.Rd);
            Operand n = GetVec(op.Rn);
            Operand m = GetVec(op.Rm);

            Operand res = context.Call(typeof(SoftFallback).GetMethod(nameof(SoftFallback.HashUpper)), d, n, m);

            context.Copy(GetVec(op.Rd), res);
        }

        public static void Sha256su0_V(ArmEmitterContext context)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            Operand d = GetVec(op.Rd);
            Operand n = GetVec(op.Rn);

            Operand res = context.Call(typeof(SoftFallback).GetMethod(nameof(SoftFallback.Sha256SchedulePart1)), d, n);

            context.Copy(GetVec(op.Rd), res);
        }

        public static void Sha256su1_V(ArmEmitterContext context)
        {
            OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

            Operand d = GetVec(op.Rd);
            Operand n = GetVec(op.Rn);
            Operand m = GetVec(op.Rm);

            Operand res = context.Call(typeof(SoftFallback).GetMethod(nameof(SoftFallback.Sha256SchedulePart2)), d, n, m);

            context.Copy(GetVec(op.Rd), res);
        }
#endregion
    }
}
