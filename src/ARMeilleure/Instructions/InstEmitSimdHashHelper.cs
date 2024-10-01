using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.Translation;
using System;

using static ARMeilleure.IntermediateRepresentation.Operand.Factory;

namespace ARMeilleure.Instructions
{
    static class InstEmitSimdHashHelper
    {
        public static Operand EmitSha256h(ArmEmitterContext context, Operand x, Operand y, Operand w, bool part2)
        {
            if (Optimizations.UseSha)
            {
                Operand src1 = context.AddIntrinsic(Intrinsic.X86Shufps, y, x, Const(0xbb));
                Operand src2 = context.AddIntrinsic(Intrinsic.X86Shufps, y, x, Const(0x11));
                Operand w2 = context.AddIntrinsic(Intrinsic.X86Punpckhqdq, w, w);

                Operand round2 = context.AddIntrinsic(Intrinsic.X86Sha256Rnds2, src1, src2, w);
                Operand round4 = context.AddIntrinsic(Intrinsic.X86Sha256Rnds2, src2, round2, w2);

                Operand res = context.AddIntrinsic(Intrinsic.X86Shufps, round4, round2, Const(part2 ? 0x11 : 0xbb));

                return res;
            }

            String method = part2 ? nameof(SoftFallback.HashUpper) : nameof(SoftFallback.HashLower);
            return context.Call(typeof(SoftFallback).GetMethod(method), x, y, w);
        }

        public static Operand EmitSha256su0(ArmEmitterContext context, Operand x, Operand y)
        {
            if (Optimizations.UseSha)
            {
                return context.AddIntrinsic(Intrinsic.X86Sha256Msg1, x, y);
            }

            return context.Call(typeof(SoftFallback).GetMethod(nameof(SoftFallback.Sha256SchedulePart1)), x, y);
        }

        public static Operand EmitSha256su1(ArmEmitterContext context, Operand x, Operand y, Operand z)
        {
            if (Optimizations.UseSha && Optimizations.UseSsse3)
            {
                Operand extr = context.AddIntrinsic(Intrinsic.X86Palignr, z, y, Const(4));
                Operand tmp = context.AddIntrinsic(Intrinsic.X86Paddd, extr, x);

                Operand res = context.AddIntrinsic(Intrinsic.X86Sha256Msg2, tmp, z);

                return res;
            }

            return context.Call(typeof(SoftFallback).GetMethod(nameof(SoftFallback.Sha256SchedulePart2)), x, y, z);
        }
    }
}
