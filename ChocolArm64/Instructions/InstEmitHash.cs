using ChocolArm64.Decoders;
using ChocolArm64.State;
using ChocolArm64.Translation;
using System;
using System.Reflection.Emit;
using System.Runtime.Intrinsics.X86;

namespace ChocolArm64.Instructions
{
    static partial class InstEmit
    {
        public static void Crc32b(ILEmitterCtx context)
        {
            EmitCrc32(context, nameof(SoftFallback.Crc32B));
        }

        public static void Crc32h(ILEmitterCtx context)
        {
            EmitCrc32(context, nameof(SoftFallback.Crc32H));
        }

        public static void Crc32w(ILEmitterCtx context)
        {
            EmitCrc32(context, nameof(SoftFallback.Crc32W));
        }

        public static void Crc32x(ILEmitterCtx context)
        {
            EmitCrc32(context, nameof(SoftFallback.Crc32X));
        }

        public static void Crc32cb(ILEmitterCtx context)
        {
            if (Optimizations.UseSse42)
            {
                EmitSse42Crc32(context, typeof(uint), typeof(byte));
            }
            else
            {
                EmitCrc32(context, nameof(SoftFallback.Crc32Cb));
            }
        }

        public static void Crc32ch(ILEmitterCtx context)
        {
            if (Optimizations.UseSse42)
            {
                EmitSse42Crc32(context, typeof(uint), typeof(ushort));
            }
            else
            {
                EmitCrc32(context, nameof(SoftFallback.Crc32Ch));
            }
        }

        public static void Crc32cw(ILEmitterCtx context)
        {
            if (Optimizations.UseSse42)
            {
                EmitSse42Crc32(context, typeof(uint), typeof(uint));
            }
            else
            {
                EmitCrc32(context, nameof(SoftFallback.Crc32Cw));
            }
        }

        public static void Crc32cx(ILEmitterCtx context)
        {
            if (Optimizations.UseSse42)
            {
                EmitSse42Crc32(context, typeof(ulong), typeof(ulong));
            }
            else
            {
                EmitCrc32(context, nameof(SoftFallback.Crc32Cx));
            }
        }

        private static void EmitSse42Crc32(ILEmitterCtx context, Type tCrc, Type tData)
        {
            OpCodeAluRs64 op = (OpCodeAluRs64)context.CurrOp;

            context.EmitLdintzr(op.Rn);
            context.EmitLdintzr(op.Rm);

            context.EmitCall(typeof(Sse42).GetMethod(nameof(Sse42.Crc32), new Type[] { tCrc, tData }));

            context.EmitStintzr(op.Rd);
        }

        private static void EmitCrc32(ILEmitterCtx context, string name)
        {
            OpCodeAluRs64 op = (OpCodeAluRs64)context.CurrOp;

            context.EmitLdintzr(op.Rn);

            if (op.RegisterSize != RegisterSize.Int32)
            {
                context.Emit(OpCodes.Conv_U4);
            }

            context.EmitLdintzr(op.Rm);

            SoftFallback.EmitCall(context, name);

            if (op.RegisterSize != RegisterSize.Int32)
            {
                context.Emit(OpCodes.Conv_U8);
            }

            context.EmitStintzr(op.Rd);
        }
    }
}
