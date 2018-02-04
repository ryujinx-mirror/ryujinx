using ChocolArm64.Decoder;
using ChocolArm64.State;
using ChocolArm64.Translation;
using System.Reflection.Emit;

namespace ChocolArm64.Instruction
{
    static partial class AInstEmit
    {
        public static void B(AILEmitterCtx Context)
        {
            AOpCodeBImmAl Op = (AOpCodeBImmAl)Context.CurrOp;

            Context.Emit(OpCodes.Br, Context.GetLabel(Op.Imm));
        }

        public static void B_Cond(AILEmitterCtx Context)
        {
            AOpCodeBImmCond Op = (AOpCodeBImmCond)Context.CurrOp;

            Context.EmitCondBranch(Context.GetLabel(Op.Imm), Op.Cond);
        }

        public static void Bl(AILEmitterCtx Context)
        {
            AOpCodeBImmAl Op = (AOpCodeBImmAl)Context.CurrOp;

            Context.EmitLdc_I(Op.Position + 4);
            Context.EmitStint(ARegisters.LRIndex);
            Context.EmitStoreState();

            if (Context.TryOptEmitSubroutineCall())
            {
                //Note: the return value of the called method will be placed
                //at the Stack, the return value is always a Int64 with the
                //return address of the function. We check if the address is
                //correct, if it isn't we keep returning until we reach the dispatcher.
                Context.Emit(OpCodes.Dup);

                Context.EmitLdc_I8(Op.Position + 4);

                AILLabel LblContinue = new AILLabel();

                Context.Emit(OpCodes.Beq_S, LblContinue);
                Context.Emit(OpCodes.Ret);

                Context.MarkLabel(LblContinue);

                Context.Emit(OpCodes.Pop);

                if (Context.CurrBlock.Next != null)
                {
                    Context.EmitLoadState(Context.CurrBlock.Next);
                }
            }
            else
            {
                Context.EmitLdc_I8(Op.Imm);

                Context.Emit(OpCodes.Ret);
            }
        }

        public static void Blr(AILEmitterCtx Context)
        {
            AOpCodeBReg Op = (AOpCodeBReg)Context.CurrOp;

            Context.EmitLdc_I(Op.Position + 4);
            Context.EmitStint(ARegisters.LRIndex);
            Context.EmitStoreState();
            Context.EmitLdintzr(Op.Rn);

            Context.Emit(OpCodes.Ret);
        }

        public static void Br(AILEmitterCtx Context)
        {
            AOpCodeBReg Op = (AOpCodeBReg)Context.CurrOp;

            Context.EmitStoreState();
            Context.EmitLdintzr(Op.Rn);

            Context.Emit(OpCodes.Ret);
        }

        public static void Cbnz(AILEmitterCtx Context) => EmitCb(Context, OpCodes.Bne_Un);
        public static void Cbz(AILEmitterCtx Context)  => EmitCb(Context, OpCodes.Beq);

        private static void EmitCb(AILEmitterCtx Context, OpCode ILOp)
        {
            AOpCodeBImmCmp Op = (AOpCodeBImmCmp)Context.CurrOp;

            Context.EmitLdintzr(Op.Rt);
            Context.EmitLdc_I(0);

            Context.Emit(ILOp, Context.GetLabel(Op.Imm));
        }

        public static void Ret(AILEmitterCtx Context)
        {
            Context.EmitStoreState();
            Context.EmitLdint(ARegisters.LRIndex);

            Context.Emit(OpCodes.Ret);
        }

        public static void Tbnz(AILEmitterCtx Context) => EmitTb(Context, OpCodes.Bne_Un);
        public static void Tbz(AILEmitterCtx Context)  => EmitTb(Context, OpCodes.Beq);

        private static void EmitTb(AILEmitterCtx Context, OpCode ILOp)
        {
            AOpCodeBImmTest Op = (AOpCodeBImmTest)Context.CurrOp;

            Context.EmitLdintzr(Op.Rt);
            Context.EmitLdc_I(1L << Op.Pos);

            Context.Emit(OpCodes.And);

            Context.EmitLdc_I(0);

            Context.Emit(ILOp, Context.GetLabel(Op.Imm));
        }
    }
}