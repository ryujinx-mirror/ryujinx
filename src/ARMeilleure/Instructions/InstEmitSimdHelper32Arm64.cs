using ARMeilleure.Decoders;
using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.State;
using ARMeilleure.Translation;
using System;
using System.Diagnostics;
using static ARMeilleure.Instructions.InstEmitHelper;
using static ARMeilleure.Instructions.InstEmitSimdHelper;
using static ARMeilleure.IntermediateRepresentation.Operand.Factory;

namespace ARMeilleure.Instructions
{
    using Func1I = Func<Operand, Operand>;
    using Func2I = Func<Operand, Operand, Operand>;
    using Func3I = Func<Operand, Operand, Operand, Operand>;

    static class InstEmitSimdHelper32Arm64
    {
        // Intrinsic Helpers

        public static Operand EmitMoveDoubleWordToSide(ArmEmitterContext context, Operand input, int originalV, int targetV)
        {
            Debug.Assert(input.Type == OperandType.V128);

            int originalSide = originalV & 1;
            int targetSide = targetV & 1;

            if (originalSide == targetSide)
            {
                return input;
            }

            Intrinsic vType = Intrinsic.Arm64VDWord | Intrinsic.Arm64V128;

            if (targetSide == 1)
            {
                return context.AddIntrinsic(Intrinsic.Arm64DupVe | vType, input, Const(OperandType.I32, 0)); // Low to high.
            }
            else
            {
                return context.AddIntrinsic(Intrinsic.Arm64DupVe | vType, input, Const(OperandType.I32, 1)); // High to low.
            }
        }

        public static Operand EmitDoubleWordInsert(ArmEmitterContext context, Operand target, Operand value, int targetV)
        {
            Debug.Assert(target.Type == OperandType.V128 && value.Type == OperandType.V128);

            int targetSide = targetV & 1;
            Operand idx = Const(targetSide);

            return context.AddIntrinsic(Intrinsic.Arm64InsVe | Intrinsic.Arm64VDWord, target, idx, value, idx);
        }

        public static Operand EmitScalarInsert(ArmEmitterContext context, Operand target, Operand value, int reg, bool doubleWidth)
        {
            Debug.Assert(target.Type == OperandType.V128 && value.Type == OperandType.V128);

            // Insert from index 0 in value to index in target.
            int index = reg & (doubleWidth ? 1 : 3);

            if (doubleWidth)
            {
                return context.AddIntrinsic(Intrinsic.Arm64InsVe | Intrinsic.Arm64VDWord, target, Const(index), value, Const(0));
            }
            else
            {
                return context.AddIntrinsic(Intrinsic.Arm64InsVe | Intrinsic.Arm64VWord, target, Const(index), value, Const(0));
            }
        }

        public static Operand EmitExtractScalar(ArmEmitterContext context, Operand target, int reg, bool doubleWidth)
        {
            int index = reg & (doubleWidth ? 1 : 3);
            if (index == 0)
            {
                return target; // Element is already at index 0, so just return the vector directly.
            }

            if (doubleWidth)
            {
                return context.AddIntrinsic(Intrinsic.Arm64DupSe | Intrinsic.Arm64VDWord, target, Const(1)); // Extract high (index 1).
            }
            else
            {
                return context.AddIntrinsic(Intrinsic.Arm64DupSe | Intrinsic.Arm64VWord, target, Const(index)); // Extract element at index.
            }
        }

        // Vector Operand Templates

        public static void EmitVectorUnaryOpSimd32(ArmEmitterContext context, Func1I vectorFunc)
        {
            OpCode32Simd op = (OpCode32Simd)context.CurrOp;

            Operand m = GetVecA32(op.Qm);
            Operand d = GetVecA32(op.Qd);

            if (!op.Q) // Register swap: move relevant doubleword to destination side.
            {
                m = EmitMoveDoubleWordToSide(context, m, op.Vm, op.Vd);
            }

            Operand res = vectorFunc(m);

            if (!op.Q) // Register insert.
            {
                res = EmitDoubleWordInsert(context, d, res, op.Vd);
            }

            context.Copy(d, res);
        }

