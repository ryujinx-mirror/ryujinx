using ARMeilleure.IntermediateRepresentation;
using System;

using static ARMeilleure.IntermediateRepresentation.OperandHelper;

namespace ARMeilleure.CodeGen.Optimizations
{
    static class ConstantFolding
    {
        public static void RunPass(Operation operation)
        {
            if (operation.Destination == null || operation.SourcesCount == 0)
            {
                return;
            }

            if (!AreAllSourcesConstant(operation))
            {
                return;
            }

            OperandType type = operation.Destination.Type;

            switch (operation.Instruction)
            {
                case Instruction.Add:
                    if (operation.GetSource(0).Relocatable ||
                        operation.GetSource(1).Relocatable)
                    {
                        break;
                    }

                    if (type == OperandType.I32)
                    {
                        EvaluateBinaryI32(operation, (x, y) => x + y);
                    }
                    else if (type == OperandType.I64)
                    {
                        EvaluateBinaryI64(operation, (x, y) => x + y);
                    }
                    break;

                case Instruction.BitwiseAnd:
                    if (type == OperandType.I32)
                    {
                        EvaluateBinaryI32(operation, (x, y) => x & y);
                    }
                    else if (type == OperandType.I64)
                    {
                        EvaluateBinaryI64(operation, (x, y) => x & y);
                    }
                    break;

                case Instruction.BitwiseExclusiveOr:
                    if (type == OperandType.I32)
                    {
                        EvaluateBinaryI32(operation, (x, y) => x ^ y);
                    }
                    else if (type == OperandType.I64)
                    {
                        EvaluateBinaryI64(operation, (x, y) => x ^ y);
                    }
                    break;

                case Instruction.BitwiseNot:
                    if (type == OperandType.I32)
                    {
                        EvaluateUnaryI32(operation, (x) => ~x);
                    }
                    else if (type == OperandType.I64)
                    {
                        EvaluateUnaryI64(operation, (x) => ~x);
                    }
                    break;

                case Instruction.BitwiseOr:
                    if (type == OperandType.I32)
                    {
                        EvaluateBinaryI32(operation, (x, y) => x | y);
                    }
                    else if (type == OperandType.I64)
                    {
                        EvaluateBinaryI64(operation, (x, y) => x | y);
                    }
                    break;

                case Instruction.ConvertI64ToI32:
                    if (type == OperandType.I32)
                    {
                        EvaluateUnaryI32(operation, (x) => x);
                    }
                    break;

                case Instruction.Copy:
                    if (type == OperandType.I32)
                    {
                        EvaluateUnaryI32(operation, (x) => x);
                    }
                    else if (type == OperandType.I64)
                    {
                        EvaluateUnaryI64(operation, (x) => x);
                    }
                    break;

                case Instruction.Divide:
                    if (type == OperandType.I32)
                    {
                        EvaluateBinaryI32(operation, (x, y) => y != 0 ? x / y : 0);
                    }
                    else if (type == OperandType.I64)
                    {
                        EvaluateBinaryI64(operation, (x, y) => y != 0 ? x / y : 0);
                    }
                    break;

                case Instruction.DivideUI:
                    if (type == OperandType.I32)
                    {
                        EvaluateBinaryI32(operation, (x, y) => y != 0 ? (int)((uint)x / (uint)y) : 0);
                    }
                    else if (type == OperandType.I64)
                    {
                        EvaluateBinaryI64(operation, (x, y) => y != 0 ? (long)((ulong)x / (ulong)y) : 0);
                    }
                    break;

                 case Instruction.Multiply:
                    if (type == OperandType.I32)
                    {
                        EvaluateBinaryI32(operation, (x, y) => x * y);
                    }
                    else if (type == OperandType.I64)
                    {
                        EvaluateBinaryI64(operation, (x, y) => x * y);
                    }
                    break;

                case Instruction.Negate:
                    if (type == OperandType.I32)
                    {
                        EvaluateUnaryI32(operation, (x) => -x);
                    }
                    else if (type == OperandType.I64)
                    {
                        EvaluateUnaryI64(operation, (x) => -x);
                    }
                    break;

                case Instruction.ShiftLeft:
                    if (type == OperandType.I32)
                    {
                        EvaluateBinaryI32(operation, (x, y) => x << y);
                    }
                    else if (type == OperandType.I64)
                    {
                        EvaluateBinaryI64(operation, (x, y) => x << (int)y);
                    }
                    break;

                case Instruction.ShiftRightSI:
                    if (type == OperandType.I32)
                    {
                        EvaluateBinaryI32(operation, (x, y) => x >> y);
                    }
                    else if (type == OperandType.I64)
                    {
                        EvaluateBinaryI64(operation, (x, y) => x >> (int)y);
                    }
                    break;

                case Instruction.ShiftRightUI:
                    if (type == OperandType.I32)
                    {
                        EvaluateBinaryI32(operation, (x, y) => (int)((uint)x >> y));
                    }
                    else if (type == OperandType.I64)
                    {
                        EvaluateBinaryI64(operation, (x, y) => (long)((ulong)x >> (int)y));
                    }
                    break;

                case Instruction.SignExtend16:
                    if (type == OperandType.I32)
                    {
                        EvaluateUnaryI32(operation, (x) => (short)x);
                    }
                    else if (type == OperandType.I64)
                    {
                        EvaluateUnaryI64(operation, (x) => (short)x);
                    }
                    break;

                case Instruction.SignExtend32:
                    if (type == OperandType.I32)
                    {
                        EvaluateUnaryI32(operation, (x) => x);
                    }
                    else if (type == OperandType.I64)
                    {
                        EvaluateUnaryI64(operation, (x) => (int)x);
                    }
                    break;

                case Instruction.SignExtend8:
                    if (type == OperandType.I32)
                    {
                        EvaluateUnaryI32(operation, (x) => (sbyte)x);
                    }
                    else if (type == OperandType.I64)
                    {
                        EvaluateUnaryI64(operation, (x) => (sbyte)x);
                    }
                    break;

                case Instruction.ZeroExtend16:
                    if (type == OperandType.I32)
                    {
                        EvaluateUnaryI32(operation, (x) => (ushort)x);
                    }
                    else if (type == OperandType.I64)
                    {
                        EvaluateUnaryI64(operation, (x) => (ushort)x);
                    }
                    break;

                case Instruction.ZeroExtend32:
                    if (type == OperandType.I32)
                    {
                        EvaluateUnaryI32(operation, (x) => x);
                    }
                    else if (type == OperandType.I64)
                    {
                        EvaluateUnaryI64(operation, (x) => (uint)x);
                    }
                    break;

                case Instruction.ZeroExtend8:
                    if (type == OperandType.I32)
                    {
                        EvaluateUnaryI32(operation, (x) => (byte)x);
                    }
                    else if (type == OperandType.I64)
                    {
                        EvaluateUnaryI64(operation, (x) => (byte)x);
                    }
                    break;

                case Instruction.Subtract:
                    if (type == OperandType.I32)
                    {
                        EvaluateBinaryI32(operation, (x, y) => x - y);
                    }
                    else if (type == OperandType.I64)
                    {
                        EvaluateBinaryI64(operation, (x, y) => x - y);
                    }
                    break;
            }
        }

        private static bool AreAllSourcesConstant(Operation operation)
        {
            for (int index = 0; index < operation.SourcesCount; index++)
            {
                Operand srcOp = operation.GetSource(index);

                if (srcOp.Kind != OperandKind.Constant)
                {
                    return false;
                }
            }

            return true;
        }

        private static void EvaluateUnaryI32(Operation operation, Func<int, int> op)
        {
            int x = operation.GetSource(0).AsInt32();

            operation.TurnIntoCopy(Const(op(x)));
        }

        private static void EvaluateUnaryI64(Operation operation, Func<long, long> op)
        {
            long x = operation.GetSource(0).AsInt64();

            operation.TurnIntoCopy(Const(op(x)));
        }

        private static void EvaluateBinaryI32(Operation operation, Func<int, int, int> op)
        {
            int x = operation.GetSource(0).AsInt32();
            int y = operation.GetSource(1).AsInt32();

            operation.TurnIntoCopy(Const(op(x, y)));
        }

        private static void EvaluateBinaryI64(Operation operation, Func<long, long, long> op)
        {
            long x = operation.GetSource(0).AsInt64();
            long y = operation.GetSource(1).AsInt64();

            operation.TurnIntoCopy(Const(op(x, y)));
        }
    }
}