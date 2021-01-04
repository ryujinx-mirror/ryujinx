using ARMeilleure.Decoders;
using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.Translation;
using System;
using System.Diagnostics;
using System.Reflection;

using static ARMeilleure.Instructions.InstEmitHelper;
using static ARMeilleure.Instructions.InstEmitSimdHelper;
using static ARMeilleure.IntermediateRepresentation.OperandHelper;

namespace ARMeilleure.Instructions
{
    using Func1I = Func<Operand, Operand>;
    using Func2I = Func<Operand, Operand, Operand>;
    using Func3I = Func<Operand, Operand, Operand, Operand>;

    static class InstEmitSimdHelper32
    {
        public static (int, int) GetQuadwordAndSubindex(int index, RegisterSize size)
        {
            switch (size)
            {
                case RegisterSize.Simd128:
                    return (index >> 1, 0);
                case RegisterSize.Simd64:
                case RegisterSize.Int64:
                    return (index >> 1, index & 1);
                case RegisterSize.Int32:
                    return (index >> 2, index & 3);
            }

            throw new ArgumentException("Unrecognized Vector Register Size.");
        }

        public static Operand ExtractScalar(ArmEmitterContext context, OperandType type, int reg)
        {
            Debug.Assert(type != OperandType.V128);

            if (type == OperandType.FP64 || type == OperandType.I64)
            {
                // From dreg.
                return context.VectorExtract(type, GetVecA32(reg >> 1), reg & 1);
            }
            else
            {
                // From sreg.
                return context.VectorExtract(type, GetVecA32(reg >> 2), reg & 3);
            }
        }

        public static void InsertScalar(ArmEmitterContext context, int reg, Operand value)
        {
            Debug.Assert(value.Type != OperandType.V128);

            Operand vec, insert;
            if (value.Type == OperandType.FP64 || value.Type == OperandType.I64)
            {
                // From dreg.
                vec = GetVecA32(reg >> 1);
                insert = context.VectorInsert(vec, value, reg & 1);
            }
            else
            {
                // From sreg.
                vec = GetVecA32(reg >> 2);
                insert = context.VectorInsert(vec, value, reg & 3);
            }

            context.Copy(vec, insert);
        }

        public static Operand ExtractElement(ArmEmitterContext context, int reg, int size, bool signed)
        {
            return EmitVectorExtract32(context, reg >> (4 - size), reg & ((16 >> size) - 1), size, signed);
        }

        public static void EmitVectorImmUnaryOp32(ArmEmitterContext context, Func1I emit)
        {
            IOpCode32SimdImm op = (IOpCode32SimdImm)context.CurrOp;

            Operand imm = Const(op.Immediate);

            int elems = op.Elems;
            (int index, int subIndex) = GetQuadwordAndSubindex(op.Vd, op.RegisterSize);

            Operand vec = GetVecA32(index);
            Operand res = vec;

            for (int item = 0; item < elems; item++)
            {
                res = EmitVectorInsert(context, res, emit(imm), item + subIndex * elems, op.Size);
            }

            context.Copy(vec, res);
        }

        public static void EmitScalarUnaryOpF32(ArmEmitterContext context, Func1I emit)
        {
            OpCode32SimdS op = (OpCode32SimdS)context.CurrOp;

            OperandType type = (op.Size & 1) != 0 ? OperandType.FP64 : OperandType.FP32;

            Operand m = ExtractScalar(context, type, op.Vm);

            InsertScalar(context, op.Vd, emit(m));
        }

        public static void EmitScalarBinaryOpF32(ArmEmitterContext context, Func2I emit)
        {
            OpCode32SimdRegS op = (OpCode32SimdRegS)context.CurrOp;

            OperandType type = (op.Size & 1) != 0 ? OperandType.FP64 : OperandType.FP32;

            Operand n = ExtractScalar(context, type, op.Vn);
            Operand m = ExtractScalar(context, type, op.Vm);

            InsertScalar(context, op.Vd, emit(n, m));
        }

        public static void EmitScalarBinaryOpI32(ArmEmitterContext context, Func2I emit)
        {
            OpCode32SimdRegS op = (OpCode32SimdRegS)context.CurrOp;

            OperandType type = (op.Size & 1) != 0 ? OperandType.I64 : OperandType.I32;

            if (op.Size < 2)
            {
                throw new NotSupportedException("Cannot perform a scalar SIMD operation on integers smaller than 32 bits.");
            }

            Operand n = ExtractScalar(context, type, op.Vn);
            Operand m = ExtractScalar(context, type, op.Vm);

            InsertScalar(context, op.Vd, emit(n, m));
        }

        public static void EmitScalarTernaryOpF32(ArmEmitterContext context, Func3I emit)
        {
            OpCode32SimdRegS op = (OpCode32SimdRegS)context.CurrOp;

            OperandType type = (op.Size & 1) != 0 ? OperandType.FP64 : OperandType.FP32;

            Operand a = ExtractScalar(context, type, op.Vd);
            Operand n = ExtractScalar(context, type, op.Vn);
            Operand m = ExtractScalar(context, type, op.Vm);

            InsertScalar(context, op.Vd, emit(a, n, m));
        }