        public static void EmitVectorUnaryOpF32(ArmEmitterContext context, Intrinsic inst)
        {
            OpCode32Simd op = (OpCode32Simd)context.CurrOp;

            inst |= ((op.Size & 1) != 0 ? Intrinsic.Arm64VDouble : Intrinsic.Arm64VFloat) | Intrinsic.Arm64V128;
            EmitVectorUnaryOpSimd32(context, (m) => context.AddIntrinsic(inst, m));
        }

        public static void EmitVectorBinaryOpSimd32(ArmEmitterContext context, Func2I vectorFunc, int side = -1)
        {
            OpCode32SimdReg op = (OpCode32SimdReg)context.CurrOp;

            Operand n = GetVecA32(op.Qn);
            Operand m = GetVecA32(op.Qm);
            Operand d = GetVecA32(op.Qd);

            if (side == -1)
            {
                side = op.Vd;
            }

            if (!op.Q) // Register swap: move relevant doubleword to destination side.
            {
                n = EmitMoveDoubleWordToSide(context, n, op.Vn, side);
                m = EmitMoveDoubleWordToSide(context, m, op.Vm, side);
            }

            Operand res = vectorFunc(n, m);

            if (!op.Q) // Register insert.
            {
                if (side != op.Vd)
                {
                    res = EmitMoveDoubleWordToSide(context, res, side, op.Vd);
                }
                res = EmitDoubleWordInsert(context, d, res, op.Vd);
            }

            context.Copy(d, res);
        }

        public static void EmitVectorBinaryOpF32(ArmEmitterContext context, Intrinsic inst)
        {
            OpCode32SimdReg op = (OpCode32SimdReg)context.CurrOp;

            inst |= ((op.Size & 1) != 0 ? Intrinsic.Arm64VDouble : Intrinsic.Arm64VFloat) | Intrinsic.Arm64V128;
            EmitVectorBinaryOpSimd32(context, (n, m) => context.AddIntrinsic(inst, n, m));
        }

        public static void EmitVectorTernaryOpSimd32(ArmEmitterContext context, Func3I vectorFunc)
        {
            OpCode32SimdReg op = (OpCode32SimdReg)context.CurrOp;

            Operand n = GetVecA32(op.Qn);
            Operand m = GetVecA32(op.Qm);
            Operand d = GetVecA32(op.Qd);
            Operand initialD = d;

            if (!op.Q) // Register swap: move relevant doubleword to destination side.
            {
                n = EmitMoveDoubleWordToSide(context, n, op.Vn, op.Vd);
                m = EmitMoveDoubleWordToSide(context, m, op.Vm, op.Vd);
            }

            Operand res = vectorFunc(d, n, m);

            if (!op.Q) // Register insert.
            {
                res = EmitDoubleWordInsert(context, initialD, res, op.Vd);
            }

            context.Copy(initialD, res);
        }

        public static void EmitVectorTernaryOpF32(ArmEmitterContext context, Intrinsic inst)
        {
            OpCode32SimdReg op = (OpCode32SimdReg)context.CurrOp;

            inst |= ((op.Size & 1) != 0 ? Intrinsic.Arm64VDouble : Intrinsic.Arm64VFloat) | Intrinsic.Arm64V128;
            EmitVectorTernaryOpSimd32(context, (d, n, m) => context.AddIntrinsic(inst, d, n, m));
        }

