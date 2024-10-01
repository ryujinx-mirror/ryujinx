using ARMeilleure.IntermediateRepresentation;
using System;
using System.Diagnostics;

namespace ARMeilleure.CodeGen.Arm64
{
    static class CodeGeneratorIntrinsic
    {
        public static void GenerateOperation(CodeGenContext context, Operation operation)
        {
            Intrinsic intrin = operation.Intrinsic;

            IntrinsicInfo info = IntrinsicTable.GetInfo(intrin & ~(Intrinsic.Arm64VTypeMask | Intrinsic.Arm64VSizeMask));

            switch (info.Type)
            {
                case IntrinsicType.ScalarUnary:
                    GenerateVectorUnary(
                        context,
                        0,
                        (uint)(intrin & Intrinsic.Arm64VSizeMask) >> (int)Intrinsic.Arm64VSizeShift,
                        info.Inst,
                        operation.Destination,
                        operation.GetSource(0));
                    break;
                case IntrinsicType.ScalarUnaryByElem:
                    Debug.Assert(operation.GetSource(1).Kind == OperandKind.Constant);

                    GenerateVectorUnaryByElem(
                        context,
                        0,
                        (uint)(intrin & Intrinsic.Arm64VSizeMask) >> (int)Intrinsic.Arm64VSizeShift,
                        info.Inst,
                        (uint)operation.GetSource(1).AsInt32(),
                        operation.Destination,
                        operation.GetSource(0));
                    break;
                case IntrinsicType.ScalarBinary:
                    GenerateVectorBinary(
                        context,
                        0,
                        (uint)(intrin & Intrinsic.Arm64VSizeMask) >> (int)Intrinsic.Arm64VSizeShift,
                        info.Inst,
                        operation.Destination,
                        operation.GetSource(0),
                        operation.GetSource(1));
                    break;
                case IntrinsicType.ScalarBinaryFPByElem:
                    Debug.Assert(operation.GetSource(2).Kind == OperandKind.Constant);

                    GenerateVectorBinaryFPByElem(
                        context,
                        0,
                        (uint)(intrin & Intrinsic.Arm64VSizeMask) >> (int)Intrinsic.Arm64VSizeShift,
                        info.Inst,
                        (uint)operation.GetSource(2).AsInt32(),
                        operation.Destination,
                        operation.GetSource(0),
                        operation.GetSource(1));
                    break;
                case IntrinsicType.ScalarBinaryRd:
                    GenerateVectorUnary(
                        context,
                        0,
                        (uint)(intrin & Intrinsic.Arm64VSizeMask) >> (int)Intrinsic.Arm64VSizeShift,
                        info.Inst,
                        operation.Destination,
                        operation.GetSource(1));
                    break;
                case IntrinsicType.ScalarBinaryShl:
                    Debug.Assert(operation.GetSource(1).Kind == OperandKind.Constant);

                    GenerateVectorBinaryShlImm(
                        context,
                        0,
                        (uint)(intrin & Intrinsic.Arm64VSizeMask) >> (int)Intrinsic.Arm64VSizeShift,
                        info.Inst,
                        operation.Destination,
                        operation.GetSource(0),
                        (uint)operation.GetSource(1).AsInt32());
                    break;
                case IntrinsicType.ScalarBinaryShr:
                    Debug.Assert(operation.GetSource(1).Kind == OperandKind.Constant);

                    GenerateVectorBinaryShrImm(
                        context,
                        0,
                        (uint)(intrin & Intrinsic.Arm64VSizeMask) >> (int)Intrinsic.Arm64VSizeShift,
                        info.Inst,
                        operation.Destination,
                        operation.GetSource(0),
                        (uint)operation.GetSource(1).AsInt32());
                    break;
                case IntrinsicType.ScalarFPCompare:
                    GenerateScalarFPCompare(
                        context,
                        (uint)(intrin & Intrinsic.Arm64VSizeMask) >> (int)Intrinsic.Arm64VSizeShift,
                        info.Inst,
                        operation.Destination,
                        operation.GetSource(0),
                        operation.GetSource(1));
                    break;
                case IntrinsicType.ScalarFPConvFixed:
                    Debug.Assert(operation.GetSource(1).Kind == OperandKind.Constant);

                    GenerateVectorBinaryShrImm(
                        context,
                        0,
                        ((uint)(intrin & Intrinsic.Arm64VSizeMask) >> (int)Intrinsic.Arm64VSizeShift) + 2u,
                        info.Inst,
                        operation.Destination,
                        operation.GetSource(0),
                        (uint)operation.GetSource(1).AsInt32());
                    break;
                case IntrinsicType.ScalarFPConvFixedGpr:
                    Debug.Assert(operation.GetSource(1).Kind == OperandKind.Constant);

                    GenerateScalarFPConvGpr(
                        context,
                        (uint)(intrin & Intrinsic.Arm64VSizeMask) >> (int)Intrinsic.Arm64VSizeShift,
                        info.Inst,
                        operation.Destination,
                        operation.GetSource(0),
                        (uint)operation.GetSource(1).AsInt32());
                    break;
                case IntrinsicType.ScalarFPConvGpr:
                    GenerateScalarFPConvGpr(
                        context,
                        (uint)(intrin & Intrinsic.Arm64VSizeMask) >> (int)Intrinsic.Arm64VSizeShift,
                        info.Inst,
                        operation.Destination,
                        operation.GetSource(0));
                    break;
                case IntrinsicType.ScalarTernary:
                    GenerateScalarTernary(
                        context,
                        (uint)(intrin & Intrinsic.Arm64VSizeMask) >> (int)Intrinsic.Arm64VSizeShift,
                        info.Inst,
                        operation.Destination,
                        operation.GetSource(1),
                        operation.GetSource(2),
                        operation.GetSource(0));
                    break;
                case IntrinsicType.ScalarTernaryFPRdByElem:
                    Debug.Assert(operation.GetSource(3).Kind == OperandKind.Constant);

                    GenerateVectorBinaryFPByElem(
                        context,
                        0,
                        (uint)(intrin & Intrinsic.Arm64VSizeMask) >> (int)Intrinsic.Arm64VSizeShift,
                        info.Inst,
                        (uint)operation.GetSource(3).AsInt32(),
                        operation.Destination,
                        operation.GetSource(1),
                        operation.GetSource(2));
                    break;
                case IntrinsicType.ScalarTernaryShlRd:
                    Debug.Assert(operation.GetSource(2).Kind == OperandKind.Constant);

                    GenerateVectorBinaryShlImm(
                        context,
                        0,
                        (uint)(intrin & Intrinsic.Arm64VSizeMask) >> (int)Intrinsic.Arm64VSizeShift,
                        info.Inst,
                        operation.Destination,
                        operation.GetSource(1),
                        (uint)operation.GetSource(2).AsInt32());
                    break;
                case IntrinsicType.ScalarTernaryShrRd:
                    Debug.Assert(operation.GetSource(2).Kind == OperandKind.Constant);

                    GenerateVectorBinaryShrImm(
                        context,
                        0,
                        (uint)(intrin & Intrinsic.Arm64VSizeMask) >> (int)Intrinsic.Arm64VSizeShift,
                        info.Inst,
                        operation.Destination,
                        operation.GetSource(1),
                        (uint)operation.GetSource(2).AsInt32());
                    break;

                case IntrinsicType.Vector128Unary:
                    GenerateVectorUnary(
                        context,
                        1,
                        0,
                        info.Inst,
                        operation.Destination,
                        operation.GetSource(0));
                    break;
                case IntrinsicType.Vector128Binary:
                    GenerateVectorBinary(
                        context,
                        1,
                        0,
                        info.Inst,
                        operation.Destination,
                        operation.GetSource(0),
                        operation.GetSource(1));
                    break;
                case IntrinsicType.Vector128BinaryRd:
                    GenerateVectorUnary(
                        context,
                        1,
                        0,
                        info.Inst,
                        operation.Destination,
                        operation.GetSource(1));
                    break;

                case IntrinsicType.VectorUnary:
                    GenerateVectorUnary(
                        context,
                        (uint)(intrin & Intrinsic.Arm64VTypeMask) >> (int)Intrinsic.Arm64VTypeShift,
                        (uint)(intrin & Intrinsic.Arm64VSizeMask) >> (int)Intrinsic.Arm64VSizeShift,
                        info.Inst,
                        operation.Destination,
                        operation.GetSource(0));
                    break;
                case IntrinsicType.VectorUnaryByElem:
                    Debug.Assert(operation.GetSource(1).Kind == OperandKind.Constant);

                    GenerateVectorUnaryByElem(
                        context,
                        (uint)(intrin & Intrinsic.Arm64VTypeMask) >> (int)Intrinsic.Arm64VTypeShift,
                        (uint)(intrin & Intrinsic.Arm64VSizeMask) >> (int)Intrinsic.Arm64VSizeShift,
                        info.Inst,
                        (uint)operation.GetSource(1).AsInt32(),
                        operation.Destination,
                        operation.GetSource(0));
                    break;
                case IntrinsicType.VectorBinary:
                    GenerateVectorBinary(
                        context,
                        (uint)(intrin & Intrinsic.Arm64VTypeMask) >> (int)Intrinsic.Arm64VTypeShift,
                        (uint)(intrin & Intrinsic.Arm64VSizeMask) >> (int)Intrinsic.Arm64VSizeShift,
                        info.Inst,
                        operation.Destination,
                        operation.GetSource(0),
                        operation.GetSource(1));
                    break;
                case IntrinsicType.VectorBinaryBitwise:
                    GenerateVectorBinary(
                        context,
                        (uint)(intrin & Intrinsic.Arm64VTypeMask) >> (int)Intrinsic.Arm64VTypeShift,
                        info.Inst,
                        operation.Destination,
                        operation.GetSource(0),
                        operation.GetSource(1));
                    break;
                case IntrinsicType.VectorBinaryByElem:
                    Debug.Assert(operation.GetSource(2).Kind == OperandKind.Constant);

                    GenerateVectorBinaryByElem(
                        context,
                        (uint)(intrin & Intrinsic.Arm64VTypeMask) >> (int)Intrinsic.Arm64VTypeShift,
                        (uint)(intrin & Intrinsic.Arm64VSizeMask) >> (int)Intrinsic.Arm64VSizeShift,
                        info.Inst,
                        (uint)operation.GetSource(2).AsInt32(),
                        operation.Destination,
                        operation.GetSource(0),
                        operation.GetSource(1));
                    break;
                case IntrinsicType.VectorBinaryFPByElem:
                    Debug.Assert(operation.GetSource(2).Kind == OperandKind.Constant);

                    GenerateVectorBinaryFPByElem(
                        context,
                        (uint)(intrin & Intrinsic.Arm64VTypeMask) >> (int)Intrinsic.Arm64VTypeShift,
                        (uint)(intrin & Intrinsic.Arm64VSizeMask) >> (int)Intrinsic.Arm64VSizeShift,
                        info.Inst,
                        (uint)operation.GetSource(2).AsInt32(),
                        operation.Destination,
                        operation.GetSource(0),
                        operation.GetSource(1));
                    break;
                case IntrinsicType.VectorBinaryRd:
                    GenerateVectorUnary(
                        context,
                        (uint)(intrin & Intrinsic.Arm64VTypeMask) >> (int)Intrinsic.Arm64VTypeShift,
                        (uint)(intrin & Intrinsic.Arm64VSizeMask) >> (int)Intrinsic.Arm64VSizeShift,
                        info.Inst,
                        operation.Destination,
                        operation.GetSource(1));
                    break;
                case IntrinsicType.VectorBinaryShl:
                    Debug.Assert(operation.GetSource(1).Kind == OperandKind.Constant);

                    GenerateVectorBinaryShlImm(
                        context,
                        (uint)(intrin & Intrinsic.Arm64VTypeMask) >> (int)Intrinsic.Arm64VTypeShift,
                        (uint)(intrin & Intrinsic.Arm64VSizeMask) >> (int)Intrinsic.Arm64VSizeShift,
                        info.Inst,
                        operation.Destination,
                        operation.GetSource(0),
                        (uint)operation.GetSource(1).AsInt32());
                    break;
                case IntrinsicType.VectorBinaryShr:
                    Debug.Assert(operation.GetSource(1).Kind == OperandKind.Constant);

                    GenerateVectorBinaryShrImm(
                        context,
                        (uint)(intrin & Intrinsic.Arm64VTypeMask) >> (int)Intrinsic.Arm64VTypeShift,
                        (uint)(intrin & Intrinsic.Arm64VSizeMask) >> (int)Intrinsic.Arm64VSizeShift,
                        info.Inst,
                        operation.Destination,
                        operation.GetSource(0),
                        (uint)operation.GetSource(1).AsInt32());
                    break;
                case IntrinsicType.VectorFPConvFixed:
                    Debug.Assert(operation.GetSource(1).Kind == OperandKind.Constant);

                    GenerateVectorBinaryShrImm(
                        context,
                        (uint)(intrin & Intrinsic.Arm64VTypeMask) >> (int)Intrinsic.Arm64VTypeShift,
                        ((uint)(intrin & Intrinsic.Arm64VSizeMask) >> (int)Intrinsic.Arm64VSizeShift) + 2u,
                        info.Inst,
                        operation.Destination,
                        operation.GetSource(0),
                        (uint)operation.GetSource(1).AsInt32());
                    break;
                case IntrinsicType.VectorInsertByElem:
                    Debug.Assert(operation.GetSource(1).Kind == OperandKind.Constant);
                    Debug.Assert(operation.GetSource(3).Kind == OperandKind.Constant);

                    GenerateVectorInsertByElem(
                        context,
                        (uint)(intrin & Intrinsic.Arm64VSizeMask) >> (int)Intrinsic.Arm64VSizeShift,
                        info.Inst,
                        (uint)operation.GetSource(3).AsInt32(),
                        (uint)operation.GetSource(1).AsInt32(),
                        operation.Destination,
                        operation.GetSource(2));
                    break;
                case IntrinsicType.VectorLookupTable:
                    Debug.Assert((uint)(operation.SourcesCount - 2) <= 3);

                    for (int i = 1; i < operation.SourcesCount - 1; i++)
                    {
                        Register currReg = operation.GetSource(i).GetRegister();
                        Register prevReg = operation.GetSource(i - 1).GetRegister();

                        Debug.Assert(prevReg.Index + 1 == currReg.Index && currReg.Type == RegisterType.Vector);
                    }

                    GenerateVectorBinary(
                        context,
                        (uint)(intrin & Intrinsic.Arm64VTypeMask) >> (int)Intrinsic.Arm64VTypeShift,
                        info.Inst | ((uint)(operation.SourcesCount - 2) << 13),
                        operation.Destination,
                        operation.GetSource(0),
                        operation.GetSource(operation.SourcesCount - 1));
                    break;
                case IntrinsicType.VectorTernaryFPRdByElem:
                    Debug.Assert(operation.GetSource(3).Kind == OperandKind.Constant);

                    GenerateVectorBinaryFPByElem(
                        context,
                        (uint)(intrin & Intrinsic.Arm64VTypeMask) >> (int)Intrinsic.Arm64VTypeShift,
                        (uint)(intrin & Intrinsic.Arm64VSizeMask) >> (int)Intrinsic.Arm64VSizeShift,
                        info.Inst,
                        (uint)operation.GetSource(3).AsInt32(),
                        operation.Destination,
                        operation.GetSource(1),
                        operation.GetSource(2));
                    break;
                case IntrinsicType.VectorTernaryRd:
                    GenerateVectorBinary(
                        context,
                        (uint)(intrin & Intrinsic.Arm64VTypeMask) >> (int)Intrinsic.Arm64VTypeShift,
                        (uint)(intrin & Intrinsic.Arm64VSizeMask) >> (int)Intrinsic.Arm64VSizeShift,
                        info.Inst,
                        operation.Destination,
                        operation.GetSource(1),
                        operation.GetSource(2));
                    break;
                case IntrinsicType.VectorTernaryRdBitwise:
                    GenerateVectorBinary(
                        context,
                        (uint)(intrin & Intrinsic.Arm64VTypeMask) >> (int)Intrinsic.Arm64VTypeShift,
                        info.Inst,
                        operation.Destination,
                        operation.GetSource(1),
                        operation.GetSource(2));
                    break;
                case IntrinsicType.VectorTernaryRdByElem:
                    Debug.Assert(operation.GetSource(3).Kind == OperandKind.Constant);

                    GenerateVectorBinaryByElem(
                        context,
                        (uint)(intrin & Intrinsic.Arm64VTypeMask) >> (int)Intrinsic.Arm64VTypeShift,
                        (uint)(intrin & Intrinsic.Arm64VSizeMask) >> (int)Intrinsic.Arm64VSizeShift,
                        info.Inst,
                        (uint)operation.GetSource(3).AsInt32(),
                        operation.Destination,
                        operation.GetSource(1),
                        operation.GetSource(2));
                    break;
                case IntrinsicType.VectorTernaryShlRd:
                    Debug.Assert(operation.GetSource(2).Kind == OperandKind.Constant);

                    GenerateVectorBinaryShlImm(
                        context,
                        (uint)(intrin & Intrinsic.Arm64VTypeMask) >> (int)Intrinsic.Arm64VTypeShift,
                        (uint)(intrin & Intrinsic.Arm64VSizeMask) >> (int)Intrinsic.Arm64VSizeShift,
                        info.Inst,
                        operation.Destination,
                        operation.GetSource(1),
                        (uint)operation.GetSource(2).AsInt32());
                    break;
                case IntrinsicType.VectorTernaryShrRd:
                    Debug.Assert(operation.GetSource(2).Kind == OperandKind.Constant);

                    GenerateVectorBinaryShrImm(
                        context,
                        (uint)(intrin & Intrinsic.Arm64VTypeMask) >> (int)Intrinsic.Arm64VTypeShift,
                        (uint)(intrin & Intrinsic.Arm64VSizeMask) >> (int)Intrinsic.Arm64VSizeShift,
                        info.Inst,
                        operation.Destination,
                        operation.GetSource(1),
                        (uint)operation.GetSource(2).AsInt32());
                    break;

                case IntrinsicType.GetRegister:
                    context.Assembler.WriteInstruction(info.Inst, operation.Destination);
                    break;
                case IntrinsicType.SetRegister:
                    context.Assembler.WriteInstruction(info.Inst, operation.GetSource(0));
                    break;

                default:
                    throw new NotImplementedException(info.Type.ToString());
            }
        }

