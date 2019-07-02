using ChocolArm64.Decoders;
using ChocolArm64.State;
using ChocolArm64.Translation;
using System.Reflection.Emit;

using static ChocolArm64.Instructions.InstEmit32Helper;

namespace ChocolArm64.Instructions
{
    static partial class InstEmit32
    {
        public static void B(ILEmitterCtx context)
        {
            IOpCode32BImm op = (IOpCode32BImm)context.CurrOp;

            if (context.CurrBlock.Branch != null)
            {
                context.Emit(OpCodes.Br, context.GetLabel(op.Imm));
            }
            else
            {
                context.EmitStoreContext();
                context.EmitLdc_I8(op.Imm);

                context.Emit(OpCodes.Ret);
            }
        }

        public static void Bl(ILEmitterCtx context)
        {
            Blx(context, x: false);
        }

        public static void Blx(ILEmitterCtx context)
        {
            Blx(context, x: true);
        }

        public static void Bx(ILEmitterCtx context)
        {
            IOpCode32BReg op = (IOpCode32BReg)context.CurrOp;

            context.EmitStoreContext();

            EmitLoadFromRegister(context, op.Rm);

            EmitBxWritePc(context);
        }

        private static void Blx(ILEmitterCtx context, bool x)
        {
            IOpCode32BImm op = (IOpCode32BImm)context.CurrOp;

            uint pc = op.GetPc();

            bool isThumb = IsThumb(context.CurrOp);

            if (!isThumb)
            {
                context.EmitLdc_I(op.GetPc() - 4);
            }
            else
            {
                context.EmitLdc_I(op.GetPc() | 1);
            }

            context.EmitStint(GetBankedRegisterAlias(context.Mode, RegisterAlias.Aarch32Lr));

            // If x is true, then this is a branch with link and exchange.
            // In this case we need to swap the mode between Arm <-> Thumb.
            if (x)
            {
                context.EmitLdc_I4(isThumb ? 0 : 1);

                context.EmitStflg((int)PState.TBit);
            }

            InstEmitFlowHelper.EmitCall(context, op.Imm);
        }
    }
}