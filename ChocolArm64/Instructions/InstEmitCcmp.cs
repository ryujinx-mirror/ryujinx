using ChocolArm64.Decoders;
using ChocolArm64.State;
using ChocolArm64.Translation;
using System;
using System.Reflection.Emit;

using static ChocolArm64.Instructions.InstEmitAluHelper;

namespace ChocolArm64.Instructions
{
    static partial class InstEmit
    {
        private enum CcmpOp
        {
            Cmp,
            Cmn
        }

        public static void Ccmn(ILEmitterCtx context) => EmitCcmp(context, CcmpOp.Cmn);
        public static void Ccmp(ILEmitterCtx context) => EmitCcmp(context, CcmpOp.Cmp);

        private static void EmitCcmp(ILEmitterCtx context, CcmpOp cmpOp)
        {
            OpCodeCcmp64 op = (OpCodeCcmp64)context.CurrOp;

            ILLabel lblTrue = new ILLabel();
            ILLabel lblEnd  = new ILLabel();

            context.EmitCondBranch(lblTrue, op.Cond);

            context.EmitLdc_I4((op.Nzcv >> 0) & 1);

            context.EmitStflg((int)PState.VBit);

            context.EmitLdc_I4((op.Nzcv >> 1) & 1);

            context.EmitStflg((int)PState.CBit);

            context.EmitLdc_I4((op.Nzcv >> 2) & 1);

            context.EmitStflg((int)PState.ZBit);

            context.EmitLdc_I4((op.Nzcv >> 3) & 1);

            context.EmitStflg((int)PState.NBit);

            context.Emit(OpCodes.Br_S, lblEnd);

            context.MarkLabel(lblTrue);

            EmitDataLoadOpers(context);

            if (cmpOp == CcmpOp.Cmp)
            {
                context.Emit(OpCodes.Sub);

                context.EmitZnFlagCheck();

                EmitSubsCCheck(context);
                EmitSubsVCheck(context);
            }
            else if (cmpOp == CcmpOp.Cmn)
            {
                context.Emit(OpCodes.Add);

                context.EmitZnFlagCheck();

                EmitAddsCCheck(context);
                EmitAddsVCheck(context);
            }
            else
            {
                throw new ArgumentException(nameof(cmpOp));
            }

            context.Emit(OpCodes.Pop);

            context.MarkLabel(lblEnd);
        }
    }
}