        private static void GenerateScalarFPCompare(
            CodeGenContext context,
            uint sz,
            uint instruction,
            Operand dest,
            Operand rn,
            Operand rm)
        {
            instruction |= (sz << 22);

            if (rm.Kind == OperandKind.Constant && rm.Value == 0)
            {
                instruction |= 0b1000;
                rm = rn;
            }

            context.Assembler.WriteInstructionRm16NoRet(instruction, rn, rm);
            context.Assembler.Mrs(dest, 1, 3, 4, 2, 0);
        }

        private static void GenerateScalarFPConvGpr(
            CodeGenContext context,
            uint sz,
            uint instruction,
            Operand rd,
            Operand rn)
        {
            instruction |= (sz << 22);

            if (rd.Type.IsInteger())
            {
                context.Assembler.WriteInstructionAuto(instruction, rd, rn);
            }
            else
            {
                if (rn.Type == OperandType.I64)
                {
                    instruction |= Assembler.SfFlag;
                }

                context.Assembler.WriteInstruction(instruction, rd, rn);
            }
        }

        private static void GenerateScalarFPConvGpr(
            CodeGenContext context,
            uint sz,
            uint instruction,
            Operand rd,
            Operand rn,
            uint fBits)
        {
            Debug.Assert(fBits <= 64);

            instruction |= (sz << 22);
            instruction |= (64 - fBits) << 10;

            if (rd.Type.IsInteger())
            {
                Debug.Assert(rd.Type != OperandType.I32 || fBits <= 32);

                context.Assembler.WriteInstructionAuto(instruction, rd, rn);
            }
            else
            {
                if (rn.Type == OperandType.I64)
                {
                    instruction |= Assembler.SfFlag;
                }
                else
                {
                    Debug.Assert(fBits <= 32);
                }

                context.Assembler.WriteInstruction(instruction, rd, rn);
            }

        }

