using ARMeilleure.Decoders;
using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.State;
using ARMeilleure.Translation;

using static ARMeilleure.Instructions.InstEmitHelper;
using static ARMeilleure.IntermediateRepresentation.OperandHelper;

namespace ARMeilleure.Instructions
{
    static class InstEmitFlowHelper
    {
        public const ulong CallFlag = 1;

        public static void EmitCondBranch(ArmEmitterContext context, Operand target, Condition cond)
        {
            if (cond != Condition.Al)
            {
                context.BranchIfTrue(target, GetCondTrue(context, cond));
            }
            else
            {
                context.Branch(target);
            }
        }

        public static Operand GetCondTrue(ArmEmitterContext context, Condition condition)
        {
            Operand cmpResult = context.TryGetComparisonResult(condition);

            if (cmpResult != null)
            {
                return cmpResult;
            }

            Operand value = Const(1);

            Operand Inverse(Operand val)
            {
                return context.BitwiseExclusiveOr(val, Const(1));
            }

            switch (condition)
            {
                case Condition.Eq:
                    value = GetFlag(PState.ZFlag);
                    break;

                case Condition.Ne:
                    value = Inverse(GetFlag(PState.ZFlag));
                    break;

                case Condition.GeUn:
                    value = GetFlag(PState.CFlag);
                    break;

                case Condition.LtUn:
                    value = Inverse(GetFlag(PState.CFlag));
                    break;

                case Condition.Mi:
                    value = GetFlag(PState.NFlag);
                    break;

                case Condition.Pl:
                    value = Inverse(GetFlag(PState.NFlag));
                    break;

                case Condition.Vs:
                    value = GetFlag(PState.VFlag);
                    break;

                case Condition.Vc:
                    value = Inverse(GetFlag(PState.VFlag));
                    break;

                case Condition.GtUn:
                {
                    Operand c = GetFlag(PState.CFlag);
                    Operand z = GetFlag(PState.ZFlag);

                    value = context.BitwiseAnd(c, Inverse(z));

                    break;
                }

                case Condition.LeUn:
                {
                    Operand c = GetFlag(PState.CFlag);
                    Operand z = GetFlag(PState.ZFlag);

                    value = context.BitwiseOr(Inverse(c), z);

                    break;
                }

                case Condition.Ge:
                {
                    Operand n = GetFlag(PState.NFlag);
                    Operand v = GetFlag(PState.VFlag);

                    value = context.ICompareEqual(n, v);

                    break;
                }

                case Condition.Lt:
                {
                    Operand n = GetFlag(PState.NFlag);
                    Operand v = GetFlag(PState.VFlag);

                    value = context.ICompareNotEqual(n, v);

                    break;
                }

                case Condition.Gt:
                {
                    Operand n = GetFlag(PState.NFlag);
                    Operand z = GetFlag(PState.ZFlag);
                    Operand v = GetFlag(PState.VFlag);

                    value = context.BitwiseAnd(Inverse(z), context.ICompareEqual(n, v));

                    break;
                }

                case Condition.Le:
                {
                    Operand n = GetFlag(PState.NFlag);
                    Operand z = GetFlag(PState.ZFlag);
                    Operand v = GetFlag(PState.VFlag);

                    value = context.BitwiseOr(z, context.ICompareNotEqual(n, v));

                    break;
                }
            }

            return value;
        }

        public static void EmitCall(ArmEmitterContext context, ulong immediate)
        {
            context.Return(Const(immediate | CallFlag));
        }

        public static void EmitVirtualCall(ArmEmitterContext context, Operand target)
        {
            EmitVirtualCallOrJump(context, target, isJump: false);
        }

        public static void EmitVirtualJump(ArmEmitterContext context, Operand target)
        {
            EmitVirtualCallOrJump(context, target, isJump: true);
        }

        private static void EmitVirtualCallOrJump(ArmEmitterContext context, Operand target, bool isJump)
        {
            context.Return(context.BitwiseOr(target, Const(target.Type, (long)CallFlag)));
        }

        private static void EmitContinueOrReturnCheck(ArmEmitterContext context, Operand retVal)
        {
            // Note: The return value of the called method will be placed
            // at the Stack, the return value is always a Int64 with the
            // return address of the function. We check if the address is
            // correct, if it isn't we keep returning until we reach the dispatcher.
            ulong nextAddr = GetNextOpAddress(context.CurrOp);

            if (context.CurrBlock.Next != null)
            {
                Operand lblContinue = Label();

                context.BranchIfTrue(lblContinue, context.ICompareEqual(retVal, Const(nextAddr)));

                context.Return(Const(nextAddr));

                context.MarkLabel(lblContinue);
            }
            else
            {
                context.Return(Const(nextAddr));
            }
        }

        private static ulong GetNextOpAddress(OpCode op)
        {
            return op.Address + (ulong)op.OpCodeSizeInBytes;
        }
    }
}
