using ARMeilleure.Decoders;
using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.Translation;

using static ARMeilleure.Instructions.InstEmitHelper;

namespace ARMeilleure.Instructions
{
    static partial class InstEmit32
    {
#region "Sha256"
        public static void Sha256h_V(ArmEmitterContext context)
        {
            OpCode32SimdReg op = (OpCode32SimdReg)context.CurrOp;

            Operand d = GetVecA32(op.Qd);
            Operand n = GetVecA32(op.Qn);
            Operand m = GetVecA32(op.Qm);

            Operand res = context.Call(typeof(SoftFallback).GetMethod(nameof(SoftFallback.HashLower)), d, n, m);

            context.Copy(GetVecA32(op.Qd), res);
        }

        public static void Sha256h2_V(ArmEmitterContext context)
        {
            OpCode32SimdReg op = (OpCode32SimdReg)context.CurrOp;

            Operand d = GetVecA32(op.Qd);
            Operand n = GetVecA32(op.Qn);
            Operand m = GetVecA32(op.Qm);

            Operand res = context.Call(typeof(SoftFallback).GetMethod(nameof(SoftFallback.HashUpper)), d, n, m);

            context.Copy(GetVecA32(op.Qd), res);
        }

        public static void Sha256su0_V(ArmEmitterContext context)
        {
            OpCode32Simd op = (OpCode32Simd)context.CurrOp;

            Operand d = GetVecA32(op.Qd);
            Operand m = GetVecA32(op.Qm);

            Operand res = context.Call(typeof(SoftFallback).GetMethod(nameof(SoftFallback.Sha256SchedulePart1)), d, m);

            context.Copy(GetVecA32(op.Qd), res);
        }

        public static void Sha256su1_V(ArmEmitterContext context)
        {
            OpCode32SimdReg op = (OpCode32SimdReg)context.CurrOp;

            Operand d = GetVecA32(op.Qd);
            Operand n = GetVecA32(op.Qn);
            Operand m = GetVecA32(op.Qm);

            Operand res = context.Call(typeof(SoftFallback).GetMethod(nameof(SoftFallback.Sha256SchedulePart2)), d, n, m);

            context.Copy(GetVecA32(op.Qd), res);
        }
#endregion
    }
}