        public static void EmitVectorUnaryOpF32(ArmEmitterContext context, Func1I emit)
        {
            OpCode32Simd op = (OpCode32Simd)context.CurrOp;

            int sizeF = op.Size & 1;

            OperandType type = sizeF != 0 ? OperandType.FP64 : OperandType.FP32;

            int elems = op.GetBytesCount() >> sizeF + 2;

            Operand res = GetVecA32(op.Qd);

            for (int index = 0; index < elems; index++)
            {
                Operand me = context.VectorExtract(type, GetVecA32(op.Qm), op.Fm + index);

                res = context.VectorInsert(res, emit(me), op.Fd + index);
            }

            context.Copy(GetVecA32(op.Qd), res);
        }

        public static void EmitVectorBinaryOpF32(ArmEmitterContext context, Func2I emit)
        {
            OpCode32SimdReg op = (OpCode32SimdReg)context.CurrOp;

            int sizeF = op.Size & 1;

            OperandType type = sizeF != 0 ? OperandType.FP64 : OperandType.FP32;

            int elems = op.GetBytesCount() >> (sizeF + 2);

            Operand res = GetVecA32(op.Qd);

            for (int index = 0; index < elems; index++)
            {
                Operand ne = context.VectorExtract(type, GetVecA32(op.Qn), op.Fn + index);
                Operand me = context.VectorExtract(type, GetVecA32(op.Qm), op.Fm + index);

                res = context.VectorInsert(res, emit(ne, me), op.Fd + index);
            }

            context.Copy(GetVecA32(op.Qd), res);
        }

        public static void EmitVectorTernaryOpF32(ArmEmitterContext context, Func3I emit)
        {
            OpCode32SimdReg op = (OpCode32SimdReg)context.CurrOp;

            int sizeF = op.Size & 1;

            OperandType type = sizeF != 0 ? OperandType.FP64 : OperandType.FP32;

            int elems = op.GetBytesCount() >> sizeF + 2;

            Operand res = GetVecA32(op.Qd);

            for (int index = 0; index < elems; index++)
            {
                Operand de = context.VectorExtract(type, GetVecA32(op.Qd), op.Fd + index);
                Operand ne = context.VectorExtract(type, GetVecA32(op.Qn), op.Fn + index);
                Operand me = context.VectorExtract(type, GetVecA32(op.Qm), op.Fm + index);

                res = context.VectorInsert(res, emit(de, ne, me), op.Fd + index);
            }

            context.Copy(GetVecA32(op.Qd), res);
        }

        // Integer

        public static void EmitVectorUnaryOpI32(ArmEmitterContext context, Func1I emit, bool signed)
        {
            OpCode32Simd op = (OpCode32Simd)context.CurrOp;

            Operand res = GetVecA32(op.Qd);

            int elems = op.GetBytesCount() >> op.Size;

            for (int index = 0; index < elems; index++)
            {
                Operand me = EmitVectorExtract32(context, op.Qm, op.Im + index, op.Size, signed);

                res = EmitVectorInsert(context, res, emit(me), op.Id + index, op.Size);
            }

            context.Copy(GetVecA32(op.Qd), res);
        }

        public static void EmitVectorBinaryOpI32(ArmEmitterContext context, Func2I emit, bool signed)
        {
            OpCode32SimdReg op = (OpCode32SimdReg)context.CurrOp;

            Operand res = GetVecA32(op.Qd);

            int elems = op.GetBytesCount() >> op.Size;

            for (int index = 0; index < elems; index++)
            {
                Operand ne = EmitVectorExtract32(context, op.Qn, op.In + index, op.Size, signed);
                Operand me = EmitVectorExtract32(context, op.Qm, op.Im + index, op.Size, signed);

                res = EmitVectorInsert(context, res, emit(ne, me), op.Id + index, op.Size);
            }

            context.Copy(GetVecA32(op.Qd), res);
        }

        public static void EmitVectorBinaryLongOpI32(ArmEmitterContext context, Func2I emit, bool signed)
        {
            OpCode32SimdReg op = (OpCode32SimdReg)context.CurrOp;

            Operand res = context.VectorZero();

            int elems = op.GetBytesCount() >> op.Size;

            for (int index = 0; index < elems; index++)
            {
                Operand ne = EmitVectorExtract32(context, op.Qn, op.In + index, op.Size, signed);
                Operand me = EmitVectorExtract32(context, op.Qm, op.Im + index, op.Size, signed);

                if (op.Size == 2)
                {
                    ne = signed ? context.SignExtend32(OperandType.I64, ne) : context.ZeroExtend32(OperandType.I64, ne);
                    me = signed ? context.SignExtend32(OperandType.I64, me) : context.ZeroExtend32(OperandType.I64, me);
                }

                res = EmitVectorInsert(context, res, emit(ne, me), index, op.Size + 1);
            }

            context.Copy(GetVecA32(op.Qd), res);
        }