        private static void GenerateScalarTernary(
            CodeGenContext context,
            uint sz,
            uint instruction,
            Operand rd,
            Operand rn,
            Operand rm,
            Operand ra)
        {
            instruction |= (sz << 22);

            context.Assembler.WriteInstruction(instruction, rd, rn, rm, ra);
        }

        private static void GenerateVectorUnary(
            CodeGenContext context,
            uint q,
            uint sz,
            uint instruction,
            Operand rd,
            Operand rn)
        {
            instruction |= (q << 30) | (sz << 22);

            context.Assembler.WriteInstruction(instruction, rd, rn);
        }

        private static void GenerateVectorUnaryByElem(
            CodeGenContext context,
            uint q,
            uint sz,
            uint instruction,
            uint srcIndex,
            Operand rd,
            Operand rn)
        {
            uint imm5 = (srcIndex << ((int)sz + 1)) | (1u << (int)sz);

            instruction |= (q << 30) | (imm5 << 16);

            context.Assembler.WriteInstruction(instruction, rd, rn);
        }

        private static void GenerateVectorBinary(
            CodeGenContext context,
            uint q,
            uint instruction,
            Operand rd,
            Operand rn,
            Operand rm)
        {
            instruction |= (q << 30);

            context.Assembler.WriteInstructionRm16(instruction, rd, rn, rm);
        }