        public static void EmitScalarUnaryOpSimd32(ArmEmitterContext context, Func1I scalarFunc, bool doubleSize)
        {
            OpCode32SimdS op = (OpCode32SimdS)context.CurrOp;

            int shift = doubleSize ? 1 : 2;
            Operand m = GetVecA32(op.Vm >> shift);
            Operand d = GetVecA32(op.Vd >> shift);

            m = EmitExtractScalar(context, m, op.Vm, doubleSize);

            Operand res = scalarFunc(m);

            // Insert scalar into vector.
            res = EmitScalarInsert(context, d, res, op.Vd, doubleSize);

            context.Copy(d, res);
        }

        public static void EmitScalarUnaryOpF32(ArmEmitterContext context, Intrinsic inst)
        {
            OpCode32SimdS op = (OpCode32SimdS)context.CurrOp;

            EmitScalarUnaryOpF32(context, inst, (op.Size & 1) != 0);
        }

        public static void EmitScalarUnaryOpF32(ArmEmitterContext context, Intrinsic inst, bool doubleSize)
        {
            inst |= (doubleSize ? Intrinsic.Arm64VDouble : Intrinsic.Arm64VFloat) | Intrinsic.Arm64V128;
            EmitScalarUnaryOpSimd32(context, (m) => (inst == 0) ? m : context.AddIntrinsic(inst, m), doubleSize);
        }

        public static void EmitScalarBinaryOpSimd32(ArmEmitterContext context, Func2I scalarFunc)
        {
            OpCode32SimdRegS op = (OpCode32SimdRegS)context.CurrOp;

            bool doubleSize = (op.Size & 1) != 0;
            int shift = doubleSize ? 1 : 2;
            Operand n = GetVecA32(op.Vn >> shift);
            Operand m = GetVecA32(op.Vm >> shift);
            Operand d = GetVecA32(op.Vd >> shift);

            n = EmitExtractScalar(context, n, op.Vn, doubleSize);
            m = EmitExtractScalar(context, m, op.Vm, doubleSize);

            Operand res = scalarFunc(n, m);

            // Insert scalar into vector.
            res = EmitScalarInsert(context, d, res, op.Vd, doubleSize);

            context.Copy(d, res);
        }

        public static void EmitScalarBinaryOpF32(ArmEmitterContext context, Intrinsic inst)
        {
            OpCode32SimdRegS op = (OpCode32SimdRegS)context.CurrOp;

            inst |= ((op.Size & 1) != 0 ? Intrinsic.Arm64VDouble : Intrinsic.Arm64VFloat) | Intrinsic.Arm64V128;
            EmitScalarBinaryOpSimd32(context, (n, m) => context.AddIntrinsic(inst, n, m));
        }

        public static void EmitScalarTernaryOpSimd32(ArmEmitterContext context, Func3I scalarFunc)
        {
            OpCode32SimdRegS op = (OpCode32SimdRegS)context.CurrOp;

            bool doubleSize = (op.Size & 1) != 0;
            int shift = doubleSize ? 1 : 2;
            Operand n = GetVecA32(op.Vn >> shift);
            Operand m = GetVecA32(op.Vm >> shift);
            Operand d = GetVecA32(op.Vd >> shift);
            Operand initialD = d;

            n = EmitExtractScalar(context, n, op.Vn, doubleSize);
            m = EmitExtractScalar(context, m, op.Vm, doubleSize);
            d = EmitExtractScalar(context, d, op.Vd, doubleSize);

            Operand res = scalarFunc(d, n, m);

            // Insert scalar into vector.
            res = EmitScalarInsert(context, initialD, res, op.Vd, doubleSize);

            context.Copy(initialD, res);
        }

        public static void EmitScalarTernaryOpF32(ArmEmitterContext context, Intrinsic inst)
        {
            OpCode32SimdRegS op = (OpCode32SimdRegS)context.CurrOp;

            inst |= ((op.Size & 1) != 0 ? Intrinsic.Arm64VDouble : Intrinsic.Arm64VFloat) | Intrinsic.Arm64V128;
            EmitScalarTernaryOpSimd32(context, (d, n, m) => context.AddIntrinsic(inst, d, n, m));
        }