        public static void EmitVectorBinaryWideOpI32(ArmEmitterContext context, Func2I emit, bool signed)
        {
            OpCode32SimdReg op = (OpCode32SimdReg)context.CurrOp;

            Operand res = context.VectorZero();

            int elems = op.GetBytesCount() >> op.Size;

            for (int index = 0; index < elems; index++)
            {
                Operand ne = EmitVectorExtract32(context, op.Qn, op.In + index, op.Size + 1, signed);
                Operand me = EmitVectorExtract32(context, op.Qm, op.Im + index, op.Size,     signed);

                if (op.Size == 2)
                {
                    me = signed ? context.SignExtend32(OperandType.I64, me) : context.ZeroExtend32(OperandType.I64, me);
                }

                res = EmitVectorInsert(context, res, emit(ne, me), index, op.Size + 1);
            }

            context.Copy(GetVecA32(op.Qd), res);
        }

        public static void EmitVectorImmBinaryQdQmOpZx32(ArmEmitterContext context, Func2I emit)
        {
            EmitVectorImmBinaryQdQmOpI32(context, emit, false);
        }

        public static void EmitVectorImmBinaryQdQmOpSx32(ArmEmitterContext context, Func2I emit)
        {
            EmitVectorImmBinaryQdQmOpI32(context, emit, true);
        }

        public static void EmitVectorImmBinaryQdQmOpI32(ArmEmitterContext context, Func2I emit, bool signed)
        {
            OpCode32SimdShImm op = (OpCode32SimdShImm)context.CurrOp;

            Operand res = GetVecA32(op.Qd);

            int elems = op.GetBytesCount() >> op.Size;

            for (int index = 0; index < elems; index++)
            {
                Operand de = EmitVectorExtract32(context, op.Qd, op.Id + index, op.Size, signed);
                Operand me = EmitVectorExtract32(context, op.Qm, op.Im + index, op.Size, signed);

                res = EmitVectorInsert(context, res, emit(de, me), op.Id + index, op.Size);
            }

            context.Copy(GetVecA32(op.Qd), res);
        }

        public static void EmitVectorTernaryLongOpI32(ArmEmitterContext context, Func3I emit, bool signed)
        {
            OpCode32SimdReg op = (OpCode32SimdReg)context.CurrOp;

            Operand res = context.VectorZero();

            int elems = op.GetBytesCount() >> op.Size;

            for (int index = 0; index < elems; index++)
            {
                Operand de = EmitVectorExtract32(context, op.Qd, op.Id + index, op.Size + 1, signed);
                Operand ne = EmitVectorExtract32(context, op.Qn, op.In + index, op.Size,     signed);
                Operand me = EmitVectorExtract32(context, op.Qm, op.Im + index, op.Size,     signed);

                if (op.Size == 2)
                {
                    ne = signed ? context.SignExtend32(OperandType.I64, ne) : context.ZeroExtend32(OperandType.I64, ne);
                    me = signed ? context.SignExtend32(OperandType.I64, me) : context.ZeroExtend32(OperandType.I64, me);
                }

                res = EmitVectorInsert(context, res, emit(de, ne, me), index, op.Size + 1);
            }

            context.Copy(GetVecA32(op.Qd), res);
        }

        public static void EmitVectorTernaryOpI32(ArmEmitterContext context, Func3I emit, bool signed)
        {
            OpCode32SimdReg op = (OpCode32SimdReg)context.CurrOp;

            Operand res = GetVecA32(op.Qd);

            int elems = op.GetBytesCount() >> op.Size;

            for (int index = 0; index < elems; index++)
            {
                Operand de = EmitVectorExtract32(context, op.Qd, op.Id + index, op.Size, signed);
                Operand ne = EmitVectorExtract32(context, op.Qn, op.In + index, op.Size, signed);
                Operand me = EmitVectorExtract32(context, op.Qm, op.Im + index, op.Size, signed);

                res = EmitVectorInsert(context, res, emit(de, ne, me), op.Id + index, op.Size);
            }

            context.Copy(GetVecA32(op.Qd), res);
        }

        public static void EmitVectorUnaryOpSx32(ArmEmitterContext context, Func1I emit)
        {
            EmitVectorUnaryOpI32(context, emit, true);
        }

        public static void EmitVectorBinaryOpSx32(ArmEmitterContext context, Func2I emit)
        {
            EmitVectorBinaryOpI32(context, emit, true);
        }

        public static void EmitVectorTernaryOpSx32(ArmEmitterContext context, Func3I emit)
        {
            EmitVectorTernaryOpI32(context, emit, true);
        }

        public static void EmitVectorUnaryOpZx32(ArmEmitterContext context, Func1I emit)
        {
            EmitVectorUnaryOpI32(context, emit, false);
        }

        public static void EmitVectorBinaryOpZx32(ArmEmitterContext context, Func2I emit)
        {
            EmitVectorBinaryOpI32(context, emit, false);
        }

        public static void EmitVectorTernaryOpZx32(ArmEmitterContext context, Func3I emit)
        {
            EmitVectorTernaryOpI32(context, emit, false);
        }

        // Vector by scalar

