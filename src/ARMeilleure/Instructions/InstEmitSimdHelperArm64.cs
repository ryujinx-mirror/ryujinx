using ARMeilleure.Decoders;
using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.State;
using ARMeilleure.Translation;

using static ARMeilleure.Instructions.InstEmitHelper;
using static ARMeilleure.IntermediateRepresentation.Operand.Factory;

namespace ARMeilleure.Instructions
{
    static class InstEmitSimdHelperArm64
    {
        public static void EmitScalarUnaryOpF(ArmEmitterContext context, Intrinsic inst)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            Operand n = GetVec(op.Rn);

            if ((op.Size & 1) != 0)
            {
                inst |= Intrinsic.Arm64VDouble;
            }

            context.Copy(GetVec(op.Rd), context.AddIntrinsic(inst, n));
        }

        public static void EmitScalarUnaryOpFFromGp(ArmEmitterContext context, Intrinsic inst)
        {
            OpCodeSimdCvt op = (OpCodeSimdCvt)context.CurrOp;

            Operand n = GetIntOrZR(context, op.Rn);

            if ((op.Size & 1) != 0)
            {
                inst |= Intrinsic.Arm64VDouble;
            }

            context.Copy(GetVec(op.Rd), context.AddIntrinsic(inst, n));
        }

        public static void EmitScalarUnaryOpFToGp(ArmEmitterContext context, Intrinsic inst)
        {
            OpCodeSimdCvt op = (OpCodeSimdCvt)context.CurrOp;

            Operand n = GetVec(op.Rn);

            if ((op.Size & 1) != 0)
            {
                inst |= Intrinsic.Arm64VDouble;
            }

            SetIntOrZR(context, op.Rd, op.RegisterSize == RegisterSize.Int32
                ? context.AddIntrinsicInt(inst, n)
                : context.AddIntrinsicLong(inst, n));
        }

        public static void EmitScalarBinaryOpF(ArmEmitterContext context, Intrinsic inst)
        {
            OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

            Operand n = GetVec(op.Rn);
            Operand m = GetVec(op.Rm);

            if ((op.Size & 1) != 0)
            {
                inst |= Intrinsic.Arm64VDouble;
            }

            context.Copy(GetVec(op.Rd), context.AddIntrinsic(inst, n, m));
        }

        public static void EmitScalarBinaryOpFByElem(ArmEmitterContext context, Intrinsic inst)
        {
            OpCodeSimdRegElemF op = (OpCodeSimdRegElemF)context.CurrOp;

            Operand n = GetVec(op.Rn);
            Operand m = GetVec(op.Rm);

            if ((op.Size & 1) != 0)
            {
                inst |= Intrinsic.Arm64VDouble;
            }

            context.Copy(GetVec(op.Rd), context.AddIntrinsic(inst, n, m, Const(op.Index)));
        }

        public static void EmitScalarTernaryOpF(ArmEmitterContext context, Intrinsic inst)
        {
            OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

            Operand n = GetVec(op.Rn);
            Operand m = GetVec(op.Rm);
            Operand a = GetVec(op.Ra);

            if ((op.Size & 1) != 0)
            {
                inst |= Intrinsic.Arm64VDouble;
            }

            context.Copy(GetVec(op.Rd), context.AddIntrinsic(inst, a, n, m));
        }

        public static void EmitScalarTernaryOpFRdByElem(ArmEmitterContext context, Intrinsic inst)
        {
            OpCodeSimdRegElemF op = (OpCodeSimdRegElemF)context.CurrOp;

            Operand d = GetVec(op.Rd);
            Operand n = GetVec(op.Rn);
            Operand m = GetVec(op.Rm);

            if ((op.Size & 1) != 0)
            {
                inst |= Intrinsic.Arm64VDouble;
            }

            context.Copy(d, context.AddIntrinsic(inst, d, n, m, Const(op.Index)));
        }

        public static void EmitScalarUnaryOp(ArmEmitterContext context, Intrinsic inst)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            Operand n = GetVec(op.Rn);

            inst |= (Intrinsic)(op.Size << (int)Intrinsic.Arm64VSizeShift);

