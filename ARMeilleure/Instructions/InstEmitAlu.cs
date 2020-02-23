using ARMeilleure.Decoders;
using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.State;
using ARMeilleure.Translation;
using System.Diagnostics;

using static ARMeilleure.Instructions.InstEmitAluHelper;
using static ARMeilleure.Instructions.InstEmitHelper;
using static ARMeilleure.IntermediateRepresentation.OperandHelper;

namespace ARMeilleure.Instructions
{
    static partial class InstEmit
    {
        public static void Adc(ArmEmitterContext context)  => EmitAdc(context, setFlags: false);
        public static void Adcs(ArmEmitterContext context) => EmitAdc(context, setFlags: true);

        private static void EmitAdc(ArmEmitterContext context, bool setFlags)
        {
            Operand n = GetAluN(context);
            Operand m = GetAluM(context);

            Operand d = context.Add(n, m);

            Operand carry = GetFlag(PState.CFlag);

            if (context.CurrOp.RegisterSize == RegisterSize.Int64)
            {
                carry = context.ZeroExtend32(OperandType.I64, carry);
            }

            d = context.Add(d, carry);

            if (setFlags)
            {
                EmitNZFlagsCheck(context, d);

                EmitAdcsCCheck(context, n, d);
                EmitAddsVCheck(context, n, m, d);
            }

            SetAluDOrZR(context, d);
        }

        public static void Add(ArmEmitterContext context)
        {
            SetAluD(context, context.Add(GetAluN(context), GetAluM(context)));
        }

        public static void Adds(ArmEmitterContext context)
        {
            Operand n = GetAluN(context);
            Operand m = GetAluM(context);

            context.MarkComparison(n, m);

            Operand d = context.Add(n, m);

            EmitNZFlagsCheck(context, d);

            EmitAddsCCheck(context, n, d);
            EmitAddsVCheck(context, n, m, d);

            SetAluDOrZR(context, d);
        }

        public static void And(ArmEmitterContext context)
        {
            SetAluD(context, context.BitwiseAnd(GetAluN(context), GetAluM(context)));
        }

        public static void Ands(ArmEmitterContext context)
        {
            Operand n = GetAluN(context);
            Operand m = GetAluM(context);

            Operand d = context.BitwiseAnd(n, m);

            EmitNZFlagsCheck(context, d);
            EmitCVFlagsClear(context);

            SetAluDOrZR(context, d);
        }

        public static void Asrv(ArmEmitterContext context)
        {
            SetAluDOrZR(context, context.ShiftRightSI(GetAluN(context), GetAluMShift(context)));
        }

        public static void Bic(ArmEmitterContext context)  => EmitBic(context, setFlags: false);
        public static void Bics(ArmEmitterContext context) => EmitBic(context, setFlags: true);

        private static void EmitBic(ArmEmitterContext context, bool setFlags)
        {
            Operand n = GetAluN(context);
            Operand m = GetAluM(context);

            Operand d = context.BitwiseAnd(n, context.BitwiseNot(m));

            if (setFlags)
            {
                EmitNZFlagsCheck(context, d);
                EmitCVFlagsClear(context);
            }

            SetAluD(context, d, setFlags);
        }

        public static void Cls(ArmEmitterContext context)
        {
            OpCodeAlu op = (OpCodeAlu)context.CurrOp;

            Operand n = GetIntOrZR(context, op.Rn);

            Operand nHigh = context.ShiftRightUI(n, Const(1));

            bool is32Bits = op.RegisterSize == RegisterSize.Int32;

            Operand mask = is32Bits ? Const(int.MaxValue) : Const(long.MaxValue);

            Operand nLow = context.BitwiseAnd(n, mask);

            Operand res = context.CountLeadingZeros(context.BitwiseExclusiveOr(nHigh, nLow));

            res = context.Subtract(res, Const(res.Type, 1));

            SetAluDOrZR(context, res);
        }

        public static void Clz(ArmEmitterContext context)
        {
            OpCodeAlu op = (OpCodeAlu)context.CurrOp;

            Operand n = GetIntOrZR(context, op.Rn);

            Operand d = context.CountLeadingZeros(n);

            SetAluDOrZR(context, d);
        }

        public static void Eon(ArmEmitterContext context)
        {
            Operand n = GetAluN(context);
            Operand m = GetAluM(context);

            Operand d = context.BitwiseExclusiveOr(n, context.BitwiseNot(m));

            SetAluD(context, d);
        }

