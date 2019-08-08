using ARMeilleure.Decoders;
using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.State;
using ARMeilleure.Translation;

using static ARMeilleure.Instructions.InstEmitHelper;
using static ARMeilleure.Instructions.InstEmitAluHelper;
using static ARMeilleure.IntermediateRepresentation.OperandHelper;

namespace ARMeilleure.Instructions
{
    static partial class InstEmit32
    {
        public static void Add(ArmEmitterContext context)
        {
            IOpCode32Alu op = (IOpCode32Alu)context.CurrOp;

            Operand n = GetAluN(context);
            Operand m = GetAluM(context, setCarry: false);

            Operand res = context.Add(n, m);

            if (op.SetFlags)
            {
                EmitNZFlagsCheck(context, res);

                EmitAddsCCheck(context, n, res);
                EmitAddsVCheck(context, n, m, res);
            }

            EmitAluStore(context, res);
        }

        public static void Cmp(ArmEmitterContext context)
        {
            IOpCode32Alu op = (IOpCode32Alu)context.CurrOp;

            Operand n = GetAluN(context);
            Operand m = GetAluM(context, setCarry: false);

            Operand res = context.Subtract(n, m);

            EmitNZFlagsCheck(context, res);

            EmitSubsCCheck(context, n, res);
            EmitSubsVCheck(context, n, m, res);
        }

        public static void Mov(ArmEmitterContext context)
        {
            IOpCode32Alu op = (IOpCode32Alu)context.CurrOp;

            Operand m = GetAluM(context);

            if (op.SetFlags)
            {
                EmitNZFlagsCheck(context, m);
            }

            EmitAluStore(context, m);
        }

        public static void Sub(ArmEmitterContext context)
        {
            IOpCode32Alu op = (IOpCode32Alu)context.CurrOp;

            Operand n = GetAluN(context);
            Operand m = GetAluM(context, setCarry: false);

            Operand res = context.Subtract(n, m);

            if (op.SetFlags)
            {
                EmitNZFlagsCheck(context, res);

                EmitSubsCCheck(context, n, res);
                EmitSubsVCheck(context, n, m, res);
            }

            EmitAluStore(context, res);
        }

        private static void EmitAluStore(ArmEmitterContext context, Operand value)
        {
            IOpCode32Alu op = (IOpCode32Alu)context.CurrOp;

            if (op.Rd == RegisterAlias.Aarch32Pc)
            {
                if (op.SetFlags)
                {
                    // TODO: Load SPSR etc.
                    Operand isThumb = GetFlag(PState.TFlag);

                    Operand lblThumb = Label();

                    context.BranchIfTrue(lblThumb, isThumb);

                    context.Return(context.ZeroExtend32(OperandType.I64, context.BitwiseAnd(value, Const(~3))));

                    context.MarkLabel(lblThumb);

                    context.Return(context.ZeroExtend32(OperandType.I64, context.BitwiseAnd(value, Const(~1))));
                }
                else
                {
                    EmitAluWritePc(context, value);
                }
            }
            else
            {
                SetIntA32(context, op.Rd, value);
            }
        }

        private static void EmitAluWritePc(ArmEmitterContext context, Operand value)
        {
            context.StoreToContext();

            if (IsThumb(context.CurrOp))
            {
                context.Return(context.ZeroExtend32(OperandType.I64, context.BitwiseAnd(value, Const(~1))));
            }
            else
            {
                EmitBxWritePc(context, value);
            }
        }
    }
}