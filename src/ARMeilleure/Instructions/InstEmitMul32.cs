using ARMeilleure.Decoders;
using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.State;
using ARMeilleure.Translation;
using System;
using static ARMeilleure.Instructions.InstEmitAluHelper;
using static ARMeilleure.Instructions.InstEmitHelper;
using static ARMeilleure.IntermediateRepresentation.Operand.Factory;

namespace ARMeilleure.Instructions
{
    static partial class InstEmit32
    {
        [Flags]
        private enum MullFlags
        {
            Subtract = 1,
            Add = 1 << 1,
            Signed = 1 << 2,

            SignedAdd = Signed | Add,
            SignedSubtract = Signed | Subtract,
        }

        public static void Mla(ArmEmitterContext context)
        {
            IOpCode32AluMla op = (IOpCode32AluMla)context.CurrOp;

            Operand n = GetAluN(context);
            Operand m = GetAluM(context);
            Operand a = GetIntA32(context, op.Ra);

            Operand res = context.Add(a, context.Multiply(n, m));

            if (ShouldSetFlags(context))
            {
                EmitNZFlagsCheck(context, res);
            }

            EmitAluStore(context, res);
        }

        public static void Mls(ArmEmitterContext context)
        {
            IOpCode32AluMla op = (IOpCode32AluMla)context.CurrOp;

            Operand n = GetAluN(context);
            Operand m = GetAluM(context);
            Operand a = GetIntA32(context, op.Ra);

            Operand res = context.Subtract(a, context.Multiply(n, m));

            EmitAluStore(context, res);
        }

        public static void Smmla(ArmEmitterContext context)
        {
            EmitSmmul(context, MullFlags.SignedAdd);
        }

        public static void Smmls(ArmEmitterContext context)
        {
            EmitSmmul(context, MullFlags.SignedSubtract);
        }

        public static void Smmul(ArmEmitterContext context)
        {
            EmitSmmul(context, MullFlags.Signed);
        }

        private static void EmitSmmul(ArmEmitterContext context, MullFlags flags)
        {
            IOpCode32AluMla op = (IOpCode32AluMla)context.CurrOp;

            Operand n = context.SignExtend32(OperandType.I64, GetIntA32(context, op.Rn));
            Operand m = context.SignExtend32(OperandType.I64, GetIntA32(context, op.Rm));

            Operand res = context.Multiply(n, m);

            if (flags.HasFlag(MullFlags.Add) && op.Ra != 0xf)
            {
                res = context.Add(context.ShiftLeft(context.ZeroExtend32(OperandType.I64, GetIntA32(context, op.Ra)), Const(32)), res);
            }
            else if (flags.HasFlag(MullFlags.Subtract))
            {
                res = context.Subtract(context.ShiftLeft(context.ZeroExtend32(OperandType.I64, GetIntA32(context, op.Ra)), Const(32)), res);
            }

            if (op.R)
            {
                res = context.Add(res, Const(0x80000000L));
            }

            Operand hi = context.ConvertI64ToI32(context.ShiftRightSI(res, Const(32)));

            EmitGenericAluStoreA32(context, op.Rd, false, hi);
        }

        public static void Smla__(ArmEmitterContext context)
        {
            IOpCode32AluMla op = (IOpCode32AluMla)context.CurrOp;

            Operand n = GetIntA32(context, op.Rn);
            Operand m = GetIntA32(context, op.Rm);
            Operand a = GetIntA32(context, op.Ra);

            if (op.NHigh)
            {
                n = context.SignExtend16(OperandType.I64, context.ShiftRightUI(n, Const(16)));
            }
            else
            {
                n = context.SignExtend16(OperandType.I64, n);
            }

            if (op.MHigh)
            {
                m = context.SignExtend16(OperandType.I64, context.ShiftRightUI(m, Const(16)));
            }
            else
            {
                m = context.SignExtend16(OperandType.I64, m);
            }

            Operand res = context.Multiply(n, m);

            Operand toAdd = context.SignExtend32(OperandType.I64, a);
            res = context.Add(res, toAdd);
            Operand q = context.ICompareNotEqual(res, context.SignExtend32(OperandType.I64, res));
            res = context.ConvertI64ToI32(res);

            UpdateQFlag(context, q);

            EmitGenericAluStoreA32(context, op.Rd, false, res);
        }