        public static void EmitVectorByScalarOpF32(ArmEmitterContext context, Func2I emit)
        {
            OpCode32SimdRegElem op = (OpCode32SimdRegElem)context.CurrOp;

            int sizeF = op.Size & 1;

            OperandType type = sizeF != 0 ? OperandType.FP64 : OperandType.FP32;

            int elems = op.GetBytesCount() >> sizeF + 2;

            Operand m = ExtractScalar(context, type, op.Vm);

            Operand res = GetVecA32(op.Qd);

            for (int index = 0; index < elems; index++)
            {
                Operand ne = context.VectorExtract(type, GetVecA32(op.Qn), op.Fn + index);

                res = context.VectorInsert(res, emit(ne, m), op.Fd + index);
            }

            context.Copy(GetVecA32(op.Qd), res);
        }

        public static void EmitVectorByScalarOpI32(ArmEmitterContext context, Func2I emit, bool signed)
        {
            OpCode32SimdRegElem op = (OpCode32SimdRegElem)context.CurrOp;

            Operand m = ExtractElement(context, op.Vm, op.Size, signed);

            Operand res = GetVecA32(op.Qd);

            int elems = op.GetBytesCount() >> op.Size;

            for (int index = 0; index < elems; index++)
            {
                Operand ne = EmitVectorExtract32(context, op.Qn, op.In + index, op.Size, signed);

                res = EmitVectorInsert(context, res, emit(ne, m), op.Id + index, op.Size);
            }

            context.Copy(GetVecA32(op.Qd), res);
        }

        public static void EmitVectorByScalarLongOpI32(ArmEmitterContext context, Func2I emit, bool signed)
        {
            OpCode32SimdRegElem op = (OpCode32SimdRegElem)context.CurrOp;

            Operand m = ExtractElement(context, op.Vm, op.Size, signed);

            if (op.Size == 2)
            {
                m = signed ? context.SignExtend32(OperandType.I64, m) : context.ZeroExtend32(OperandType.I64, m);
            }

            Operand res = context.VectorZero();

            int elems = op.GetBytesCount() >> op.Size;

            for (int index = 0; index < elems; index++)
            {
                Operand ne = EmitVectorExtract32(context, op.Qn, op.In + index, op.Size, signed);

                if (op.Size == 2)
                {
                    ne = signed ? context.SignExtend32(OperandType.I64, ne) : context.ZeroExtend32(OperandType.I64, ne);
                }

                res = EmitVectorInsert(context, res, emit(ne, m), index, op.Size + 1);
            }

            context.Copy(GetVecA32(op.Qd), res);
        }

        public static void EmitVectorsByScalarOpF32(ArmEmitterContext context, Func3I emit)
        {
            OpCode32SimdRegElem op = (OpCode32SimdRegElem)context.CurrOp;

            int sizeF = op.Size & 1;

            OperandType type = sizeF != 0 ? OperandType.FP64 : OperandType.FP32;

            int elems = op.GetBytesCount() >> sizeF + 2;

            Operand m = ExtractScalar(context, type, op.Vm);

            Operand res = GetVecA32(op.Qd);

            for (int index = 0; index < elems; index++)
            {
                Operand de = context.VectorExtract(type, GetVecA32(op.Qd), op.Fd + index);
                Operand ne = context.VectorExtract(type, GetVecA32(op.Qn), op.Fn + index);

                res = context.VectorInsert(res, emit(de, ne, m), op.Fd + index);
            }

            context.Copy(GetVecA32(op.Qd), res);
        }

        public static void EmitVectorsByScalarOpI32(ArmEmitterContext context, Func3I emit, bool signed)
        {
            OpCode32SimdRegElem op = (OpCode32SimdRegElem)context.CurrOp;

            Operand m = EmitVectorExtract32(context, op.Vm >> (4 - op.Size), op.Vm & ((1 << (4 - op.Size)) - 1), op.Size, signed);

            Operand res = GetVecA32(op.Qd);

            int elems = op.GetBytesCount() >> op.Size;

            for (int index = 0; index < elems; index++)
            {
                Operand de = EmitVectorExtract32(context, op.Qd, op.Id + index, op.Size, signed);
                Operand ne = EmitVectorExtract32(context, op.Qn, op.In + index, op.Size, signed);

                res = EmitVectorInsert(context, res, emit(de, ne, m), op.Id + index, op.Size);
            }

            context.Copy(GetVecA32(op.Qd), res);
        }

        // Pairwise

        public static void EmitVectorPairwiseOpF32(ArmEmitterContext context, Func2I emit)
        {
            OpCode32SimdReg op = (OpCode32SimdReg)context.CurrOp;

            int sizeF = op.Size & 1;

            OperandType type = sizeF != 0 ? OperandType.FP64 : OperandType.FP32;

            int elems = op.GetBytesCount() >> (sizeF + 2);
            int pairs = elems >> 1;

            Operand res = GetVecA32(op.Qd);
            Operand mvec = GetVecA32(op.Qm);
            Operand nvec = GetVecA32(op.Qn);

            for (int index = 0; index < pairs; index++)
            {
                int pairIndex = index << 1;

                Operand n1 = context.VectorExtract(type, nvec, op.Fn + pairIndex);
                Operand n2 = context.VectorExtract(type, nvec, op.Fn + pairIndex + 1);

                res = context.VectorInsert(res, emit(n1, n2), op.Fd + index);

                Operand m1 = context.VectorExtract(type, mvec, op.Fm + pairIndex);
                Operand m2 = context.VectorExtract(type, mvec, op.Fm + pairIndex + 1);

                res = context.VectorInsert(res, emit(m1, m2), op.Fd + index + pairs);
            }

            context.Copy(GetVecA32(op.Qd), res);
        }

