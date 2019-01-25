using ChocolArm64.Decoders;
using ChocolArm64.State;
using ChocolArm64.Translation;
using System.Reflection.Emit;

namespace ChocolArm64.Instructions
{
    static partial class InstEmit
    {
        public static void B(ILEmitterCtx context)
        {
            OpCodeBImmAl64 op = (OpCodeBImmAl64)context.CurrOp;

            if (context.CurrBlock.Branch != null)
            {
                context.Emit(OpCodes.Br, context.GetLabel(op.Imm));
            }
            else
            {
                context.EmitStoreState();
                context.EmitLdc_I8(op.Imm);

                context.Emit(OpCodes.Ret);
            }
        }

        public static void B_Cond(ILEmitterCtx context)
        {
            OpCodeBImmCond64 op = (OpCodeBImmCond64)context.CurrOp;

            EmitBranch(context, op.Cond);
        }

        public static void Bl(ILEmitterCtx context)
        {
            OpCodeBImmAl64 op = (OpCodeBImmAl64)context.CurrOp;

            context.EmitLdc_I(op.Position + 4);
            context.EmitStint(RegisterAlias.Lr);
            context.EmitStoreState();

            InstEmitFlowHelper.EmitCall(context, op.Imm);
        }

        public static void Blr(ILEmitterCtx context)
        {
            OpCodeBReg64 op = (OpCodeBReg64)context.CurrOp;

            context.EmitLdintzr(op.Rn);
            context.EmitLdc_I(op.Position + 4);
            context.EmitStint(RegisterAlias.Lr);
            context.EmitStoreState();

            context.Emit(OpCodes.Ret);
        }

        public static void Br(ILEmitterCtx context)
        {
            OpCodeBReg64 op = (OpCodeBReg64)context.CurrOp;

            context.EmitStoreState();
            context.EmitLdintzr(op.Rn);

            context.Emit(OpCodes.Ret);
        }

        public static void Cbnz(ILEmitterCtx context) => EmitCb(context, OpCodes.Bne_Un);
        public static void Cbz(ILEmitterCtx context)  => EmitCb(context, OpCodes.Beq);

        private static void EmitCb(ILEmitterCtx context, OpCode ilOp)
        {
            OpCodeBImmCmp64 op = (OpCodeBImmCmp64)context.CurrOp;

            context.EmitLdintzr(op.Rt);
            context.EmitLdc_I(0);

            EmitBranch(context, ilOp);
        }

        public static void Ret(ILEmitterCtx context)
        {
            context.EmitStoreState();
            context.EmitLdint(RegisterAlias.Lr);

            context.Emit(OpCodes.Ret);
        }

        public static void Tbnz(ILEmitterCtx context) => EmitTb(context, OpCodes.Bne_Un);
        public static void Tbz(ILEmitterCtx context)  => EmitTb(context, OpCodes.Beq);

        private static void EmitTb(ILEmitterCtx context, OpCode ilOp)
        {
            OpCodeBImmTest64 op = (OpCodeBImmTest64)context.CurrOp;

            context.EmitLdintzr(op.Rt);
            context.EmitLdc_I(1L << op.Pos);

            context.Emit(OpCodes.And);

            context.EmitLdc_I(0);

            EmitBranch(context, ilOp);
        }

        private static void EmitBranch(ILEmitterCtx context, Condition cond)
        {
            OpCodeBImm64 op = (OpCodeBImm64)context.CurrOp;

            if (context.CurrBlock.Next   != null &&
                context.CurrBlock.Branch != null)
            {
                context.EmitCondBranch(context.GetLabel(op.Imm), cond);
            }
            else
            {
                context.EmitStoreState();

                ILLabel lblTaken = new ILLabel();

                context.EmitCondBranch(lblTaken, cond);

                context.EmitLdc_I8(op.Position + 4);

                context.Emit(OpCodes.Ret);

                context.MarkLabel(lblTaken);

                context.EmitLdc_I8(op.Imm);

                context.Emit(OpCodes.Ret);
            }
        }

        private static void EmitBranch(ILEmitterCtx context, OpCode ilOp)
        {
            OpCodeBImm64 op = (OpCodeBImm64)context.CurrOp;

            if (context.CurrBlock.Next   != null &&
                context.CurrBlock.Branch != null)
            {
                context.Emit(ilOp, context.GetLabel(op.Imm));
            }
            else
            {
                context.EmitStoreState();

                ILLabel lblTaken = new ILLabel();

                context.Emit(ilOp, lblTaken);

                context.EmitLdc_I8(op.Position + 4);

                context.Emit(OpCodes.Ret);

                context.MarkLabel(lblTaken);

                context.EmitLdc_I8(op.Imm);

                context.Emit(OpCodes.Ret);
            }
        }
    }
}