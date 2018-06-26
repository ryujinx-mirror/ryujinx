using ChocolArm64.Decoder;
using ChocolArm64.State;
using ChocolArm64.Translation;
using System;
using System.Reflection.Emit;
using System.Runtime.Intrinsics.X86;

namespace ChocolArm64.Instruction
{
    static partial class AInstEmit
    {
        public static void Crc32b(AILEmitterCtx Context)
        {
            EmitCrc32(Context, nameof(ASoftFallback.Crc32b));
        }

        public static void Crc32h(AILEmitterCtx Context)
        {
            EmitCrc32(Context, nameof(ASoftFallback.Crc32h));
        }

        public static void Crc32w(AILEmitterCtx Context)
        {
            EmitCrc32(Context, nameof(ASoftFallback.Crc32w));
        }

        public static void Crc32x(AILEmitterCtx Context)
        {
            EmitCrc32(Context, nameof(ASoftFallback.Crc32x));
        }

        public static void Crc32cb(AILEmitterCtx Context)
        {
            if (AOptimizations.UseSse42)
            {
                EmitSse42Crc32(Context, typeof(uint), typeof(byte));
            }
            else
            {
                EmitCrc32(Context, nameof(ASoftFallback.Crc32cb));
            }
        }

        public static void Crc32ch(AILEmitterCtx Context)
        {
            if (AOptimizations.UseSse42)
            {
                EmitSse42Crc32(Context, typeof(uint), typeof(ushort));
            }
            else
            {
                EmitCrc32(Context, nameof(ASoftFallback.Crc32ch));
            }
        }

        public static void Crc32cw(AILEmitterCtx Context)
        {
            if (AOptimizations.UseSse42)
            {
                EmitSse42Crc32(Context, typeof(uint), typeof(uint));
            }
            else
            {
                EmitCrc32(Context, nameof(ASoftFallback.Crc32cw));
            }
        }

        public static void Crc32cx(AILEmitterCtx Context)
        {
            if (AOptimizations.UseSse42)
            {
                EmitSse42Crc32(Context, typeof(ulong), typeof(ulong));
            }
            else
            {
                EmitCrc32(Context, nameof(ASoftFallback.Crc32cx));
            }
        }

        private static void EmitSse42Crc32(AILEmitterCtx Context, Type TCrc, Type TData)
        {
            AOpCodeAluRs Op = (AOpCodeAluRs)Context.CurrOp;

            Context.EmitLdintzr(Op.Rn);
            Context.EmitLdintzr(Op.Rm);

            Context.EmitCall(typeof(Sse42).GetMethod(nameof(Sse42.Crc32), new Type[] { TCrc, TData }));

            Context.EmitStintzr(Op.Rd);
        }

        private static void EmitCrc32(AILEmitterCtx Context, string Name)
        {
            AOpCodeAluRs Op = (AOpCodeAluRs)Context.CurrOp;

            Context.EmitLdintzr(Op.Rn);

            if (Op.RegisterSize != ARegisterSize.Int32)
            {
                Context.Emit(OpCodes.Conv_U4);
            }

            Context.EmitLdintzr(Op.Rm);

            ASoftFallback.EmitCall(Context, Name);

            if (Op.RegisterSize != ARegisterSize.Int32)
            {
                Context.Emit(OpCodes.Conv_U8);
            }

            Context.EmitStintzr(Op.Rd);
        }
    }
}