        // Pairwise

        public static void EmitVectorPairwiseOpF32(ArmEmitterContext context, Intrinsic inst32)
        {
            OpCode32SimdReg op = (OpCode32SimdReg)context.CurrOp;

            inst32 |= Intrinsic.Arm64V64 | Intrinsic.Arm64VFloat;
            EmitVectorBinaryOpSimd32(context, (n, m) => context.AddIntrinsic(inst32, n, m), 0);
        }

        public static void EmitVcmpOrVcmpe(ArmEmitterContext context, bool signalNaNs)
        {
            OpCode32SimdS op = (OpCode32SimdS)context.CurrOp;

            bool cmpWithZero = (op.Opc & 2) != 0;

            Intrinsic inst = signalNaNs ? Intrinsic.Arm64FcmpeS : Intrinsic.Arm64FcmpS;
            inst |= ((op.Size & 1) != 0 ? Intrinsic.Arm64VDouble : Intrinsic.Arm64VFloat) | Intrinsic.Arm64V128;

            bool doubleSize = (op.Size & 1) != 0;
            int shift = doubleSize ? 1 : 2;
            Operand n = GetVecA32(op.Vd >> shift);
            Operand m = GetVecA32(op.Vm >> shift);

            n = EmitExtractScalar(context, n, op.Vd, doubleSize);
            m = cmpWithZero ? Const(0) : EmitExtractScalar(context, m, op.Vm, doubleSize);

            Operand nzcv = context.AddIntrinsicInt(inst, n, m);

            Operand one = Const(1);

            SetFpFlag(context, FPState.VFlag, context.BitwiseAnd(context.ShiftRightUI(nzcv, Const(28)), one));
            SetFpFlag(context, FPState.CFlag, context.BitwiseAnd(context.ShiftRightUI(nzcv, Const(29)), one));
            SetFpFlag(context, FPState.ZFlag, context.BitwiseAnd(context.ShiftRightUI(nzcv, Const(30)), one));
            SetFpFlag(context, FPState.NFlag, context.BitwiseAnd(context.ShiftRightUI(nzcv, Const(31)), one));
        }

        public static void EmitCmpOpF32(ArmEmitterContext context, CmpCondition cond, bool zero)
        {
            OpCode32Simd op = (OpCode32Simd)context.CurrOp;

            int sizeF = op.Size & 1;

            Intrinsic inst;
            if (zero)
            {
                inst = cond switch
                {
                    CmpCondition.Equal => Intrinsic.Arm64FcmeqVz,
                    CmpCondition.GreaterThan => Intrinsic.Arm64FcmgtVz,
                    CmpCondition.GreaterThanOrEqual => Intrinsic.Arm64FcmgeVz,
                    CmpCondition.LessThan => Intrinsic.Arm64FcmltVz,
                    CmpCondition.LessThanOrEqual => Intrinsic.Arm64FcmleVz,
                    _ => throw new InvalidOperationException(),
                };
            }
            else
            {
                inst = cond switch
                {
                    CmpCondition.Equal => Intrinsic.Arm64FcmeqV,
                    CmpCondition.GreaterThan => Intrinsic.Arm64FcmgtV,
                    CmpCondition.GreaterThanOrEqual => Intrinsic.Arm64FcmgeV,
                    _ => throw new InvalidOperationException(),
                };
            }

            inst |= (sizeF != 0 ? Intrinsic.Arm64VDouble : Intrinsic.Arm64VFloat) | Intrinsic.Arm64V128;

            if (zero)
            {
                EmitVectorUnaryOpSimd32(context, (m) =>
                {
                    return context.AddIntrinsic(inst, m);
                });
            }
            else
            {
                EmitVectorBinaryOpSimd32(context, (n, m) =>
                {
                    return context.AddIntrinsic(inst, n, m);
                });
            }
        }
    }
}
