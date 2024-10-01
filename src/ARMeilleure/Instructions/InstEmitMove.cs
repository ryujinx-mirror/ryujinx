using ARMeilleure.Decoders;
using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.Translation;

using static ARMeilleure.Instructions.InstEmitHelper;
using static ARMeilleure.IntermediateRepresentation.Operand.Factory;

namespace ARMeilleure.Instructions
{
    static partial class InstEmit
    {
        public static void Movk(ArmEmitterContext context)
        {
            OpCodeMov op = (OpCodeMov)context.CurrOp;

            OperandType type = op.GetOperandType();

            Operand res = GetIntOrZR(context, op.Rd);

            res = context.BitwiseAnd(res, Const(type, ~(0xffffL << op.Bit)));

            res = context.BitwiseOr(res, Const(type, op.Immediate));

            SetIntOrZR(context, op.Rd, res);
        }

        public static void Movn(ArmEmitterContext context)
        {
            OpCodeMov op = (OpCodeMov)context.CurrOp;

            SetIntOrZR(context, op.Rd, Const(op.GetOperandType(), ~op.Immediate));
        }

        public static void Movz(ArmEmitterContext context)
        {
            OpCodeMov op = (OpCodeMov)context.CurrOp;

            SetIntOrZR(context, op.Rd, Const(op.GetOperandType(), op.Immediate));
        }
    }
}
