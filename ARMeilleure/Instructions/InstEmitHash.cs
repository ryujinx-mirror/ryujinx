// https://www.intel.com/content/dam/www/public/us/en/documents/white-papers/fast-crc-computation-generic-polynomials-pclmulqdq-paper.pdf

using ARMeilleure.Decoders;
using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.Translation;

using static ARMeilleure.Instructions.InstEmitHelper;
using static ARMeilleure.Instructions.InstEmitSimdHelper;
using static ARMeilleure.IntermediateRepresentation.OperandHelper;

namespace ARMeilleure.Instructions
{
    static partial class InstEmit
    {
        public static void Crc32b(ArmEmitterContext context)
        {
            if (Optimizations.UsePclmulqdq)
            {
                EmitCrc32Optimized(context, false, 8);
            }
            else
            {
                EmitCrc32Call(context, nameof(SoftFallback.Crc32b));
            }
        }

        public static void Crc32h(ArmEmitterContext context)
        {
            if (Optimizations.UsePclmulqdq)
            {
                EmitCrc32Optimized(context, false, 16);
            }
            else
            {
                EmitCrc32Call(context, nameof(SoftFallback.Crc32h));
            }
        }

        public static void Crc32w(ArmEmitterContext context)
        {
            if (Optimizations.UsePclmulqdq)
            {
                EmitCrc32Optimized(context, false, 32);
            }
            else
            {
                EmitCrc32Call(context, nameof(SoftFallback.Crc32w));
            }
        }

        public static void Crc32x(ArmEmitterContext context)
        {
            if (Optimizations.UsePclmulqdq)
            {
                EmitCrc32Optimized64(context, false);
            }
            else
            {
                EmitCrc32Call(context, nameof(SoftFallback.Crc32x));
            }
        }

        public static void Crc32cb(ArmEmitterContext context)
        {
            if (Optimizations.UsePclmulqdq)
            {
                EmitCrc32Optimized(context, true, 8);
            }
            else
            {
                EmitCrc32Call(context, nameof(SoftFallback.Crc32cb));
            }
        }

        public static void Crc32ch(ArmEmitterContext context)
        {
            if (Optimizations.UsePclmulqdq)
            {
                EmitCrc32Optimized(context, true, 16);
            }
            else
            {
                EmitCrc32Call(context, nameof(SoftFallback.Crc32ch));
            }
        }

        public static void Crc32cw(ArmEmitterContext context)
        {
            if (Optimizations.UsePclmulqdq)
            {
                EmitCrc32Optimized(context, true, 32);
            }
            else
            {
                EmitCrc32Call(context, nameof(SoftFallback.Crc32cw));
            }
        }

        public static void Crc32cx(ArmEmitterContext context)
        {
            if (Optimizations.UsePclmulqdq)
            {
                EmitCrc32Optimized64(context, true);
            }
            else
            {
                EmitCrc32Call(context, nameof(SoftFallback.Crc32cx));
            }
        }

        private static void EmitCrc32Optimized(ArmEmitterContext context, bool castagnoli, int bitsize)
        {
            OpCodeAluBinary op = (OpCodeAluBinary)context.CurrOp;

            long mu = castagnoli ? 0x0DEA713F1 : 0x1F7011641; // mu' = floor(x^64/P(x))'
            long polynomial = castagnoli ? 0x105EC76F0 : 0x1DB710641; // P'(x) << 1

            Operand crc = GetIntOrZR(context, op.Rn);
            Operand data = GetIntOrZR(context, op.Rm);

            crc = context.VectorInsert(context.VectorZero(), crc, 0);

            switch (bitsize)
            {
                case 8: data = context.VectorInsert8(context.VectorZero(), data, 0); break;
                case 16: data = context.VectorInsert16(context.VectorZero(), data, 0); break;
                case 32: data = context.VectorInsert(context.VectorZero(), data, 0); break;
            }

            Operand tmp = context.AddIntrinsic(Intrinsic.X86Pxor, crc, data);
            tmp = context.AddIntrinsic(Intrinsic.X86Psllq, tmp, Const(64 - bitsize));
            tmp = context.AddIntrinsic(Intrinsic.X86Pclmulqdq, tmp, X86GetScalar(context, mu), Const(0));
            tmp = context.AddIntrinsic(Intrinsic.X86Pclmulqdq, tmp, X86GetScalar(context, polynomial), Const(0));

            if (bitsize < 32)
            {
                crc = context.AddIntrinsic(Intrinsic.X86Pslldq, crc, Const((64 - bitsize) / 8));
                tmp = context.AddIntrinsic(Intrinsic.X86Pxor, tmp, crc);
            }

            SetIntOrZR(context, op.Rd, context.VectorExtract(OperandType.I32, tmp, 2));
        }

        private static void EmitCrc32Optimized64(ArmEmitterContext context, bool castagnoli)
        {
            OpCodeAluBinary op = (OpCodeAluBinary)context.CurrOp;

            long mu = castagnoli ? 0x0DEA713F1 : 0x1F7011641; // mu' = floor(x^64/P(x))'
            long polynomial = castagnoli ? 0x105EC76F0 : 0x1DB710641; // P'(x) << 1

            Operand crc = GetIntOrZR(context, op.Rn);
            Operand data = GetIntOrZR(context, op.Rm);

            crc = context.VectorInsert(context.VectorZero(), crc, 0);
            data = context.VectorInsert(context.VectorZero(), data, 0);

            Operand tmp = context.AddIntrinsic(Intrinsic.X86Pxor, crc, data);
            Operand res = context.AddIntrinsic(Intrinsic.X86Pslldq, tmp, Const(4));

            tmp = context.AddIntrinsic(Intrinsic.X86Pclmulqdq, res, X86GetScalar(context, mu), Const(0));
            tmp = context.AddIntrinsic(Intrinsic.X86Pclmulqdq, tmp, X86GetScalar(context, polynomial), Const(0));

            tmp = context.AddIntrinsic(Intrinsic.X86Pxor, tmp, res);
            tmp = context.AddIntrinsic(Intrinsic.X86Psllq, tmp, Const(32));

            tmp = context.AddIntrinsic(Intrinsic.X86Pclmulqdq, tmp, X86GetScalar(context, mu), Const(1));
            tmp = context.AddIntrinsic(Intrinsic.X86Pclmulqdq, tmp, X86GetScalar(context, polynomial), Const(0));

            SetIntOrZR(context, op.Rd, context.VectorExtract(OperandType.I32, tmp, 2));
        }

        private static void EmitCrc32Call(ArmEmitterContext context, string name)
        {
            OpCodeAluBinary op = (OpCodeAluBinary)context.CurrOp;

            Operand n = GetIntOrZR(context, op.Rn);
            Operand m = GetIntOrZR(context, op.Rm);

            Operand d = context.Call(typeof(SoftFallback).GetMethod(name), n, m);

            SetIntOrZR(context, op.Rd, d);
        }
    }
}