        public static void Smlal(ArmEmitterContext context)
        {
            EmitMlal(context, true);
        }

        public static void Smlal__(ArmEmitterContext context)
        {
            IOpCode32AluUmull op = (IOpCode32AluUmull)context.CurrOp;

            Operand n = GetIntA32(context, op.Rn);
            Operand m = GetIntA32(context, op.Rm);

            if (op.NHigh)
            {
                n = context.SignExtend16(OperandType.I64, context.ShiftRightUI(n, Const(16)));
            }
            else
            {
                n = context.SignExtend16(OperandType.I64, n);
            }

            if (op.MHigh)
            {
                m = context.SignExtend16(OperandType.I64, context.ShiftRightUI(m, Const(16)));
            }
            else
            {
                m = context.SignExtend16(OperandType.I64, m);
            }

            Operand res = context.Multiply(n, m);

            Operand toAdd = context.ShiftLeft(context.ZeroExtend32(OperandType.I64, GetIntA32(context, op.RdHi)), Const(32));
            toAdd = context.BitwiseOr(toAdd, context.ZeroExtend32(OperandType.I64, GetIntA32(context, op.RdLo)));
            res = context.Add(res, toAdd);

            Operand hi = context.ConvertI64ToI32(context.ShiftRightUI(res, Const(32)));
            Operand lo = context.ConvertI64ToI32(res);

            EmitGenericAluStoreA32(context, op.RdHi, false, hi);
            EmitGenericAluStoreA32(context, op.RdLo, false, lo);
        }

        public static void Smlaw_(ArmEmitterContext context)
        {
            IOpCode32AluMla op = (IOpCode32AluMla)context.CurrOp;

            Operand n = GetIntA32(context, op.Rn);
            Operand m = GetIntA32(context, op.Rm);
            Operand a = GetIntA32(context, op.Ra);

            if (op.MHigh)
            {
                m = context.SignExtend16(OperandType.I64, context.ShiftRightUI(m, Const(16)));
            }
            else
            {
                m = context.SignExtend16(OperandType.I64, m);
            }

            Operand res = context.Multiply(context.SignExtend32(OperandType.I64, n), m);

            Operand toAdd = context.ShiftLeft(context.SignExtend32(OperandType.I64, a), Const(16));
            res = context.Add(res, toAdd);
            res = context.ShiftRightSI(res, Const(16));
            Operand q = context.ICompareNotEqual(res, context.SignExtend32(OperandType.I64, res));
            res = context.ConvertI64ToI32(res);

            UpdateQFlag(context, q);

            EmitGenericAluStoreA32(context, op.Rd, false, res);
        }

        public static void Smul__(ArmEmitterContext context)
        {
            IOpCode32AluMla op = (IOpCode32AluMla)context.CurrOp;

            Operand n = GetIntA32(context, op.Rn);
            Operand m = GetIntA32(context, op.Rm);

            if (op.NHigh)
            {
                n = context.ShiftRightSI(n, Const(16));
            }
            else
            {
                n = context.SignExtend16(OperandType.I32, n);
            }

            if (op.MHigh)
            {
                m = context.ShiftRightSI(m, Const(16));
            }
            else
            {
                m = context.SignExtend16(OperandType.I32, m);
            }

            Operand res = context.Multiply(n, m);

            EmitGenericAluStoreA32(context, op.Rd, false, res);
        }

        public static void Smull(ArmEmitterContext context)
        {
            IOpCode32AluUmull op = (IOpCode32AluUmull)context.CurrOp;

            Operand n = context.SignExtend32(OperandType.I64, GetIntA32(context, op.Rn));
            Operand m = context.SignExtend32(OperandType.I64, GetIntA32(context, op.Rm));

            Operand res = context.Multiply(n, m);

            Operand hi = context.ConvertI64ToI32(context.ShiftRightUI(res, Const(32)));
            Operand lo = context.ConvertI64ToI32(res);

            if (ShouldSetFlags(context))
            {
                EmitNZFlagsCheck(context, res);
            }

            EmitGenericAluStoreA32(context, op.RdHi, ShouldSetFlags(context), hi);
            EmitGenericAluStoreA32(context, op.RdLo, ShouldSetFlags(context), lo);
        }