        public static void EmitVectorPairwiseOpI32(ArmEmitterContext context, Func2I emit, bool signed)
        {
            OpCode32SimdReg op = (OpCode32SimdReg)context.CurrOp;

            int elems = op.GetBytesCount() >> op.Size;
            int pairs = elems >> 1;

            Operand res = GetVecA32(op.Qd);

            for (int index = 0; index < pairs; index++)
            {
                int pairIndex = index << 1;
                Operand n1 = EmitVectorExtract32(context, op.Qn, op.In + pairIndex, op.Size, signed);
                Operand n2 = EmitVectorExtract32(context, op.Qn, op.In + pairIndex + 1, op.Size, signed);

                Operand m1 = EmitVectorExtract32(context, op.Qm, op.Im + pairIndex, op.Size, signed);
                Operand m2 = EmitVectorExtract32(context, op.Qm, op.Im + pairIndex + 1, op.Size, signed);

                res = EmitVectorInsert(context, res, emit(n1, n2), op.Id + index, op.Size);
                res = EmitVectorInsert(context, res, emit(m1, m2), op.Id + index + pairs, op.Size);
            }

            context.Copy(GetVecA32(op.Qd), res);
        }

        // Narrow

        public static void EmitVectorUnaryNarrowOp32(ArmEmitterContext context, Func1I emit, bool signed = false)
        {
            OpCode32Simd op = (OpCode32Simd)context.CurrOp;

            int elems = 8 >> op.Size; // Size contains the target element size. (for when it becomes a doubleword)

            Operand res = GetVecA32(op.Qd);
            int id = (op.Vd & 1) << (3 - op.Size); // Target doubleword base.

            for (int index = 0; index < elems; index++)
            {
                Operand m = EmitVectorExtract32(context, op.Qm, index, op.Size + 1, signed);

                res = EmitVectorInsert(context, res, emit(m), id + index, op.Size);
            }

            context.Copy(GetVecA32(op.Qd), res);
        }

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

            if (targetSide == 1)
            {
                return context.AddIntrinsic(Intrinsic.X86Movlhps, input, input); // Low to high.
            }
            else
            {
                return context.AddIntrinsic(Intrinsic.X86Movhlps, input, input); // High to low.
            }
        }

        public static Operand EmitDoubleWordInsert(ArmEmitterContext context, Operand target, Operand value, int targetV)
        {
            Debug.Assert(target.Type == OperandType.V128 && value.Type == OperandType.V128);

            int targetSide = targetV & 1;
            int shuffleMask = 2;

            if (targetSide == 1)
            {
                return context.AddIntrinsic(Intrinsic.X86Shufpd, target, value, Const(shuffleMask));
            }
            else
            {
                return context.AddIntrinsic(Intrinsic.X86Shufpd, value, target, Const(shuffleMask));
            }
        }

        public static Operand EmitScalarInsert(ArmEmitterContext context, Operand target, Operand value, int reg, bool doubleWidth)
        {
            Debug.Assert(target.Type == OperandType.V128 && value.Type == OperandType.V128);

            // Insert from index 0 in value to index in target.
            int index = reg & (doubleWidth ? 1 : 3);

            if (doubleWidth)
            {
                if (index == 1)
                {
                    return context.AddIntrinsic(Intrinsic.X86Movlhps, target, value); // Low to high.
                }
                else
                {
                    return context.AddIntrinsic(Intrinsic.X86Shufpd, value, target, Const(2)); // Low to low, keep high from original.
                }
            }
            else
            {
                if (Optimizations.UseSse41)
                {
                    return context.AddIntrinsic(Intrinsic.X86Insertps, target, value, Const(index << 4));
                }
                else
                {
                    target = EmitSwapScalar(context, target, index, doubleWidth); // Swap value to replace into element 0.
                    target = context.AddIntrinsic(Intrinsic.X86Movss, target, value); // Move the value into element 0 of the vector.
                    return EmitSwapScalar(context, target, index, doubleWidth); // Swap new value back to the correct index.
                }
            }
        }