        private static void GenerateVectorBinary(
            CodeGenContext context,
            uint q,
            uint sz,
            uint instruction,
            Operand rd,
            Operand rn,
            Operand rm)
        {
            instruction |= (q << 30) | (sz << 22);

            context.Assembler.WriteInstructionRm16(instruction, rd, rn, rm);
        }

        private static void GenerateVectorBinaryByElem(
            CodeGenContext context,
            uint q,
            uint size,
            uint instruction,
            uint srcIndex,
            Operand rd,
            Operand rn,
            Operand rm)
        {
            instruction |= (q << 30) | (size << 22);

            if (size == 2)
            {
                instruction |= ((srcIndex & 1) << 21) | ((srcIndex & 2) << 10);
            }
            else
            {
                instruction |= ((srcIndex & 3) << 20) | ((srcIndex & 4) << 9);
            }

            context.Assembler.WriteInstructionRm16(instruction, rd, rn, rm);
        }

        private static void GenerateVectorBinaryFPByElem(
            CodeGenContext context,
            uint q,
            uint sz,
            uint instruction,
            uint srcIndex,
            Operand rd,
            Operand rn,
            Operand rm)
        {
            instruction |= (q << 30) | (sz << 22);

            if (sz != 0)
            {
                instruction |= (srcIndex & 1) << 11;
            }
            else
            {
                instruction |= ((srcIndex & 1) << 21) | ((srcIndex & 2) << 10);
            }

            context.Assembler.WriteInstructionRm16(instruction, rd, rn, rm);
        }