        public static void Smulw_(ArmEmitterContext context)
        {
            IOpCode32AluMla op = (IOpCode32AluMla)context.CurrOp;

            Operand n = GetIntA32(context, op.Rn);
            Operand m = GetIntA32(context, op.Rm);

            if (op.MHigh)
            {
                m = context.SignExtend16(OperandType.I64, context.ShiftRightUI(m, Const(16)));
            }
            else
            {
                m = context.SignExtend16(OperandType.I64, m);
            }

            Operand res = context.Multiply(context.SignExtend32(OperandType.I64, n), m);

            res = context.ShiftRightUI(res, Const(16));
            res = context.ConvertI64ToI32(res);

            EmitGenericAluStoreA32(context, op.Rd, false, res);
        }

        public static void Umaal(ArmEmitterContext context)
        {
            IOpCode32AluUmull op = (IOpCode32AluUmull)context.CurrOp;

            Operand n = context.ZeroExtend32(OperandType.I64, GetIntA32(context, op.Rn));
            Operand m = context.ZeroExtend32(OperandType.I64, GetIntA32(context, op.Rm));
            Operand dHi = context.ZeroExtend32(OperandType.I64, GetIntA32(context, op.RdHi));
            Operand dLo = context.ZeroExtend32(OperandType.I64, GetIntA32(context, op.RdLo));

            Operand res = context.Multiply(n, m);
            res = context.Add(res, dHi);
            res = context.Add(res, dLo);

            Operand hi = context.ConvertI64ToI32(context.ShiftRightUI(res, Const(32)));
            Operand lo = context.ConvertI64ToI32(res);

            EmitGenericAluStoreA32(context, op.RdHi, false, hi);
            EmitGenericAluStoreA32(context, op.RdLo, false, lo);
        }

        public static void Umlal(ArmEmitterContext context)
        {
            EmitMlal(context, false);
        }

        public static void Umull(ArmEmitterContext context)
        {
            IOpCode32AluUmull op = (IOpCode32AluUmull)context.CurrOp;

            Operand n = context.ZeroExtend32(OperandType.I64, GetIntA32(context, op.Rn));
            Operand m = context.ZeroExtend32(OperandType.I64, GetIntA32(context, op.Rm));

            Operand res = context.Multiply(n, m);

            Operand hi = context.ConvertI64ToI32(context.ShiftRightUI(res, Const(32)));
            Operand lo = context.ConvertI64ToI32(res);

            if (ShouldSetFlags(context))
            {
                EmitNZFlagsCheck(context, res);
            }

            EmitGenericAluStoreA32(context, op.RdHi, ShouldSetFlags(context), hi);
            EmitGenericAluStoreA32(context, op.RdLo, ShouldSetFlags(context), lo);
        }

        private static void EmitMlal(ArmEmitterContext context, bool signed)
        {
            IOpCode32AluUmull op = (IOpCode32AluUmull)context.CurrOp;

            Operand n = GetIntA32(context, op.Rn);
            Operand m = GetIntA32(context, op.Rm);

            if (signed)
            {
                n = context.SignExtend32(OperandType.I64, n);
                m = context.SignExtend32(OperandType.I64, m);
            }
            else
            {
                n = context.ZeroExtend32(OperandType.I64, n);
                m = context.ZeroExtend32(OperandType.I64, m);
            }

            Operand res = context.Multiply(n, m);

            Operand toAdd = context.ShiftLeft(context.ZeroExtend32(OperandType.I64, GetIntA32(context, op.RdHi)), Const(32));
            toAdd = context.BitwiseOr(toAdd, context.ZeroExtend32(OperandType.I64, GetIntA32(context, op.RdLo)));
            res = context.Add(res, toAdd);

            Operand hi = context.ConvertI64ToI32(context.ShiftRightUI(res, Const(32)));
            Operand lo = context.ConvertI64ToI32(res);

            if (ShouldSetFlags(context))
            {
                EmitNZFlagsCheck(context, res);
            }

            EmitGenericAluStoreA32(context, op.RdHi, ShouldSetFlags(context), hi);
            EmitGenericAluStoreA32(context, op.RdLo, ShouldSetFlags(context), lo);
        }

        private static void UpdateQFlag(ArmEmitterContext context, Operand q)
        {
            Operand lblSkipSetQ = Label();

            context.BranchIfFalse(lblSkipSetQ, q);

            SetFlag(context, PState.QFlag, Const(1));

            context.MarkLabel(lblSkipSetQ);
        }
    }
}
