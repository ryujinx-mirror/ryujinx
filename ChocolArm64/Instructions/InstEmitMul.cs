using ChocolArm64.Decoders;
using ChocolArm64.Translation;
using System.Reflection.Emit;

namespace ChocolArm64.Instructions
{
    static partial class InstEmit
    {
        public static void Madd(ILEmitterCtx context) => EmitMul(context, OpCodes.Add);
        public static void Msub(ILEmitterCtx context) => EmitMul(context, OpCodes.Sub);

        private static void EmitMul(ILEmitterCtx context, OpCode ilOp)
        {
            OpCodeMul64 op = (OpCodeMul64)context.CurrOp;

            context.EmitLdintzr(op.Ra);
            context.EmitLdintzr(op.Rn);
            context.EmitLdintzr(op.Rm);

            context.Emit(OpCodes.Mul);
            context.Emit(ilOp);

            context.EmitStintzr(op.Rd);
        }

        public static void Smaddl(ILEmitterCtx context) => EmitMull(context, OpCodes.Add, true);
        public static void Smsubl(ILEmitterCtx context) => EmitMull(context, OpCodes.Sub, true);
        public static void Umaddl(ILEmitterCtx context) => EmitMull(context, OpCodes.Add, false);
        public static void Umsubl(ILEmitterCtx context) => EmitMull(context, OpCodes.Sub, false);

        private static void EmitMull(ILEmitterCtx context, OpCode addSubOp, bool signed)
        {
            OpCodeMul64 op = (OpCodeMul64)context.CurrOp;

            OpCode castOp = signed
                ? OpCodes.Conv_I8
                : OpCodes.Conv_U8;

            context.EmitLdintzr(op.Ra);
            context.EmitLdintzr(op.Rn);

            context.Emit(OpCodes.Conv_I4);
            context.Emit(castOp);

            context.EmitLdintzr(op.Rm);

            context.Emit(OpCodes.Conv_I4);
            context.Emit(castOp);
            context.Emit(OpCodes.Mul);

            context.Emit(addSubOp);

            context.EmitStintzr(op.Rd);
        }

        public static void Smulh(ILEmitterCtx context)
        {
            OpCodeMul64 op = (OpCodeMul64)context.CurrOp;

            context.EmitLdintzr(op.Rn);
            context.EmitLdintzr(op.Rm);

            SoftFallback.EmitCall(context, nameof(SoftFallback.SMulHi128));

            context.EmitStintzr(op.Rd);
        }

        public static void Umulh(ILEmitterCtx context)
        {
            OpCodeMul64 op = (OpCodeMul64)context.CurrOp;

            context.EmitLdintzr(op.Rn);
            context.EmitLdintzr(op.Rm);

            SoftFallback.EmitCall(context, nameof(SoftFallback.UMulHi128));

            context.EmitStintzr(op.Rd);
        }
    }
}