        public static void Eor(ArmEmitterContext context)
        {
            SetAluD(context, context.BitwiseExclusiveOr(GetAluN(context), GetAluM(context)));
        }

        public static void Extr(ArmEmitterContext context)
        {
            OpCodeAluRs op = (OpCodeAluRs)context.CurrOp;

            Operand res = GetIntOrZR(context, op.Rm);

            if (op.Shift != 0)
            {
                if (op.Rn == op.Rm)
                {
                    res = context.RotateRight(res, Const(op.Shift));
                }
                else
                {
                    res = context.ShiftRightUI(res, Const(op.Shift));

                    Operand n = GetIntOrZR(context, op.Rn);

                    int invShift = op.GetBitsCount() - op.Shift;

                    res = context.BitwiseOr(res, context.ShiftLeft(n, Const(invShift)));
                }
            }

            SetAluDOrZR(context, res);
        }

        public static void Lslv(ArmEmitterContext context)
        {
            SetAluDOrZR(context, context.ShiftLeft(GetAluN(context), GetAluMShift(context)));
        }

        public static void Lsrv(ArmEmitterContext context)
        {
            SetAluDOrZR(context, context.ShiftRightUI(GetAluN(context), GetAluMShift(context)));
        }

        public static void Sbc(ArmEmitterContext context)  => EmitSbc(context, setFlags: false);
        public static void Sbcs(ArmEmitterContext context) => EmitSbc(context, setFlags: true);

        private static void EmitSbc(ArmEmitterContext context, bool setFlags)
        {
            Operand n = GetAluN(context);
            Operand m = GetAluM(context);

            Operand d = context.Subtract(n, m);

            Operand borrow = context.BitwiseExclusiveOr(GetFlag(PState.CFlag), Const(1));

            if (context.CurrOp.RegisterSize == RegisterSize.Int64)
            {
                borrow = context.ZeroExtend32(OperandType.I64, borrow);
            }

            d = context.Subtract(d, borrow);

            if (setFlags)
            {
                EmitNZFlagsCheck(context, d);

                EmitSbcsCCheck(context, n, m);
                EmitSubsVCheck(context, n, m, d);
            }

            SetAluDOrZR(context, d);
        }

        public static void Sub(ArmEmitterContext context)
        {
            SetAluD(context, context.Subtract(GetAluN(context), GetAluM(context)));
        }

        public static void Subs(ArmEmitterContext context)
        {
            Operand n = GetAluN(context);
            Operand m = GetAluM(context);

            context.MarkComparison(n, m);

            Operand d = context.Subtract(n, m);

            EmitNZFlagsCheck(context, d);

            EmitSubsCCheck(context, n, m);
            EmitSubsVCheck(context, n, m, d);

            SetAluDOrZR(context, d);
        }

        public static void Orn(ArmEmitterContext context)
        {
            Operand n = GetAluN(context);
            Operand m = GetAluM(context);

            Operand d = context.BitwiseOr(n, context.BitwiseNot(m));

            SetAluD(context, d);
        }

        public static void Orr(ArmEmitterContext context)
        {
            SetAluD(context, context.BitwiseOr(GetAluN(context), GetAluM(context)));
        }

        public static void Rbit(ArmEmitterContext context)
        {
            OpCodeAlu op = (OpCodeAlu)context.CurrOp;

            Operand n = GetIntOrZR(context, op.Rn);
            Operand d;

            if (op.RegisterSize == RegisterSize.Int32)
            {
                d = EmitReverseBits32Op(context, n);
            }
            else
            {
                d = EmitReverseBits64Op(context, n);
            }

            SetAluDOrZR(context, d);
        }

