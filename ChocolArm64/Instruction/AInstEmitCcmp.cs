using ChocolArm64.Decoder;
using ChocolArm64.State;
using ChocolArm64.Translation;
using System;
using System.Reflection.Emit;

using static ChocolArm64.Instruction.AInstEmitAluHelper;

namespace ChocolArm64.Instruction
{
    static partial class AInstEmit
    {
        private enum CcmpOp
        {
            Cmp,
            Cmn
        }

        public static void Ccmn(AILEmitterCtx Context) => EmitCcmp(Context, CcmpOp.Cmn);
        public static void Ccmp(AILEmitterCtx Context) => EmitCcmp(Context, CcmpOp.Cmp);

        private static void EmitCcmp(AILEmitterCtx Context, CcmpOp CmpOp)
        {
            AOpCodeCcmp Op = (AOpCodeCcmp)Context.CurrOp;

            AILLabel LblTrue = new AILLabel();
            AILLabel LblEnd  = new AILLabel();

            Context.EmitCondBranch(LblTrue, Op.Cond);

            Context.EmitLdc_I4((Op.NZCV >> 0) & 1);

            Context.EmitStflg((int)APState.VBit);

            Context.EmitLdc_I4((Op.NZCV >> 1) & 1);

            Context.EmitStflg((int)APState.CBit);

            Context.EmitLdc_I4((Op.NZCV >> 2) & 1);

            Context.EmitStflg((int)APState.ZBit);

            Context.EmitLdc_I4((Op.NZCV >> 3) & 1);

            Context.EmitStflg((int)APState.NBit);

            Context.Emit(OpCodes.Br_S, LblEnd);

            Context.MarkLabel(LblTrue);

            EmitDataLoadOpers(Context);

            if (CmpOp == CcmpOp.Cmp)
            {
                Context.Emit(OpCodes.Sub);

                Context.EmitZNFlagCheck();

                EmitSubsCCheck(Context);
                EmitSubsVCheck(Context);
            }
            else if (CmpOp == CcmpOp.Cmn)
            {
                Context.Emit(OpCodes.Add);

                Context.EmitZNFlagCheck();

                EmitAddsCCheck(Context);
                EmitAddsVCheck(Context);
            }
            else
            {
                throw new ArgumentException(nameof(CmpOp));
            }

            Context.Emit(OpCodes.Pop);

            Context.MarkLabel(LblEnd);
        }
    }
}