        public static Operand EmitSwapScalar(ArmEmitterContext context, Operand target, int reg, bool doubleWidth)
        {
            // Index into 0, 0 into index. This swap happens at the start of an A32 scalar op if required.
            int index = reg & (doubleWidth ? 1 : 3);
            if (index == 0) return target;

            if (doubleWidth)
            {
                int shuffleMask = 1; // Swap top and bottom. (b0 = 1, b1 = 0)
                return context.AddIntrinsic(Intrinsic.X86Shufpd, target, target, Const(shuffleMask));
            }
            else
            {
                int shuffleMask = (3 << 6) | (2 << 4) | (1 << 2) | index; // Swap index and 0. (others remain)
                shuffleMask &= ~(3 << (index * 2));

                return context.AddIntrinsic(Intrinsic.X86Shufps, target, target, Const(shuffleMask));
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

        public static void EmitVectorUnaryOpF32(ArmEmitterContext context, Intrinsic inst32, Intrinsic inst64)
        {
            OpCode32Simd op = (OpCode32Simd)context.CurrOp;

            Intrinsic inst = (op.Size & 1) != 0 ? inst64 : inst32;

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

        public static void EmitVectorBinaryOpF32(ArmEmitterContext context, Intrinsic inst32, Intrinsic inst64)
        {
            OpCode32SimdReg op = (OpCode32SimdReg)context.CurrOp;

            Intrinsic inst = (op.Size & 1) != 0 ? inst64 : inst32;
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

        public static void EmitVectorTernaryOpF32(ArmEmitterContext context, Intrinsic inst32pt1, Intrinsic inst64pt1, Intrinsic inst32pt2, Intrinsic inst64pt2)
        {
            OpCode32SimdReg op = (OpCode32SimdReg)context.CurrOp;

            Intrinsic inst1 = (op.Size & 1) != 0 ? inst64pt1 : inst32pt1;
            Intrinsic inst2 = (op.Size & 1) != 0 ? inst64pt2 : inst32pt2;

            EmitVectorTernaryOpSimd32(context, (d, n, m) =>
            {
                Operand res = context.AddIntrinsic(inst1, n, m);
                return res = context.AddIntrinsic(inst2, d, res);
            });
        }

        public static void EmitVectorTernaryOpF32(ArmEmitterContext context, Intrinsic inst32)
        {
            OpCode32SimdReg op = (OpCode32SimdReg)context.CurrOp;

            Debug.Assert((op.Size & 1) == 0);

            EmitVectorTernaryOpSimd32(context, (d, n, m) =>
            {
                return context.AddIntrinsic(inst32, d, n, m);
            });
        }

        public static void EmitScalarUnaryOpSimd32(ArmEmitterContext context, Func1I scalarFunc)
        {
            OpCode32SimdS op = (OpCode32SimdS)context.CurrOp;

            bool doubleSize = (op.Size & 1) != 0;
            int shift = doubleSize ? 1 : 2;
            Operand m = GetVecA32(op.Vm >> shift);
            Operand d = GetVecA32(op.Vd >> shift);

            m = EmitSwapScalar(context, m, op.Vm, doubleSize);

            Operand res = scalarFunc(m);

            // Insert scalar into vector.
            res = EmitScalarInsert(context, d, res, op.Vd, doubleSize);

            context.Copy(d, res);
        }

        public static void EmitScalarUnaryOpF32(ArmEmitterContext context, Intrinsic inst32, Intrinsic inst64)
        {
            OpCode32SimdS op = (OpCode32SimdS)context.CurrOp;

            Intrinsic inst = (op.Size & 1) != 0 ? inst64 : inst32;

            EmitScalarUnaryOpSimd32(context, (m) => (inst == 0) ? m : context.AddIntrinsic(inst, m));
        }

        public static void EmitScalarBinaryOpSimd32(ArmEmitterContext context, Func2I scalarFunc)
        {
            OpCode32SimdRegS op = (OpCode32SimdRegS)context.CurrOp;

            bool doubleSize = (op.Size & 1) != 0;
            int shift = doubleSize ? 1 : 2;
            Operand n = GetVecA32(op.Vn >> shift);
            Operand m = GetVecA32(op.Vm >> shift);
            Operand d = GetVecA32(op.Vd >> shift);

            n = EmitSwapScalar(context, n, op.Vn, doubleSize);
            m = EmitSwapScalar(context, m, op.Vm, doubleSize);

            Operand res = scalarFunc(n, m);

            // Insert scalar into vector.
            res = EmitScalarInsert(context, d, res, op.Vd, doubleSize);

            context.Copy(d, res);
        }

        public static void EmitScalarBinaryOpF32(ArmEmitterContext context, Intrinsic inst32, Intrinsic inst64)
        {
            OpCode32SimdRegS op = (OpCode32SimdRegS)context.CurrOp;

            Intrinsic inst = (op.Size & 1) != 0 ? inst64 : inst32;

            EmitScalarBinaryOpSimd32(context, (n, m) =>  context.AddIntrinsic(inst, n, m));
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

            n = EmitSwapScalar(context, n, op.Vn, doubleSize);
            m = EmitSwapScalar(context, m, op.Vm, doubleSize);
            d = EmitSwapScalar(context, d, op.Vd, doubleSize);

            Operand res = scalarFunc(d, n, m);

            // Insert scalar into vector.
            res = EmitScalarInsert(context, initialD, res, op.Vd, doubleSize);

            context.Copy(initialD, res);
        }

        public static void EmitScalarTernaryOpF32(ArmEmitterContext context, Intrinsic inst32, Intrinsic inst64)
        {
            OpCode32SimdRegS op = (OpCode32SimdRegS)context.CurrOp;

            bool doubleSize = (op.Size & 1) != 0;

            Intrinsic inst = doubleSize ? inst64 : inst32;

            EmitScalarTernaryOpSimd32(context, (d, n, m) =>
            {
                return context.AddIntrinsic(inst, d, n, m);
            });
        }

        public static void EmitScalarTernaryOpF32(
            ArmEmitterContext context,
            Intrinsic inst32pt1,
            Intrinsic inst64pt1,
            Intrinsic inst32pt2,
            Intrinsic inst64pt2,
            bool isNegD = false)
        {
            OpCode32SimdRegS op = (OpCode32SimdRegS)context.CurrOp;

            bool doubleSize = (op.Size & 1) != 0;

            Intrinsic inst1 = doubleSize ? inst64pt1 : inst32pt1;
            Intrinsic inst2 = doubleSize ? inst64pt2 : inst32pt2;

            EmitScalarTernaryOpSimd32(context, (d, n, m) =>
            {
                Operand res = context.AddIntrinsic(inst1, n, m);

                if (isNegD)
                {
                    Operand mask = doubleSize
                        ? X86GetScalar(context, -0d)
                        : X86GetScalar(context, -0f);

                    d = doubleSize
                        ? context.AddIntrinsic(Intrinsic.X86Xorpd, mask, d)
                        : context.AddIntrinsic(Intrinsic.X86Xorps, mask, d);
                }

                return context.AddIntrinsic(inst2, d, res);
            });
        }

        // By Scalar

        public static void EmitVectorByScalarOpSimd32(ArmEmitterContext context, Func2I vectorFunc)
        {
            OpCode32SimdRegElem op = (OpCode32SimdRegElem)context.CurrOp;

            Operand n = GetVecA32(op.Qn);
            Operand d = GetVecA32(op.Qd);

            int index = op.Vm & 3;
            int dupeMask = (index << 6) | (index << 4) | (index << 2) | index;
            Operand m = GetVecA32(op.Vm >> 2);
            m = context.AddIntrinsic(Intrinsic.X86Shufps, m, m, Const(dupeMask));

            if (!op.Q) // Register swap: move relevant doubleword to destination side.
            {
                n = EmitMoveDoubleWordToSide(context, n, op.Vn, op.Vd);
            }

            Operand res = vectorFunc(n, m);

            if (!op.Q) // Register insert.
            {
                res = EmitDoubleWordInsert(context, d, res, op.Vd);
            }

            context.Copy(d, res);
        }

        public static void EmitVectorByScalarOpF32(ArmEmitterContext context, Intrinsic inst32, Intrinsic inst64)
        {
            OpCode32SimdRegElem op = (OpCode32SimdRegElem)context.CurrOp;

            Intrinsic inst = (op.Size & 1) != 0 ? inst64 : inst32;
            EmitVectorByScalarOpSimd32(context, (n, m) => context.AddIntrinsic(inst, n, m));
        }

        public static void EmitVectorsByScalarOpSimd32(ArmEmitterContext context, Func3I vectorFunc)
        {
            OpCode32SimdRegElem op = (OpCode32SimdRegElem)context.CurrOp;

            Operand n = GetVecA32(op.Qn);
            Operand d = GetVecA32(op.Qd);
            Operand initialD = d;

            int index = op.Vm & 3;
            int dupeMask = (index << 6) | (index << 4) | (index << 2) | index;
            Operand m = GetVecA32(op.Vm >> 2);
            m = context.AddIntrinsic(Intrinsic.X86Shufps, m, m, Const(dupeMask));

            if (!op.Q) // Register swap: move relevant doubleword to destination side.
            {
                n = EmitMoveDoubleWordToSide(context, n, op.Vn, op.Vd);
            }

            Operand res = vectorFunc(d, n, m);

            if (!op.Q) // Register insert.
            {
                res = EmitDoubleWordInsert(context, initialD, res, op.Vd);
            }

            context.Copy(initialD, res);
        }

        public static void EmitVectorsByScalarOpF32(ArmEmitterContext context, Intrinsic inst32pt1, Intrinsic inst64pt1, Intrinsic inst32pt2, Intrinsic inst64pt2)
        {
            OpCode32SimdRegElem op = (OpCode32SimdRegElem)context.CurrOp;

            Intrinsic inst1 = (op.Size & 1) != 0 ? inst64pt1 : inst32pt1;
            Intrinsic inst2 = (op.Size & 1) != 0 ? inst64pt2 : inst32pt2;

            EmitVectorsByScalarOpSimd32(context, (d, n, m) =>
            {
                Operand res = context.AddIntrinsic(inst1, n, m);
                return res = context.AddIntrinsic(inst2, d, res);
            });
        }

        // Pairwise

        public static void EmitSse2VectorPairwiseOpF32(ArmEmitterContext context, Intrinsic inst32)
        {
            OpCode32SimdReg op = (OpCode32SimdReg)context.CurrOp;

            EmitVectorBinaryOpSimd32(context, (n, m) =>
            {
                Operand unpck = context.AddIntrinsic(Intrinsic.X86Unpcklps, n, m);

                Operand part0 = unpck;
                Operand part1 = context.AddIntrinsic(Intrinsic.X86Movhlps, unpck, unpck);

                return context.AddIntrinsic(inst32, part0, part1);
            }, 0);
        }

        public static void EmitSsse3VectorPairwiseOp32(ArmEmitterContext context, Intrinsic[] inst)
        {
            OpCode32SimdReg op = (OpCode32SimdReg)context.CurrOp;

            EmitVectorBinaryOpSimd32(context, (n, m) =>
            {
                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    Operand zeroEvenMask = X86GetElements(context, ZeroMask, EvenMasks[op.Size]);
                    Operand zeroOddMask = X86GetElements(context, ZeroMask, OddMasks[op.Size]);

                    Operand mN = context.AddIntrinsic(Intrinsic.X86Punpcklqdq, n, m); // m:n

                    Operand left = context.AddIntrinsic(Intrinsic.X86Pshufb, mN, zeroEvenMask); // 0:even from m:n
                    Operand right = context.AddIntrinsic(Intrinsic.X86Pshufb, mN, zeroOddMask); // 0:odd  from m:n

                    return context.AddIntrinsic(inst[op.Size], left, right);
                }
                else if (op.Size < 3)
                {
                    Operand oddEvenMask = X86GetElements(context, OddMasks[op.Size], EvenMasks[op.Size]);

                    Operand oddEvenN = context.AddIntrinsic(Intrinsic.X86Pshufb, n, oddEvenMask); // odd:even from n
                    Operand oddEvenM = context.AddIntrinsic(Intrinsic.X86Pshufb, m, oddEvenMask); // odd:even from m

                    Operand left = context.AddIntrinsic(Intrinsic.X86Punpcklqdq, oddEvenN, oddEvenM);
                    Operand right = context.AddIntrinsic(Intrinsic.X86Punpckhqdq, oddEvenN, oddEvenM);

                    return context.AddIntrinsic(inst[op.Size], left, right);
                }
                else
                {
                    Operand left = context.AddIntrinsic(Intrinsic.X86Punpcklqdq, n, m);
                    Operand right = context.AddIntrinsic(Intrinsic.X86Punpckhqdq, n, m);

                    return context.AddIntrinsic(inst[3], left, right);
                }
            }, 0);
        }

        // Generic Functions

        public static Operand EmitSoftFloatCallDefaultFpscr(ArmEmitterContext context, string name, params Operand[] callArgs)
        {
            IOpCodeSimd op = (IOpCodeSimd)context.CurrOp;

            MethodInfo info = (op.Size & 1) == 0
                ? typeof(SoftFloat32).GetMethod(name)
                : typeof(SoftFloat64).GetMethod(name);

            Array.Resize(ref callArgs, callArgs.Length + 1);
            callArgs[callArgs.Length - 1] = Const(1);

            return context.Call(info, callArgs);
        }

        public static Operand EmitVectorExtractSx32(ArmEmitterContext context, int reg, int index, int size)
        {
            return EmitVectorExtract32(context, reg, index, size, true);
        }

        public static Operand EmitVectorExtractZx32(ArmEmitterContext context, int reg, int index, int size)
        {
            return EmitVectorExtract32(context, reg, index, size, false);
        }

        public static Operand EmitVectorExtract32(ArmEmitterContext context, int reg, int index, int size, bool signed)
        {
            ThrowIfInvalid(index, size);

            Operand res = null;

            switch (size)
            {
                case 0:
                    res = context.VectorExtract8(GetVec(reg), index);
                    break;

                case 1:
                    res = context.VectorExtract16(GetVec(reg), index);
                    break;

                case 2:
                    res = context.VectorExtract(OperandType.I32, GetVec(reg), index);
                    break;

                case 3:
                    res = context.VectorExtract(OperandType.I64, GetVec(reg), index);
                    break;
            }

            if (signed)
            {
                switch (size)
                {
                    case 0: res = context.SignExtend8(OperandType.I32, res); break;
                    case 1: res = context.SignExtend16(OperandType.I32, res); break;
                }
            }
            else
            {
                switch (size)
                {
                    case 0: res = context.ZeroExtend8(OperandType.I32, res); break;
                    case 1: res = context.ZeroExtend16(OperandType.I32, res); break;
                }
            }

            return res;
        }

        public static Operand EmitPolynomialMultiply(ArmEmitterContext context, Operand op1, Operand op2, int eSize)
        {
            Debug.Assert(eSize <= 32);

            Operand result = eSize == 32 ? Const(0L) : Const(0);

            if (eSize == 32)
            {
                op1 = context.ZeroExtend32(OperandType.I64, op1);
                op2 = context.ZeroExtend32(OperandType.I64, op2);
            }

            for (int i = 0; i < eSize; i++)
            {
                Operand mask = context.BitwiseAnd(op1, Const(op1.Type, 1L << i));

                result = context.BitwiseExclusiveOr(result, context.Multiply(op2, mask));
            }

            return result;
        }
    }
}
