using ARMeilleure.Decoders;
using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.Translation;

using static ARMeilleure.Instructions.InstEmitHelper;

namespace ARMeilleure.Instructions
{
    partial class InstEmit32
    {
        public static void Aesd_V(ArmEmitterContext context)
        {
            OpCode32Simd op = (OpCode32Simd)context.CurrOp;

            Operand d = GetVecA32(op.Qd);
            Operand n = GetVecA32(op.Qm);

            Operand res;

            if (Optimizations.UseArm64Aes)
            {
                res = context.AddIntrinsic(Intrinsic.Arm64AesdV, d, n);
            }
            else if (Optimizations.UseAesni)
            {
                res = context.AddIntrinsic(Intrinsic.X86Aesdeclast, context.AddIntrinsic(Intrinsic.X86Xorpd, d, n), context.VectorZero());
            }
            else
            {
                res = context.Call(typeof(SoftFallback).GetMethod(nameof(SoftFallback.Decrypt)), d, n);
            }

            context.Copy(d, res);
        }

        public static void Aese_V(ArmEmitterContext context)
        {
            OpCode32Simd op = (OpCode32Simd)context.CurrOp;

            Operand d = GetVecA32(op.Qd);
            Operand n = GetVecA32(op.Qm);

            Operand res;

            if (Optimizations.UseArm64Aes)
            {
                res = context.AddIntrinsic(Intrinsic.Arm64AeseV, d, n);
            }
            else if (Optimizations.UseAesni)
            {
                res = context.AddIntrinsic(Intrinsic.X86Aesenclast, context.AddIntrinsic(Intrinsic.X86Xorpd, d, n), context.VectorZero());
            }
            else
            {
                res = context.Call(typeof(SoftFallback).GetMethod(nameof(SoftFallback.Encrypt)), d, n);
            }

            context.Copy(d, res);
        }

        public static void Aesimc_V(ArmEmitterContext context)
        {
            OpCode32Simd op = (OpCode32Simd)context.CurrOp;

            Operand n = GetVecA32(op.Qm);

            Operand res;

            if (Optimizations.UseArm64Aes)
            {
                res = context.AddIntrinsic(Intrinsic.Arm64AesimcV, n);
            }
            else if (Optimizations.UseAesni)
            {
                res = context.AddIntrinsic(Intrinsic.X86Aesimc, n);
            }
            else
            {
                res = context.Call(typeof(SoftFallback).GetMethod(nameof(SoftFallback.InverseMixColumns)), n);
            }

            context.Copy(GetVecA32(op.Qd), res);
        }

        public static void Aesmc_V(ArmEmitterContext context)
        {
            OpCode32Simd op = (OpCode32Simd)context.CurrOp;

            Operand n = GetVecA32(op.Qm);

            Operand res;

            if (Optimizations.UseArm64Aes)
            {
                res = context.AddIntrinsic(Intrinsic.Arm64AesmcV, n);
            }
            else if (Optimizations.UseAesni)
            {
                Operand roundKey = context.VectorZero();

                // Inverse Shift Rows, Inverse Sub Bytes, xor 0 so nothing happens.
                res = context.AddIntrinsic(Intrinsic.X86Aesdeclast, n, roundKey);

                // Shift Rows, Sub Bytes, Mix Columns (!), xor 0 so nothing happens.
                res = context.AddIntrinsic(Intrinsic.X86Aesenc, res, roundKey);
            }
            else
            {
                res = context.Call(typeof(SoftFallback).GetMethod(nameof(SoftFallback.MixColumns)), n);
            }

            context.Copy(GetVecA32(op.Qd), res);
        }
    }
}
