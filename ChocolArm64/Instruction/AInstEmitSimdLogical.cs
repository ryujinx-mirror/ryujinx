using ChocolArm64.Translation;
using System;
using System.Reflection.Emit;

using static ChocolArm64.Instruction.AInstEmitSimdHelper;

namespace ChocolArm64.Instruction
{
    static partial class AInstEmit
    {
        public static void And_V(AILEmitterCtx Context)
        {
            EmitVectorBinaryOpZx(Context, () => Context.Emit(OpCodes.And));
        }

        public static void Bic_V(AILEmitterCtx Context)
        {
            EmitVectorBinaryOpZx(Context, () =>
            {
                Context.Emit(OpCodes.Not);
                Context.Emit(OpCodes.And);
            });
        }

        public static void Bic_Vi(AILEmitterCtx Context)
        {
            EmitVectorImmBinaryOp(Context, () =>
            {
                Context.Emit(OpCodes.Not);
                Context.Emit(OpCodes.And);
            });
        }

        public static void Bsl_V(AILEmitterCtx Context)
        {
            EmitVectorTernaryOpZx(Context, () =>
            {
                Context.EmitSttmp();
                Context.EmitLdtmp();

                Context.Emit(OpCodes.Xor);
                Context.Emit(OpCodes.And);

                Context.EmitLdtmp();

                Context.Emit(OpCodes.Xor);
            });
        }

        public static void Eor_V(AILEmitterCtx Context)
        {
            EmitVectorBinaryOpZx(Context, () => Context.Emit(OpCodes.Xor));
        }

        public static void Not_V(AILEmitterCtx Context)
        {
            EmitVectorUnaryOpZx(Context, () => Context.Emit(OpCodes.Not));
        }

        public static void Orr_V(AILEmitterCtx Context)
        {
            EmitVectorBinaryOpZx(Context, () => Context.Emit(OpCodes.Or));
        }

        public static void Orr_Vi(AILEmitterCtx Context)
        {
            EmitVectorImmBinaryOp(Context, () => Context.Emit(OpCodes.Or));
        }

        public static void Rev64_V(AILEmitterCtx Context)
        {
            Action Emit = () =>
            {
                ASoftFallback.EmitCall(Context, nameof(ASoftFallback.ReverseBits64));
            };
            
            EmitVectorUnaryOpZx(Context, Emit);
        }
    }
}