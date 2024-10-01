using ARMeilleure.Decoders;
using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.Translation;
using System;
using System.Diagnostics.CodeAnalysis;
using static ARMeilleure.Instructions.InstEmitHelper;

namespace ARMeilleure.Instructions
{
    static partial class InstEmit
    {
        public static void Madd(ArmEmitterContext context) => EmitMul(context, isAdd: true);
        public static void Msub(ArmEmitterContext context) => EmitMul(context, isAdd: false);

        private static void EmitMul(ArmEmitterContext context, bool isAdd)
        {
            OpCodeMul op = (OpCodeMul)context.CurrOp;

            Operand a = GetIntOrZR(context, op.Ra);
            Operand n = GetIntOrZR(context, op.Rn);
            Operand m = GetIntOrZR(context, op.Rm);

            Operand res = context.Multiply(n, m);

            res = isAdd ? context.Add(a, res) : context.Subtract(a, res);

            SetIntOrZR(context, op.Rd, res);
        }

        public static void Smaddl(ArmEmitterContext context) => EmitMull(context, MullFlags.SignedAdd);
        public static void Smsubl(ArmEmitterContext context) => EmitMull(context, MullFlags.SignedSubtract);
        public static void Umaddl(ArmEmitterContext context) => EmitMull(context, MullFlags.Add);
        public static void Umsubl(ArmEmitterContext context) => EmitMull(context, MullFlags.Subtract);

        [Flags]
        [SuppressMessage("Design", "CA1069: Enums values should not be duplicated")]
        private enum MullFlags
        {
            Subtract = 0,
            Add = 1 << 0,
            Signed = 1 << 1,

            SignedAdd = Signed | Add,
            SignedSubtract = Signed | Subtract,
        }

        private static void EmitMull(ArmEmitterContext context, MullFlags flags)
        {
            OpCodeMul op = (OpCodeMul)context.CurrOp;

            Operand GetExtendedRegister32(int index)
            {
                Operand value = GetIntOrZR(context, index);

                if ((flags & MullFlags.Signed) != 0)
                {
                    return context.SignExtend32(value.Type, value);
                }
                else
                {
                    return context.ZeroExtend32(value.Type, value);
                }
            }

            Operand a = GetIntOrZR(context, op.Ra);

            Operand n = GetExtendedRegister32(op.Rn);
            Operand m = GetExtendedRegister32(op.Rm);

            Operand res = context.Multiply(n, m);

            res = (flags & MullFlags.Add) != 0 ? context.Add(a, res) : context.Subtract(a, res);

            SetIntOrZR(context, op.Rd, res);
        }

        public static void Smulh(ArmEmitterContext context)
        {
            OpCodeMul op = (OpCodeMul)context.CurrOp;

            Operand n = GetIntOrZR(context, op.Rn);
            Operand m = GetIntOrZR(context, op.Rm);

            Operand d = context.Multiply64HighSI(n, m);

            SetIntOrZR(context, op.Rd, d);
        }

        public static void Umulh(ArmEmitterContext context)
        {
            OpCodeMul op = (OpCodeMul)context.CurrOp;

            Operand n = GetIntOrZR(context, op.Rn);
            Operand m = GetIntOrZR(context, op.Rm);

            Operand d = context.Multiply64HighUI(n, m);

            SetIntOrZR(context, op.Rd, d);
        }
    }
}
