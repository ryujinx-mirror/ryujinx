using ChocolArm64.Decoder;
using ChocolArm64.Translation;
using System.Reflection.Emit;

namespace ChocolArm64.Instruction
{
    static partial class AInstEmit
    {
        public static void Movk(AILEmitterCtx Context)
        {
            AOpCodeMov Op = (AOpCodeMov)Context.CurrOp;

            Context.EmitLdintzr(Op.Rd);
            Context.EmitLdc_I(~(0xffffL << Op.Pos));

            Context.Emit(OpCodes.And);

            Context.EmitLdc_I(Op.Imm);

            Context.Emit(OpCodes.Or);

            Context.EmitStintzr(Op.Rd);
        }

        public static void Movn(AILEmitterCtx Context)
        {
            AOpCodeMov Op = (AOpCodeMov)Context.CurrOp;

            Context.EmitLdc_I(~Op.Imm);
            Context.EmitStintzr(Op.Rd);
        }

        public static void Movz(AILEmitterCtx Context)
        {
            AOpCodeMov Op = (AOpCodeMov)Context.CurrOp;

            Context.EmitLdc_I(Op.Imm);
            Context.EmitStintzr(Op.Rd);
        }
    }
}