        private static Operand EmitReverseBits64Op(ArmEmitterContext context, Operand op)
        {
            Debug.Assert(op.Type == OperandType.I64);

            Operand val = context.BitwiseOr(context.ShiftRightUI(context.BitwiseAnd(op, Const(0xaaaaaaaaaaaaaaaaul)), Const(1)),
                                            context.ShiftLeft   (context.BitwiseAnd(op, Const(0x5555555555555555ul)), Const(1)));

            val = context.BitwiseOr(context.ShiftRightUI(context.BitwiseAnd(val, Const(0xccccccccccccccccul)), Const(2)),
                                    context.ShiftLeft   (context.BitwiseAnd(val, Const(0x3333333333333333ul)), Const(2)));
            val = context.BitwiseOr(context.ShiftRightUI(context.BitwiseAnd(val, Const(0xf0f0f0f0f0f0f0f0ul)), Const(4)),
                                    context.ShiftLeft   (context.BitwiseAnd(val, Const(0x0f0f0f0f0f0f0f0ful)), Const(4)));
            val = context.BitwiseOr(context.ShiftRightUI(context.BitwiseAnd(val, Const(0xff00ff00ff00ff00ul)), Const(8)),
                                    context.ShiftLeft   (context.BitwiseAnd(val, Const(0x00ff00ff00ff00fful)), Const(8)));
            val = context.BitwiseOr(context.ShiftRightUI(context.BitwiseAnd(val, Const(0xffff0000ffff0000ul)), Const(16)),
                                    context.ShiftLeft   (context.BitwiseAnd(val, Const(0x0000ffff0000fffful)), Const(16)));

            return context.BitwiseOr(context.ShiftRightUI(val, Const(32)), context.ShiftLeft(val, Const(32)));
        }

        public static void Rev16(ArmEmitterContext context)
        {
            OpCodeAlu op = (OpCodeAlu)context.CurrOp;

            Operand n = GetIntOrZR(context, op.Rn);
            Operand d;

            if (op.RegisterSize == RegisterSize.Int32)
            {
                d = EmitReverseBytes16_32Op(context, n);
            }
            else
            {
                d = EmitReverseBytes16_64Op(context, n);
            }

            SetAluDOrZR(context, d);
        }

        public static void Rev32(ArmEmitterContext context)
        {
            OpCodeAlu op = (OpCodeAlu)context.CurrOp;

            Operand n = GetIntOrZR(context, op.Rn);
            Operand d;

            if (op.RegisterSize == RegisterSize.Int32)
            {
                d = context.ByteSwap(n);
            }
            else
            {
                d = EmitReverseBytes32_64Op(context, n);
            }

            SetAluDOrZR(context, d);
        }

        private static Operand EmitReverseBytes32_64Op(ArmEmitterContext context, Operand op)
        {
            Debug.Assert(op.Type == OperandType.I64);

            Operand val = EmitReverseBytes16_64Op(context, op);

            return context.BitwiseOr(context.ShiftRightUI(context.BitwiseAnd(val, Const(0xffff0000ffff0000ul)), Const(16)),
                                     context.ShiftLeft   (context.BitwiseAnd(val, Const(0x0000ffff0000fffful)), Const(16)));
        }

        public static void Rev64(ArmEmitterContext context)
        {
            OpCodeAlu op = (OpCodeAlu)context.CurrOp;

            SetAluDOrZR(context, context.ByteSwap(GetIntOrZR(context, op.Rn)));
        }

        public static void Rorv(ArmEmitterContext context)
        {
            SetAluDOrZR(context, context.RotateRight(GetAluN(context), GetAluMShift(context)));
        }

        private static Operand GetAluMShift(ArmEmitterContext context)
        {
            IOpCodeAluRs op = (IOpCodeAluRs)context.CurrOp;

            Operand m = GetIntOrZR(context, op.Rm);

            if (op.RegisterSize == RegisterSize.Int64)
            {
                m = context.ConvertI64ToI32(m);
            }

            return context.BitwiseAnd(m, Const(context.CurrOp.GetBitsCount() - 1));
        }

        private static void EmitCVFlagsClear(ArmEmitterContext context)
        {
            SetFlag(context, PState.CFlag, Const(0));
            SetFlag(context, PState.VFlag, Const(0));
        }

        public static void SetAluD(ArmEmitterContext context, Operand d)
        {
            SetAluD(context, d, x31IsZR: false);
        }

        public static void SetAluDOrZR(ArmEmitterContext context, Operand d)
        {
            SetAluD(context, d, x31IsZR: true);
        }

        public static void SetAluD(ArmEmitterContext context, Operand d, bool x31IsZR)
        {
            IOpCodeAlu op = (IOpCodeAlu)context.CurrOp;

            if ((x31IsZR || op is IOpCodeAluRs) && op.Rd == RegisterConsts.ZeroIndex)
            {
                return;
            }

            SetIntOrSP(context, op.Rd, d);
        }
    }
}
