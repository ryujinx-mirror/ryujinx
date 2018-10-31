using ChocolArm64.Decoders;
using ChocolArm64.Translation;
using System.Reflection.Emit;

namespace ChocolArm64.Instructions
{
    static partial class InstEmit
    {
        public static void Movk(ILEmitterCtx context)
        {
            OpCodeMov64 op = (OpCodeMov64)context.CurrOp;

            context.EmitLdintzr(op.Rd);
            context.EmitLdc_I(~(0xffffL << op.Pos));

            context.Emit(OpCodes.And);

            context.EmitLdc_I(op.Imm);

            context.Emit(OpCodes.Or);

            context.EmitStintzr(op.Rd);
        }

        public static void Movn(ILEmitterCtx context)
        {
            OpCodeMov64 op = (OpCodeMov64)context.CurrOp;

            context.EmitLdc_I(~op.Imm);
            context.EmitStintzr(op.Rd);
        }

        public static void Movz(ILEmitterCtx context)
        {
            OpCodeMov64 op = (OpCodeMov64)context.CurrOp;

            context.EmitLdc_I(op.Imm);
            context.EmitStintzr(op.Rd);
        }
    }
}