            context.Copy(GetVec(op.Rd), context.AddIntrinsic(inst, n));
        }

        public static void EmitScalarBinaryOp(ArmEmitterContext context, Intrinsic inst)
        {
            OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

            Operand n = GetVec(op.Rn);
            Operand m = GetVec(op.Rm);

            inst |= (Intrinsic)(op.Size << (int)Intrinsic.Arm64VSizeShift);

            context.Copy(GetVec(op.Rd), context.AddIntrinsic(inst, n, m));
        }

        public static void EmitScalarBinaryOpRd(ArmEmitterContext context, Intrinsic inst)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            Operand d = GetVec(op.Rd);
            Operand n = GetVec(op.Rn);

            inst |= (Intrinsic)(op.Size << (int)Intrinsic.Arm64VSizeShift);

            context.Copy(GetVec(op.Rd), context.AddIntrinsic(inst, d, n));
        }

        public static void EmitScalarTernaryOpRd(ArmEmitterContext context, Intrinsic inst)
        {
            OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

            Operand d = GetVec(op.Rd);
            Operand n = GetVec(op.Rn);
            Operand m = GetVec(op.Rm);

            inst |= (Intrinsic)(op.Size << (int)Intrinsic.Arm64VSizeShift);

            context.Copy(d, context.AddIntrinsic(inst, d, n, m));
        }

        public static void EmitScalarShiftBinaryOp(ArmEmitterContext context, Intrinsic inst, int shift)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            Operand n = GetVec(op.Rn);

            inst |= (Intrinsic)(op.Size << (int)Intrinsic.Arm64VSizeShift);

            context.Copy(GetVec(op.Rd), context.AddIntrinsic(inst, n, Const(shift)));
        }

        public static void EmitScalarShiftTernaryOpRd(ArmEmitterContext context, Intrinsic inst, int shift)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            Operand d = GetVec(op.Rd);
            Operand n = GetVec(op.Rn);

            inst |= (Intrinsic)(op.Size << (int)Intrinsic.Arm64VSizeShift);

            context.Copy(GetVec(op.Rd), context.AddIntrinsic(inst, d, n, Const(shift)));
        }

        public static void EmitScalarSaturatingShiftTernaryOpRd(ArmEmitterContext context, Intrinsic inst, int shift)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            Operand d = GetVec(op.Rd);
            Operand n = GetVec(op.Rn);

            inst |= (Intrinsic)(op.Size << (int)Intrinsic.Arm64VSizeShift);

            context.Copy(GetVec(op.Rd), context.AddIntrinsic(inst, d, n, Const(shift)));

            context.SetPendingQcFlagSync();
        }

        public static void EmitScalarSaturatingUnaryOp(ArmEmitterContext context, Intrinsic inst)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            Operand n = GetVec(op.Rn);

            inst |= (Intrinsic)(op.Size << (int)Intrinsic.Arm64VSizeShift);

            Operand result = context.AddIntrinsic(inst, n);

            context.Copy(GetVec(op.Rd), result);

            context.SetPendingQcFlagSync();
        }

        public static void EmitScalarSaturatingBinaryOp(ArmEmitterContext context, Intrinsic inst)
        {
            OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

            Operand n = GetVec(op.Rn);
            Operand m = GetVec(op.Rm);

            inst |= (Intrinsic)(op.Size << (int)Intrinsic.Arm64VSizeShift);

            Operand result = context.AddIntrinsic(inst, n, m);

            context.Copy(GetVec(op.Rd), result);

            context.SetPendingQcFlagSync();
        }

        public static void EmitScalarSaturatingBinaryOpRd(ArmEmitterContext context, Intrinsic inst)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            Operand d = GetVec(op.Rd);
            Operand n = GetVec(op.Rn);

            inst |= (Intrinsic)(op.Size << (int)Intrinsic.Arm64VSizeShift);

            Operand result = context.AddIntrinsic(inst, d, n);

            context.Copy(GetVec(op.Rd), result);

            context.SetPendingQcFlagSync();
        }

        public static void EmitScalarConvertBinaryOpF(ArmEmitterContext context, Intrinsic inst, int fBits)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            Operand n = GetVec(op.Rn);

            if ((op.Size & 1) != 0)
            {
                inst |= Intrinsic.Arm64VDouble;
            }

            context.Copy(GetVec(op.Rd), context.AddIntrinsic(inst, n, Const(fBits)));
        }

        public static void EmitScalarConvertBinaryOpFFromGp(ArmEmitterContext context, Intrinsic inst, int fBits)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            Operand n = GetIntOrZR(context, op.Rn);

            if ((op.Size & 1) != 0)
            {
                inst |= Intrinsic.Arm64VDouble;
            }

            context.Copy(GetVec(op.Rd), context.AddIntrinsic(inst, n, Const(fBits)));
        }

        public static void EmitScalarConvertBinaryOpFToGp(ArmEmitterContext context, Intrinsic inst, int fBits)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            Operand n = GetVec(op.Rn);

            if ((op.Size & 1) != 0)
            {
                inst |= Intrinsic.Arm64VDouble;
            }

            SetIntOrZR(context, op.Rd, op.RegisterSize == RegisterSize.Int32
                ? context.AddIntrinsicInt(inst, n, Const(fBits))
                : context.AddIntrinsicLong(inst, n, Const(fBits)));
        }

        public static void EmitVectorUnaryOpF(ArmEmitterContext context, Intrinsic inst)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            Operand n = GetVec(op.Rn);

            if ((op.Size & 1) != 0)
            {
                inst |= Intrinsic.Arm64VDouble;
            }

            if (op.RegisterSize == RegisterSize.Simd128)
            {
                inst |= Intrinsic.Arm64V128;
            }

            context.Copy(GetVec(op.Rd), context.AddIntrinsic(inst, n));
        }

        public static void EmitVectorBinaryOpF(ArmEmitterContext context, Intrinsic inst)
        {
            OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

            Operand n = GetVec(op.Rn);
            Operand m = GetVec(op.Rm);

            if ((op.Size & 1) != 0)
            {
                inst |= Intrinsic.Arm64VDouble;
            }

            if (op.RegisterSize == RegisterSize.Simd128)
            {
                inst |= Intrinsic.Arm64V128;
            }

            context.Copy(GetVec(op.Rd), context.AddIntrinsic(inst, n, m));
        }

        public static void EmitVectorBinaryOpFRd(ArmEmitterContext context, Intrinsic inst)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            Operand d = GetVec(op.Rd);
            Operand n = GetVec(op.Rn);

            if ((op.Size & 1) != 0)
            {
                inst |= Intrinsic.Arm64VDouble;
            }

            if (op.RegisterSize == RegisterSize.Simd128)
            {
                inst |= Intrinsic.Arm64V128;
            }

            context.Copy(GetVec(op.Rd), context.AddIntrinsic(inst, d, n));
        }

        public static void EmitVectorBinaryOpFByElem(ArmEmitterContext context, Intrinsic inst)
        {
            OpCodeSimdRegElemF op = (OpCodeSimdRegElemF)context.CurrOp;

            Operand n = GetVec(op.Rn);
            Operand m = GetVec(op.Rm);

            if ((op.Size & 1) != 0)
            {
                inst |= Intrinsic.Arm64VDouble;
            }

            if (op.RegisterSize == RegisterSize.Simd128)
            {
                inst |= Intrinsic.Arm64V128;
            }

            context.Copy(GetVec(op.Rd), context.AddIntrinsic(inst, n, m, Const(op.Index)));
        }

        public static void EmitVectorTernaryOpFRd(ArmEmitterContext context, Intrinsic inst)
        {
            OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

            Operand d = GetVec(op.Rd);
            Operand n = GetVec(op.Rn);
            Operand m = GetVec(op.Rm);

            if ((op.Size & 1) != 0)
            {
                inst |= Intrinsic.Arm64VDouble;
            }

            if (op.RegisterSize == RegisterSize.Simd128)
            {
                inst |= Intrinsic.Arm64V128;
            }

            context.Copy(d, context.AddIntrinsic(inst, d, n, m));
        }

        public static void EmitVectorTernaryOpFRdByElem(ArmEmitterContext context, Intrinsic inst)
        {
            OpCodeSimdRegElemF op = (OpCodeSimdRegElemF)context.CurrOp;

            Operand d = GetVec(op.Rd);
            Operand n = GetVec(op.Rn);
            Operand m = GetVec(op.Rm);

            if ((op.Size & 1) != 0)
            {
                inst |= Intrinsic.Arm64VDouble;
            }

            if (op.RegisterSize == RegisterSize.Simd128)
            {
                inst |= Intrinsic.Arm64V128;
            }

            context.Copy(d, context.AddIntrinsic(inst, d, n, m, Const(op.Index)));
        }

        public static void EmitVectorUnaryOp(ArmEmitterContext context, Intrinsic inst)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            Operand n = GetVec(op.Rn);

            inst |= (Intrinsic)(op.Size << (int)Intrinsic.Arm64VSizeShift);

            if (op.RegisterSize == RegisterSize.Simd128)
            {
                inst |= Intrinsic.Arm64V128;
            }

            context.Copy(GetVec(op.Rd), context.AddIntrinsic(inst, n));
        }

        public static void EmitVectorBinaryOp(ArmEmitterContext context, Intrinsic inst)
        {
            OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

            Operand n = GetVec(op.Rn);
            Operand m = GetVec(op.Rm);

            inst |= (Intrinsic)(op.Size << (int)Intrinsic.Arm64VSizeShift);

            if (op.RegisterSize == RegisterSize.Simd128)
            {
                inst |= Intrinsic.Arm64V128;
            }

            context.Copy(GetVec(op.Rd), context.AddIntrinsic(inst, n, m));
        }

        public static void EmitVectorBinaryOpRd(ArmEmitterContext context, Intrinsic inst)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            Operand d = GetVec(op.Rd);
            Operand n = GetVec(op.Rn);

            inst |= (Intrinsic)(op.Size << (int)Intrinsic.Arm64VSizeShift);

            if (op.RegisterSize == RegisterSize.Simd128)
            {
                inst |= Intrinsic.Arm64V128;
            }

            context.Copy(GetVec(op.Rd), context.AddIntrinsic(inst, d, n));
        }

        public static void EmitVectorBinaryOpByElem(ArmEmitterContext context, Intrinsic inst)
        {
            OpCodeSimdRegElem op = (OpCodeSimdRegElem)context.CurrOp;

            Operand n = GetVec(op.Rn);
            Operand m = GetVec(op.Rm);

            inst |= (Intrinsic)(op.Size << (int)Intrinsic.Arm64VSizeShift);

            if (op.RegisterSize == RegisterSize.Simd128)
            {
                inst |= Intrinsic.Arm64V128;
            }

            context.Copy(GetVec(op.Rd), context.AddIntrinsic(inst, n, m, Const(op.Index)));
        }

        public static void EmitVectorTernaryOpRd(ArmEmitterContext context, Intrinsic inst)
        {
            OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

            Operand d = GetVec(op.Rd);
            Operand n = GetVec(op.Rn);
            Operand m = GetVec(op.Rm);

            inst |= (Intrinsic)(op.Size << (int)Intrinsic.Arm64VSizeShift);

            if (op.RegisterSize == RegisterSize.Simd128)
            {
                inst |= Intrinsic.Arm64V128;
            }

            context.Copy(d, context.AddIntrinsic(inst, d, n, m));
        }

        public static void EmitVectorTernaryOpRdByElem(ArmEmitterContext context, Intrinsic inst)
        {
            OpCodeSimdRegElem op = (OpCodeSimdRegElem)context.CurrOp;

            Operand d = GetVec(op.Rd);
            Operand n = GetVec(op.Rn);
            Operand m = GetVec(op.Rm);

            inst |= (Intrinsic)(op.Size << (int)Intrinsic.Arm64VSizeShift);

            if (op.RegisterSize == RegisterSize.Simd128)
            {
                inst |= Intrinsic.Arm64V128;
            }

            context.Copy(d, context.AddIntrinsic(inst, d, n, m, Const(op.Index)));
        }

        public static void EmitVectorShiftBinaryOp(ArmEmitterContext context, Intrinsic inst, int shift)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            Operand n = GetVec(op.Rn);

            inst |= (Intrinsic)(op.Size << (int)Intrinsic.Arm64VSizeShift);

            if (op.RegisterSize == RegisterSize.Simd128)
            {
                inst |= Intrinsic.Arm64V128;
            }

            context.Copy(GetVec(op.Rd), context.AddIntrinsic(inst, n, Const(shift)));
        }

        public static void EmitVectorShiftTernaryOpRd(ArmEmitterContext context, Intrinsic inst, int shift)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            Operand d = GetVec(op.Rd);
            Operand n = GetVec(op.Rn);

            inst |= (Intrinsic)(op.Size << (int)Intrinsic.Arm64VSizeShift);

            if (op.RegisterSize == RegisterSize.Simd128)
            {
                inst |= Intrinsic.Arm64V128;
            }

            context.Copy(GetVec(op.Rd), context.AddIntrinsic(inst, d, n, Const(shift)));
        }

        public static void EmitVectorSaturatingShiftTernaryOpRd(ArmEmitterContext context, Intrinsic inst, int shift)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            Operand d = GetVec(op.Rd);
            Operand n = GetVec(op.Rn);

            inst |= (Intrinsic)(op.Size << (int)Intrinsic.Arm64VSizeShift);

            if (op.RegisterSize == RegisterSize.Simd128)
            {
                inst |= Intrinsic.Arm64V128;
            }

            context.Copy(GetVec(op.Rd), context.AddIntrinsic(inst, d, n, Const(shift)));

            context.SetPendingQcFlagSync();
        }

        public static void EmitVectorSaturatingUnaryOp(ArmEmitterContext context, Intrinsic inst)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            Operand n = GetVec(op.Rn);

            inst |= (Intrinsic)(op.Size << (int)Intrinsic.Arm64VSizeShift);

            if (op.RegisterSize == RegisterSize.Simd128)
            {
                inst |= Intrinsic.Arm64V128;
            }

            Operand result = context.AddIntrinsic(inst, n);

            context.Copy(GetVec(op.Rd), result);

            context.SetPendingQcFlagSync();
        }

        public static void EmitVectorSaturatingBinaryOp(ArmEmitterContext context, Intrinsic inst)
        {
            OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

            Operand n = GetVec(op.Rn);
            Operand m = GetVec(op.Rm);

            inst |= (Intrinsic)(op.Size << (int)Intrinsic.Arm64VSizeShift);

            if (op.RegisterSize == RegisterSize.Simd128)
            {
                inst |= Intrinsic.Arm64V128;
            }

            Operand result = context.AddIntrinsic(inst, n, m);

            context.Copy(GetVec(op.Rd), result);

            context.SetPendingQcFlagSync();
        }

        public static void EmitVectorSaturatingBinaryOpRd(ArmEmitterContext context, Intrinsic inst)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            Operand d = GetVec(op.Rd);
            Operand n = GetVec(op.Rn);

            inst |= (Intrinsic)(op.Size << (int)Intrinsic.Arm64VSizeShift);

            if (op.RegisterSize == RegisterSize.Simd128)
            {
                inst |= Intrinsic.Arm64V128;
            }

            Operand result = context.AddIntrinsic(inst, d, n);

            context.Copy(GetVec(op.Rd), result);

            context.SetPendingQcFlagSync();
        }

        public static void EmitVectorSaturatingBinaryOpByElem(ArmEmitterContext context, Intrinsic inst)
        {
            OpCodeSimdRegElem op = (OpCodeSimdRegElem)context.CurrOp;

            Operand n = GetVec(op.Rn);
            Operand m = GetVec(op.Rm);

            inst |= (Intrinsic)(op.Size << (int)Intrinsic.Arm64VSizeShift);

            if (op.RegisterSize == RegisterSize.Simd128)
            {
                inst |= Intrinsic.Arm64V128;
            }

            Operand result = context.AddIntrinsic(inst, n, m, Const(op.Index));

            context.Copy(GetVec(op.Rd), result);

            context.SetPendingQcFlagSync();
        }

        public static void EmitVectorConvertBinaryOpF(ArmEmitterContext context, Intrinsic inst, int fBits)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            Operand n = GetVec(op.Rn);

            if ((op.Size & 1) != 0)
            {
                inst |= Intrinsic.Arm64VDouble;
            }

            if (op.RegisterSize == RegisterSize.Simd128)
            {
                inst |= Intrinsic.Arm64V128;
            }

            context.Copy(GetVec(op.Rd), context.AddIntrinsic(inst, n, Const(fBits)));
        }

        public static void EmitVectorLookupTable(ArmEmitterContext context, Intrinsic inst)
        {
            OpCodeSimdTbl op = (OpCodeSimdTbl)context.CurrOp;

            Operand[] operands = new Operand[op.Size + 1];

            operands[op.Size] = GetVec(op.Rm);

            for (int index = 0; index < op.Size; index++)
            {
                operands[index] = GetVec((op.Rn + index) & 0x1F);
            }

            if (op.RegisterSize == RegisterSize.Simd128)
            {
                inst |= Intrinsic.Arm64V128;
            }

            context.Copy(GetVec(op.Rd), context.AddIntrinsic(inst, operands));
        }

        public static void EmitFcmpOrFcmpe(ArmEmitterContext context, bool signalNaNs)
        {
            OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

            bool cmpWithZero = op is not OpCodeSimdFcond && op.Bit3;

            Intrinsic inst = signalNaNs ? Intrinsic.Arm64FcmpeS : Intrinsic.Arm64FcmpS;

            if ((op.Size & 1) != 0)
            {
                inst |= Intrinsic.Arm64VDouble;
            }

            Operand n = GetVec(op.Rn);
            Operand m = cmpWithZero ? Const(0) : GetVec(op.Rm);

            Operand nzcv = context.AddIntrinsicInt(inst, n, m);

            Operand one = Const(1);

            SetFlag(context, PState.VFlag, context.BitwiseAnd(context.ShiftRightUI(nzcv, Const(28)), one));
            SetFlag(context, PState.CFlag, context.BitwiseAnd(context.ShiftRightUI(nzcv, Const(29)), one));
            SetFlag(context, PState.ZFlag, context.BitwiseAnd(context.ShiftRightUI(nzcv, Const(30)), one));
            SetFlag(context, PState.NFlag, context.BitwiseAnd(context.ShiftRightUI(nzcv, Const(31)), one));
        }
    }
}