        private static void GenerateVectorBinaryShlImm(
            CodeGenContext context,
            uint q,
            uint sz,
            uint instruction,
            Operand rd,
            Operand rn,
            uint shift)
        {
            instruction |= (q << 30);

            Debug.Assert(shift >= 0 && shift < (8u << (int)sz));

            uint imm = (8u << (int)sz) | (shift & (0x3fu >> (int)(3 - sz)));

            instruction |= (imm << 16);

            context.Assembler.WriteInstruction(instruction, rd, rn);
        }

        private static void GenerateVectorBinaryShrImm(
            CodeGenContext context,
            uint q,
            uint sz,
            uint instruction,
            Operand rd,
            Operand rn,
            uint shift)
        {
            instruction |= (q << 30);

            Debug.Assert(shift > 0 && shift <= (8u << (int)sz));

            uint imm = (8u << (int)sz) | ((8u << (int)sz) - shift);

            instruction |= (imm << 16);

            context.Assembler.WriteInstruction(instruction, rd, rn);
        }

        private static void GenerateVectorInsertByElem(
            CodeGenContext context,
            uint sz,
            uint instruction,
            uint srcIndex,
            uint dstIndex,
            Operand rd,
            Operand rn)
        {
            uint imm4 = srcIndex << (int)sz;
            uint imm5 = (dstIndex << ((int)sz + 1)) | (1u << (int)sz);

            instruction |= imm4 << 11;
            instruction |= imm5 << 16;

            context.Assembler.WriteInstruction(instruction, rd, rn);
        }
    }
}
