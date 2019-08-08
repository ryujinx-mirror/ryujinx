using ARMeilleure.Decoders;
using ARMeilleure.State;
using ARMeilleure.Translation;

using static ARMeilleure.Instructions.InstEmitHelper;
using static ARMeilleure.IntermediateRepresentation.OperandHelper;

namespace ARMeilleure.Instructions
{
    static partial class InstEmit32
    {
        public static void B(ArmEmitterContext context)
        {
            IOpCode32BImm op = (IOpCode32BImm)context.CurrOp;

            if (context.CurrBlock.Branch != null)
            {
                context.Branch(context.GetLabel((ulong)op.Immediate));
            }
            else
            {
                context.StoreToContext();

                context.Return(Const(op.Immediate));
            }
        }

        public static void Bl(ArmEmitterContext context)
        {
            Blx(context, x: false);
        }

        public static void Blx(ArmEmitterContext context)
        {
            Blx(context, x: true);
        }

        public static void Bx(ArmEmitterContext context)
        {
            IOpCode32BReg op = (IOpCode32BReg)context.CurrOp;

            context.StoreToContext();

            EmitBxWritePc(context, GetIntA32(context, op.Rm));
        }

        private static void Blx(ArmEmitterContext context, bool x)
        {
            IOpCode32BImm op = (IOpCode32BImm)context.CurrOp;

            uint pc = op.GetPc();

            bool isThumb = IsThumb(context.CurrOp);

            uint currentPc = isThumb
                ? op.GetPc() | 1
                : op.GetPc() - 4;

            SetIntOrSP(context, GetBankedRegisterAlias(context.Mode, RegisterAlias.Aarch32Lr), Const(currentPc));

            // If x is true, then this is a branch with link and exchange.
            // In this case we need to swap the mode between Arm <-> Thumb.
            if (x)
            {
                SetFlag(context, PState.TFlag, Const(isThumb ? 0 : 1));
            }

            InstEmitFlowHelper.EmitCall(context, (ulong)op.Immediate);
        }
    }
}