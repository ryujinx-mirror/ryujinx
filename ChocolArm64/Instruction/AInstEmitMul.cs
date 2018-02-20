using ChocolArm64.Decoder;
using ChocolArm64.Translation;
using System.Reflection.Emit;

namespace ChocolArm64.Instruction
{
    static partial class AInstEmit
    {
        public static void Madd(AILEmitterCtx Context) => EmitMul(Context, OpCodes.Add);
        public static void Msub(AILEmitterCtx Context) => EmitMul(Context, OpCodes.Sub);

        private static void EmitMul(AILEmitterCtx Context, OpCode ILOp)
        {
            AOpCodeMul Op = (AOpCodeMul)Context.CurrOp;

            Context.EmitLdintzr(Op.Ra);
            Context.EmitLdintzr(Op.Rn);
            Context.EmitLdintzr(Op.Rm);

            Context.Emit(OpCodes.Mul);
            Context.Emit(ILOp);

            Context.EmitStintzr(Op.Rd);
        }

        public static void Smaddl(AILEmitterCtx Context) => EmitMull(Context, OpCodes.Add, true);
        public static void Smsubl(AILEmitterCtx Context) => EmitMull(Context, OpCodes.Sub, true);
        public static void Umaddl(AILEmitterCtx Context) => EmitMull(Context, OpCodes.Add, false);
        public static void Umsubl(AILEmitterCtx Context) => EmitMull(Context, OpCodes.Sub, false);

        private static void EmitMull(AILEmitterCtx Context, OpCode AddSubOp, bool Signed)
        {
            AOpCodeMul Op = (AOpCodeMul)Context.CurrOp;

            OpCode CastOp = Signed
                ? OpCodes.Conv_I8
                : OpCodes.Conv_U8;

            Context.EmitLdintzr(Op.Ra);
            Context.EmitLdintzr(Op.Rn);

            Context.Emit(OpCodes.Conv_I4);
            Context.Emit(CastOp);

            Context.EmitLdintzr(Op.Rm);

            Context.Emit(OpCodes.Conv_I4);
            Context.Emit(CastOp);
            Context.Emit(OpCodes.Mul);

            Context.Emit(AddSubOp);

            Context.EmitStintzr(Op.Rd);
        }

        public static void Smulh(AILEmitterCtx Context)
        {
            AOpCodeMul Op = (AOpCodeMul)Context.CurrOp;

            Context.EmitLdintzr(Op.Rn);
            Context.EmitLdintzr(Op.Rm);

            ASoftFallback.EmitCall(Context, nameof(ASoftFallback.SMulHi128));

            Context.EmitStintzr(Op.Rd);
        }

        public static void Umulh(AILEmitterCtx Context)
        {
            AOpCodeMul Op = (AOpCodeMul)Context.CurrOp;

            Context.EmitLdintzr(Op.Rn);
            Context.EmitLdintzr(Op.Rm);

            ASoftFallback.EmitCall(Context, nameof(ASoftFallback.UMulHi128));

            Context.EmitStintzr(Op.Rd);
        }
    }
}