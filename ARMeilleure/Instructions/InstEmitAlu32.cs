using ARMeilleure.Decoders;
using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.State;
using ARMeilleure.Translation;

using static ARMeilleure.Instructions.InstEmitAluHelper;
using static ARMeilleure.Instructions.InstEmitHelper;
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

        public static void Adc(ArmEmitterContext context)
        {
            IOpCode32Alu op = (IOpCode32Alu)context.CurrOp;

            Operand n = GetAluN(context);
            Operand m = GetAluM(context, setCarry: false);

            Operand res = context.Add(n, m);

            Operand carry = GetFlag(PState.CFlag);

            res = context.Add(res, carry);

            if (op.SetFlags)
            {
                EmitNZFlagsCheck(context, res);

                EmitAdcsCCheck(context, n, res);
                EmitAddsVCheck(context, n, m, res);
            }

            EmitAluStore(context, res);
        }

        public static void And(ArmEmitterContext context)
        {
            IOpCode32Alu op = (IOpCode32Alu)context.CurrOp;

            Operand n = GetAluN(context);
            Operand m = GetAluM(context);

            Operand res = context.BitwiseAnd(n, m);

            if (op.SetFlags)
            {
                EmitNZFlagsCheck(context, res);
            }

            EmitAluStore(context, res);
        }

        public static void Bfc(ArmEmitterContext context)
        {
            OpCode32AluBf op = (OpCode32AluBf)context.CurrOp;

            Operand d = GetIntA32(context, op.Rd);
            Operand res = context.BitwiseAnd(d, Const(~op.DestMask));

            SetIntA32(context, op.Rd, res);
        }

        public static void Bfi(ArmEmitterContext context)
        {
            OpCode32AluBf op = (OpCode32AluBf)context.CurrOp;

            Operand n = GetIntA32(context, op.Rn);
            Operand d = GetIntA32(context, op.Rd);
            Operand part = context.BitwiseAnd(n, Const(op.SourceMask));

            if (op.Lsb != 0)
            {
                part = context.ShiftLeft(part, Const(op.Lsb));
            }

            Operand res = context.BitwiseAnd(d, Const(~op.DestMask));
            res = context.BitwiseOr(res, context.BitwiseAnd(part, Const(op.DestMask)));

            SetIntA32(context, op.Rd, res);
        }

        public static void Bic(ArmEmitterContext context)
        {
            IOpCode32Alu op = (IOpCode32Alu)context.CurrOp;

            Operand n = GetAluN(context);
            Operand m = GetAluM(context);

            Operand res = context.BitwiseAnd(n, context.BitwiseNot(m));

            if (op.SetFlags)
            {
                EmitNZFlagsCheck(context, res);
            }

            EmitAluStore(context, res);
        }

        public static void Clz(ArmEmitterContext context)
        {
            Operand m = GetAluM(context, setCarry: false);

            Operand res = context.CountLeadingZeros(m);
            EmitAluStore(context, res);
        }

        public static void Cmp(ArmEmitterContext context)
        {
            Operand n = GetAluN(context);
            Operand m = GetAluM(context, setCarry: false);

            Operand res = context.Subtract(n, m);

            EmitNZFlagsCheck(context, res);

            EmitSubsCCheck(context, n, res);
            EmitSubsVCheck(context, n, m, res);
        }

        public static void Cmn(ArmEmitterContext context)
        {
            Operand n = GetAluN(context);
            Operand m = GetAluM(context, setCarry: false);

            Operand res = context.Add(n, m);

            EmitNZFlagsCheck(context, res);

            EmitAddsCCheck(context, n, res);
            EmitAddsVCheck(context, n, m, res);
        }

        public static void Eor(ArmEmitterContext context)
        {
            IOpCode32Alu op = (IOpCode32Alu)context.CurrOp;

            Operand n = GetAluN(context);
            Operand m = GetAluM(context);

            Operand res = context.BitwiseExclusiveOr(n, m);

            if (op.SetFlags)
            {
                EmitNZFlagsCheck(context, res);
            }

            EmitAluStore(context, res);
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

        public static void Movt(ArmEmitterContext context)
        {
            OpCode32AluImm16 op = (OpCode32AluImm16)context.CurrOp;

            Operand d = GetIntA32(context, op.Rd);
            Operand imm = Const(op.Immediate << 16); // Immeditate value as top halfword.
            Operand res = context.BitwiseAnd(d, Const(0x0000ffff));
            res = context.BitwiseOr(res, imm);

            EmitAluStore(context, res);
        }

        public static void Mul(ArmEmitterContext context)
        {
            IOpCode32Alu op = (IOpCode32Alu)context.CurrOp;

            Operand n = GetAluN(context);
            Operand m = GetAluM(context);

            Operand res = context.Multiply(n, m);

            if (op.SetFlags)
            {
                EmitNZFlagsCheck(context, res);
            }

            EmitAluStore(context, res);
        }

        public static void Mvn(ArmEmitterContext context)
        {
            IOpCode32Alu op = (IOpCode32Alu)context.CurrOp;
            Operand m = GetAluM(context);

            Operand res = context.BitwiseNot(m);

            if (op.SetFlags)
            {
                EmitNZFlagsCheck(context, res);
            }

            EmitAluStore(context, res);
        }

        public static void Orr(ArmEmitterContext context)
        {
            IOpCode32Alu op = (IOpCode32Alu)context.CurrOp;

            Operand n = GetAluN(context);
            Operand m = GetAluM(context);

            Operand res = context.BitwiseOr(n, m);

            if (op.SetFlags)
            {
                EmitNZFlagsCheck(context, res);
            }

            EmitAluStore(context, res);
        }

        public static void Pkh(ArmEmitterContext context)
        {
            OpCode32AluRsImm op = (OpCode32AluRsImm)context.CurrOp;

            Operand n = GetAluN(context);
            Operand m = GetAluM(context);

            Operand res;

            bool tbform = op.ShiftType == ShiftType.Asr;
            if (tbform)
            {
                res = context.BitwiseOr(context.BitwiseAnd(n, Const(0xFFFF0000)), context.BitwiseAnd(m, Const(0xFFFF)));
            }
            else
            {
                res = context.BitwiseOr(context.BitwiseAnd(m, Const(0xFFFF0000)), context.BitwiseAnd(n, Const(0xFFFF)));
            }

            EmitAluStore(context, res);
        }

        public static void Rbit(ArmEmitterContext context)
        {
            Operand m = GetAluM(context);

            Operand res = EmitReverseBits32Op(context, m);

            EmitAluStore(context, res);
        }

        public static void Rev(ArmEmitterContext context)
        {
            Operand m = GetAluM(context);

            Operand res = context.ByteSwap(m);

            EmitAluStore(context, res);
        }

        public static void Rev16(ArmEmitterContext context)
        {
            Operand m = GetAluM(context);

            Operand res = EmitReverseBytes16_32Op(context, m);

            EmitAluStore(context, res);
        }

        public static void Revsh(ArmEmitterContext context)
        {
            Operand m = GetAluM(context);

            Operand res = EmitReverseBytes16_32Op(context, m);

            EmitAluStore(context, context.SignExtend16(OperandType.I32, res));
        }

        public static void Rsc(ArmEmitterContext context)
        {
            IOpCode32Alu op = (IOpCode32Alu)context.CurrOp;

            Operand n = GetAluN(context);
            Operand m = GetAluM(context, setCarry: false);

            Operand res = context.Subtract(m, n);

            Operand borrow = context.BitwiseExclusiveOr(GetFlag(PState.CFlag), Const(1));

            res = context.Subtract(res, borrow);

            if (op.SetFlags)
            {
                EmitNZFlagsCheck(context, res);

                EmitSbcsCCheck(context, m, n);
                EmitSubsVCheck(context, m, n, res);
            }

            EmitAluStore(context, res);
        }

        public static void Rsb(ArmEmitterContext context)
        {
            IOpCode32Alu op = (IOpCode32Alu)context.CurrOp;

            Operand n = GetAluN(context);
            Operand m = GetAluM(context, setCarry: false);

            Operand res = context.Subtract(m, n);

            if (op.SetFlags)
            {
                EmitNZFlagsCheck(context, res);

                EmitSubsCCheck(context, m, res);
                EmitSubsVCheck(context, m, n, res);
            }

            EmitAluStore(context, res);
        }

        public static void Sbc(ArmEmitterContext context)
        {
            IOpCode32Alu op = (IOpCode32Alu)context.CurrOp;

            Operand n = GetAluN(context);
            Operand m = GetAluM(context, setCarry: false);

            Operand res = context.Subtract(n, m);

            Operand borrow = context.BitwiseExclusiveOr(GetFlag(PState.CFlag), Const(1));

            res = context.Subtract(res, borrow);

            if (op.SetFlags)
            {
                EmitNZFlagsCheck(context, res);

                EmitSbcsCCheck(context, n, m);
                EmitSubsVCheck(context, n, m, res);
            }

            EmitAluStore(context, res);
        }

        public static void Sbfx(ArmEmitterContext context)
        {
            OpCode32AluBf op = (OpCode32AluBf)context.CurrOp;

            var msb = op.Lsb + op.Msb; // For this instruction, the msb is actually a width.

            Operand n = GetIntA32(context, op.Rn);
            Operand res = context.ShiftRightSI(context.ShiftLeft(n, Const(31 - msb)), Const(31 - op.Msb));

            SetIntA32(context, op.Rd, res);
        }

        public static void Sdiv(ArmEmitterContext context)
        {
            EmitDiv(context, false);
        }

        public static void Ssat(ArmEmitterContext context)
        {
            OpCode32Sat op = (OpCode32Sat)context.CurrOp;

            EmitSat(context, -(1 << op.SatImm), (1 << op.SatImm) - 1);
        }

        public static void Ssat16(ArmEmitterContext context)
        {
            OpCode32Sat16 op = (OpCode32Sat16)context.CurrOp;

            EmitSat16(context, -(1 << op.SatImm), (1 << op.SatImm) - 1);
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

        public static void Sxtb(ArmEmitterContext context)
        {
            EmitSignExtend(context, true, 8);
        }

        public static void Sxtb16(ArmEmitterContext context)
        {
            EmitExtend16(context, true);
        }

        public static void Sxth(ArmEmitterContext context)
        {
            EmitSignExtend(context, true, 16);
        }

        public static void Teq(ArmEmitterContext context)
        {
            Operand n = GetAluN(context);
            Operand m = GetAluM(context);

            Operand res = context.BitwiseExclusiveOr(n, m);

            EmitNZFlagsCheck(context, res);
        }

        public static void Tst(ArmEmitterContext context)
        {
            Operand n = GetAluN(context);
            Operand m = GetAluM(context);

            Operand res = context.BitwiseAnd(n, m);
            EmitNZFlagsCheck(context, res);
        }

        public static void Ubfx(ArmEmitterContext context)
        {
            OpCode32AluBf op = (OpCode32AluBf)context.CurrOp;

            var msb = op.Lsb + op.Msb; // For this instruction, the msb is actually a width.

            Operand n = GetIntA32(context, op.Rn);
            Operand res = context.ShiftRightUI(context.ShiftLeft(n, Const(31 - msb)), Const(31 - op.Msb));

            SetIntA32(context, op.Rd, res);
        }

        public static void Udiv(ArmEmitterContext context)
        {
            EmitDiv(context, true);
        }

        public static void Usat(ArmEmitterContext context)
        {
            OpCode32Sat op = (OpCode32Sat)context.CurrOp;

            EmitSat(context, 0, op.SatImm == 32 ? (int)(~0) : (1 << op.SatImm) - 1);
        }

        public static void Usat16(ArmEmitterContext context)
        {
            OpCode32Sat16 op = (OpCode32Sat16)context.CurrOp;

            EmitSat16(context, 0, (1 << op.SatImm) - 1);
        }

        public static void Uxtb(ArmEmitterContext context)
        {
            EmitSignExtend(context, false, 8);
        }

        public static void Uxtb16(ArmEmitterContext context)
        {
            EmitExtend16(context, false);
        }

        public static void Uxth(ArmEmitterContext context)
        {
            EmitSignExtend(context, false, 16);
        }

        private static void EmitSignExtend(ArmEmitterContext context, bool signed, int bits)
        {
            IOpCode32AluUx op = (IOpCode32AluUx)context.CurrOp;

            Operand m = GetAluM(context);
            Operand res;

            if (op.RotateBits == 0)
            {
                res = m;
            }
            else
            {
                Operand rotate = Const(op.RotateBits);
                res = context.RotateRight(m, rotate);
            }

            switch (bits)
            {
                case 8:
                    res = (signed) ? context.SignExtend8(OperandType.I32, res) : context.ZeroExtend8(OperandType.I32, res);
                    break;
                case 16:
                    res = (signed) ? context.SignExtend16(OperandType.I32, res) : context.ZeroExtend16(OperandType.I32, res);
                    break;
            }

            if (op.Add)
            {
                res = context.Add(res, GetAluN(context));
            }

            EmitAluStore(context, res);
        }

        private static void EmitExtend16(ArmEmitterContext context, bool signed)
        {
            IOpCode32AluUx op = (IOpCode32AluUx)context.CurrOp;

            Operand m = GetAluM(context);
            Operand res;

            if (op.RotateBits == 0)
            {
                res = m;
            }
            else
            {
                Operand rotate = Const(op.RotateBits);
                res = context.RotateRight(m, rotate);
            }

            Operand low16, high16;
            if (signed)
            {
                low16 = context.SignExtend8(OperandType.I32, res);
                high16 = context.SignExtend8(OperandType.I32, context.ShiftRightUI(res, Const(16)));
            }
            else
            {
                low16 = context.ZeroExtend8(OperandType.I32, res);
                high16 = context.ZeroExtend8(OperandType.I32, context.ShiftRightUI(res, Const(16)));
            }

            if (op.Add)
            {
                Operand n = GetAluN(context);
                Operand lowAdd, highAdd;
                if (signed)
                {
                    lowAdd = context.SignExtend16(OperandType.I32, n);
                    highAdd = context.SignExtend16(OperandType.I32, context.ShiftRightUI(n, Const(16)));
                }
                else
                {
                    lowAdd = context.ZeroExtend16(OperandType.I32, n);
                    highAdd = context.ZeroExtend16(OperandType.I32, context.ShiftRightUI(n, Const(16)));
                }

                low16 = context.Add(low16, lowAdd);
                high16 = context.Add(high16, highAdd);
            }

            res = context.BitwiseOr(
                context.ZeroExtend16(OperandType.I32, low16),
                context.ShiftLeft(context.ZeroExtend16(OperandType.I32, high16), Const(16)));

            EmitAluStore(context, res);
        }

        private static void EmitDiv(ArmEmitterContext context, bool unsigned)
        {
            Operand n = GetAluN(context);
            Operand m = GetAluM(context);
            Operand zero = Const(m.Type, 0);

            Operand divisorIsZero = context.ICompareEqual(m, zero);

            Operand lblBadDiv = Label();
            Operand lblEnd = Label();

            context.BranchIfTrue(lblBadDiv, divisorIsZero);

            if (!unsigned)
            {
                // ARM64 behaviour: If Rn == INT_MIN && Rm == -1, Rd = INT_MIN (overflow).
                // TODO: tests to ensure A32 works the same

                Operand intMin = Const(int.MinValue);
                Operand minus1 = Const(-1);

                Operand nIsIntMin = context.ICompareEqual(n, intMin);
                Operand mIsMinus1 = context.ICompareEqual(m, minus1);

                Operand lblGoodDiv = Label();

                context.BranchIfFalse(lblGoodDiv, context.BitwiseAnd(nIsIntMin, mIsMinus1));

                EmitAluStore(context, intMin);

                context.Branch(lblEnd);

                context.MarkLabel(lblGoodDiv);
            }

            Operand res = unsigned
                ? context.DivideUI(n, m)
                : context.Divide(n, m);

            EmitAluStore(context, res);

            context.Branch(lblEnd);

            context.MarkLabel(lblBadDiv);

            EmitAluStore(context, zero);

            context.MarkLabel(lblEnd);
        }

        private static void EmitSat(ArmEmitterContext context, int intMin, int intMax)
        {
            OpCode32Sat op = (OpCode32Sat)context.CurrOp;

            Operand n = GetIntA32(context, op.Rn);

            int shift = DecodeImmShift(op.ShiftType, op.Imm5);

            switch (op.ShiftType)
            {
                case ShiftType.Lsl:
                    if (shift == 32)
                    {
                        n = Const(0);
                    }
                    else
                    {
                        n = context.ShiftLeft(n, Const(shift));
                    }
                    break;
                case ShiftType.Asr:
                    if (shift == 32)
                    {
                        n = context.ShiftRightSI(n, Const(31));
                    }
                    else
                    {
                        n = context.ShiftRightSI(n, Const(shift));
                    }
                    break;
            }

            Operand lblCheckLtIntMin = Label();
            Operand lblNoSat = Label();
            Operand lblEnd = Label();

            context.BranchIfFalse(lblCheckLtIntMin, context.ICompareGreater(n, Const(intMax)));

            SetFlag(context, PState.QFlag, Const(1));
            SetIntA32(context, op.Rd, Const(intMax));
            context.Branch(lblEnd);

            context.MarkLabel(lblCheckLtIntMin);
            context.BranchIfFalse(lblNoSat, context.ICompareLess(n, Const(intMin)));

            SetFlag(context, PState.QFlag, Const(1));
            SetIntA32(context, op.Rd, Const(intMin));
            context.Branch(lblEnd);

            context.MarkLabel(lblNoSat);

            SetIntA32(context, op.Rd, n);

            context.MarkLabel(lblEnd);
        }

        private static void EmitSat16(ArmEmitterContext context, int intMin, int intMax)
        {
            OpCode32Sat16 op = (OpCode32Sat16)context.CurrOp;

            void SetD(int part, Operand value)
            {
                if (part == 0)
                {
                    SetIntA32(context, op.Rd, context.ZeroExtend16(OperandType.I32, value));
                }
                else
                {
                    SetIntA32(context, op.Rd, context.BitwiseOr(GetIntA32(context, op.Rd), context.ShiftLeft(value, Const(16))));
                }
            }

            Operand n = GetIntA32(context, op.Rn);

            Operand nLow = context.SignExtend16(OperandType.I32, n);
            Operand nHigh = context.ShiftRightSI(n, Const(16));

            for (int part = 0; part < 2; part++)
            {
                Operand nPart = part == 0 ? nLow : nHigh;

                Operand lblCheckLtIntMin = Label();
                Operand lblNoSat = Label();
                Operand lblEnd = Label();

                context.BranchIfFalse(lblCheckLtIntMin, context.ICompareGreater(nPart, Const(intMax)));

                SetFlag(context, PState.QFlag, Const(1));
                SetD(part, Const(intMax));
                context.Branch(lblEnd);

                context.MarkLabel(lblCheckLtIntMin);
                context.BranchIfFalse(lblNoSat, context.ICompareLess(nPart, Const(intMin)));

                SetFlag(context, PState.QFlag, Const(1));
                SetD(part, Const(intMin));
                context.Branch(lblEnd);

                context.MarkLabel(lblNoSat);

                SetD(part, nPart);

                context.MarkLabel(lblEnd);
            }
        }

        private static void EmitAluStore(ArmEmitterContext context, Operand value)
        {
            IOpCode32Alu op = (IOpCode32Alu)context.CurrOp;

            EmitGenericAluStoreA32(context, op.Rd, op.SetFlags, value);
        }
    }
}