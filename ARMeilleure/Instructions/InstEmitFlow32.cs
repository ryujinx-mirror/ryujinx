using ARMeilleure.Decoders;
using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.State;
using ARMeilleure.Translation;

using static ARMeilleure.Instructions.InstEmitFlowHelper;
using static ARMeilleure.Instructions.InstEmitHelper;
using static ARMeilleure.IntermediateRepresentation.Operand.Factory;

namespace ARMeilleure.Instructions
{
    static partial class InstEmit32
    {
        public static void B(ArmEmitterContext context)
        {
            IOpCode32BImm op = (IOpCode32BImm)context.CurrOp;

            context.Branch(context.GetLabel((ulong)op.Immediate));
        }

        public static void Bl(ArmEmitterContext context)
        {
            Blx(context, x: false);
        }

        public static void Blx(ArmEmitterContext context)
        {
            Blx(context, x: true);
        }

        private static void Blx(ArmEmitterContext context, bool x)
        {
            IOpCode32BImm op = (IOpCode32BImm)context.CurrOp;

            uint pc = op.GetPc();

            bool isThumb = ((OpCode32)context.CurrOp).IsThumb();

            uint currentPc = isThumb
                ? pc | 1
                : pc - 4;

            SetIntA32(context, GetBankedRegisterAlias(context.Mode, RegisterAlias.Aarch32Lr), Const(currentPc));

            // If x is true, then this is a branch with link and exchange.
            // In this case we need to swap the mode between Arm <-> Thumb.
            if (x)
            {
                SetFlag(context, PState.TFlag, Const(isThumb ? 0 : 1));
            }

            EmitCall(context, (ulong)op.Immediate);
        }

        public static void Blxr(ArmEmitterContext context)
        {
            IOpCode32BReg op = (IOpCode32BReg)context.CurrOp;

            uint pc = op.GetPc();

            Operand addr = context.Copy(GetIntA32(context, op.Rm));
            Operand bitOne = context.BitwiseAnd(addr, Const(1));

            bool isThumb = ((OpCode32)context.CurrOp).IsThumb();

            uint currentPc = isThumb
                ? (pc - 2) | 1
                : pc - 4;

            SetIntA32(context, GetBankedRegisterAlias(context.Mode, RegisterAlias.Aarch32Lr), Const(currentPc));

            SetFlag(context, PState.TFlag, bitOne);

            EmitBxWritePc(context, addr);
        }

        public static void Bx(ArmEmitterContext context)
        {
            IOpCode32BReg op = (IOpCode32BReg)context.CurrOp;

            EmitBxWritePc(context, GetIntA32(context, op.Rm), op.Rm);
        }

        public static void Cbnz(ArmEmitterContext context) => EmitCb(context, onNotZero: true);
        public static void Cbz(ArmEmitterContext context)  => EmitCb(context, onNotZero: false);

        private static void EmitCb(ArmEmitterContext context, bool onNotZero)
        {
            OpCodeT16BImmCmp op = (OpCodeT16BImmCmp)context.CurrOp;

            Operand value = GetIntOrZR(context, op.Rn);
            Operand lblTarget = context.GetLabel((ulong)op.Immediate);

            if (onNotZero)
            {
                context.BranchIfTrue(lblTarget, value);
            }
            else
            {
                context.BranchIfFalse(lblTarget, value);
            }
        }

        public static void It(ArmEmitterContext context)
        {
            OpCodeT16IfThen op = (OpCodeT16IfThen)context.CurrOp;

            context.SetIfThenBlockState(op.IfThenBlockConds);
        }
    }
}