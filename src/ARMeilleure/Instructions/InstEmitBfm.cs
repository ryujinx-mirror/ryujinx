using ARMeilleure.Decoders;
using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.Translation;

using static ARMeilleure.Instructions.InstEmitHelper;
using static ARMeilleure.IntermediateRepresentation.Operand.Factory;

namespace ARMeilleure.Instructions
{
    static partial class InstEmit
    {
        public static void Bfm(ArmEmitterContext context)
        {
            OpCodeBfm op = (OpCodeBfm)context.CurrOp;

            Operand d = GetIntOrZR(context, op.Rd);
            Operand n = GetIntOrZR(context, op.Rn);

            Operand res;

            if (op.Pos < op.Shift)
            {
                // BFI.
                int shift = op.GetBitsCount() - op.Shift;

                int width = op.Pos + 1;

                long mask = (long)(ulong.MaxValue >> (64 - width));

                res = context.ShiftLeft(context.BitwiseAnd(n, Const(n.Type, mask)), Const(shift));

                res = context.BitwiseOr(res, context.BitwiseAnd(d, Const(d.Type, ~(mask << shift))));
            }
            else
            {
                // BFXIL.
                int shift = op.Shift;

                int width = op.Pos - shift + 1;

                long mask = (long)(ulong.MaxValue >> (64 - width));

                res = context.BitwiseAnd(context.ShiftRightUI(n, Const(shift)), Const(n.Type, mask));

                res = context.BitwiseOr(res, context.BitwiseAnd(d, Const(d.Type, ~mask)));
            }

            SetIntOrZR(context, op.Rd, res);
        }

        public static void Sbfm(ArmEmitterContext context)
        {
            OpCodeBfm op = (OpCodeBfm)context.CurrOp;

            int bitsCount = op.GetBitsCount();

            if (op.Pos + 1 == bitsCount)
            {
                EmitSbfmShift(context);
            }
            else if (op.Pos < op.Shift)
            {
                EmitSbfiz(context);
            }
            else if (op.Pos == 7 && op.Shift == 0)
            {
                Operand n = GetIntOrZR(context, op.Rn);

                SetIntOrZR(context, op.Rd, context.SignExtend8(n.Type, n));
            }
            else if (op.Pos == 15 && op.Shift == 0)
            {
                Operand n = GetIntOrZR(context, op.Rn);

                SetIntOrZR(context, op.Rd, context.SignExtend16(n.Type, n));
            }
            else if (op.Pos == 31 && op.Shift == 0)
            {
                Operand n = GetIntOrZR(context, op.Rn);

                SetIntOrZR(context, op.Rd, context.SignExtend32(n.Type, n));
            }
            else
            {
                Operand res = GetIntOrZR(context, op.Rn);

                res = context.ShiftLeft(res, Const(bitsCount - 1 - op.Pos));
                res = context.ShiftRightSI(res, Const(bitsCount - 1));
                res = context.BitwiseAnd(res, Const(res.Type, ~op.TMask));

                Operand n2 = GetBfmN(context);

                SetIntOrZR(context, op.Rd, context.BitwiseOr(res, n2));
            }
        }

        public static void Ubfm(ArmEmitterContext context)
        {
            OpCodeBfm op = (OpCodeBfm)context.CurrOp;

            if (op.Pos + 1 == op.GetBitsCount())
            {
                EmitUbfmShift(context);
            }
            else if (op.Pos < op.Shift)
            {
                EmitUbfiz(context);
            }
            else if (op.Pos + 1 == op.Shift)
            {
                EmitBfmLsl(context);
            }
            else if (op.Pos == 7 && op.Shift == 0)
            {
                Operand n = GetIntOrZR(context, op.Rn);

                SetIntOrZR(context, op.Rd, context.BitwiseAnd(n, Const(n.Type, 0xff)));
            }
            else if (op.Pos == 15 && op.Shift == 0)
            {
                Operand n = GetIntOrZR(context, op.Rn);

                SetIntOrZR(context, op.Rd, context.BitwiseAnd(n, Const(n.Type, 0xffff)));
            }
            else
            {
                SetIntOrZR(context, op.Rd, GetBfmN(context));
            }
        }

        private static void EmitSbfiz(ArmEmitterContext context) => EmitBfiz(context, signed: true);
        private static void EmitUbfiz(ArmEmitterContext context) => EmitBfiz(context, signed: false);

        private static void EmitBfiz(ArmEmitterContext context, bool signed)
        {
            OpCodeBfm op = (OpCodeBfm)context.CurrOp;

            int width = op.Pos + 1;

            Operand res = GetIntOrZR(context, op.Rn);

            res = context.ShiftLeft(res, Const(op.GetBitsCount() - width));

            res = signed
                ? context.ShiftRightSI(res, Const(op.Shift - width))
                : context.ShiftRightUI(res, Const(op.Shift - width));

            SetIntOrZR(context, op.Rd, res);
        }

        private static void EmitSbfmShift(ArmEmitterContext context)
        {
            EmitBfmShift(context, signed: true);
        }

        private static void EmitUbfmShift(ArmEmitterContext context)
        {
            EmitBfmShift(context, signed: false);
        }

        private static void EmitBfmShift(ArmEmitterContext context, bool signed)
        {
            OpCodeBfm op = (OpCodeBfm)context.CurrOp;

            Operand res = GetIntOrZR(context, op.Rn);

            res = signed
                ? context.ShiftRightSI(res, Const(op.Shift))
                : context.ShiftRightUI(res, Const(op.Shift));

            SetIntOrZR(context, op.Rd, res);
        }

        private static void EmitBfmLsl(ArmEmitterContext context)
        {
            OpCodeBfm op = (OpCodeBfm)context.CurrOp;

            Operand res = GetIntOrZR(context, op.Rn);

            int shift = op.GetBitsCount() - op.Shift;

            SetIntOrZR(context, op.Rd, context.ShiftLeft(res, Const(shift)));
        }

        private static Operand GetBfmN(ArmEmitterContext context)
        {
            OpCodeBfm op = (OpCodeBfm)context.CurrOp;

            Operand res = GetIntOrZR(context, op.Rn);

            long mask = op.WMask & op.TMask;

            return context.BitwiseAnd(context.RotateRight(res, Const(op.Shift)), Const(res.Type, mask));
        }
    }
}
