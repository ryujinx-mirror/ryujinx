using ARMeilleure.IntermediateRepresentation;
using System;

using static ARMeilleure.IntermediateRepresentation.OperandHelper;

namespace ARMeilleure.CodeGen.Optimizations
{
    static class Simplification
    {
        public static void RunPass(Operation operation)
        {
            switch (operation.Instruction)
            {
                case Instruction.Add:
                    if (operation.GetSource(0).Relocatable ||
                        operation.GetSource(1).Relocatable)
                    {
                        break;
                    }

                    TryEliminateBinaryOpComutative(operation, 0);
                    break;

                case Instruction.BitwiseAnd:
                    TryEliminateBitwiseAnd(operation);
                    break;

                case Instruction.BitwiseOr:
                    TryEliminateBitwiseOr(operation);
                    break;

                case Instruction.BitwiseExclusiveOr:
                    TryEliminateBitwiseExclusiveOr(operation);
                    break;

                case Instruction.ConditionalSelect:
                    TryEliminateConditionalSelect(operation);
                    break;

                case Instruction.Divide:
                    TryEliminateBinaryOpY(operation, 1);
                    break;

                case Instruction.Multiply:
                    TryEliminateBinaryOpComutative(operation, 1);
                    break;

                case Instruction.ShiftLeft:
                case Instruction.ShiftRightSI:
                case Instruction.ShiftRightUI:
                case Instruction.Subtract:
                    TryEliminateBinaryOpY(operation, 0);
                    break;
            }
        }

        private static void TryEliminateBitwiseAnd(Operation operation)
        {
            // Try to recognize and optimize those 3 patterns (in order):
            // x & 0xFFFFFFFF == x,          0xFFFFFFFF & y == y,
            // x & 0x00000000 == 0x00000000, 0x00000000 & y == 0x00000000
            Operand x = operation.GetSource(0);
            Operand y = operation.GetSource(1);

            if (IsConstEqual(x, AllOnes(x.Type)))
            {
                operation.TurnIntoCopy(y);
            }
            else if (IsConstEqual(y, AllOnes(y.Type)))
            {
                operation.TurnIntoCopy(x);
            }
            else if (IsConstEqual(x, 0) || IsConstEqual(y, 0))
            {
                operation.TurnIntoCopy(Const(0));
            }
        }

        private static void TryEliminateBitwiseOr(Operation operation)
        {
            // Try to recognize and optimize those 3 patterns (in order):
            // x | 0x00000000 == x,          0x00000000 | y == y,
            // x | 0xFFFFFFFF == 0xFFFFFFFF, 0xFFFFFFFF | y == 0xFFFFFFFF
            Operand x = operation.GetSource(0);
            Operand y = operation.GetSource(1);

            if (IsConstEqual(x, 0))
            {
                operation.TurnIntoCopy(y);
            }
            else if (IsConstEqual(y, 0))
            {
                operation.TurnIntoCopy(x);
            }
            else if (IsConstEqual(x, AllOnes(x.Type)) || IsConstEqual(y, AllOnes(y.Type)))
            {
                operation.TurnIntoCopy(Const(AllOnes(x.Type)));
            }
        }

        private static void TryEliminateBitwiseExclusiveOr(Operation operation)
        {
            // Try to recognize and optimize those 2 patterns (in order):
            // x ^ y == 0x00000000 when x == y
            // 0x00000000 ^ y == y, x ^ 0x00000000 == x
            Operand x = operation.GetSource(0);
            Operand y = operation.GetSource(1);

            if (x == y && x.Type.IsInteger())
            {
                operation.TurnIntoCopy(Const(x.Type, 0));
            }
            else
            {
                TryEliminateBinaryOpComutative(operation, 0);
            }
        }

        private static void TryEliminateBinaryOpY(Operation operation, ulong comparand)
        {
            Operand x = operation.GetSource(0);
            Operand y = operation.GetSource(1);

            if (IsConstEqual(y, comparand))
            {
                operation.TurnIntoCopy(x);
            }
        }

        private static void TryEliminateBinaryOpComutative(Operation operation, ulong comparand)
        {
            Operand x = operation.GetSource(0);
            Operand y = operation.GetSource(1);

            if (IsConstEqual(x, comparand))
            {
                operation.TurnIntoCopy(y);
            }
            else if (IsConstEqual(y, comparand))
            {
                operation.TurnIntoCopy(x);
            }
        }

        private static void TryEliminateConditionalSelect(Operation operation)
        {
            Operand cond = operation.GetSource(0);

            if (cond.Kind != OperandKind.Constant)
            {
                return;
            }

            // The condition is constant, we can turn it into a copy, and select
            // the source based on the condition value.
            int srcIndex = cond.Value != 0 ? 1 : 2;

            Operand source = operation.GetSource(srcIndex);

            operation.TurnIntoCopy(source);
        }

        private static bool IsConstEqual(Operand operand, ulong comparand)
        {
            if (operand.Kind != OperandKind.Constant || !operand.Type.IsInteger())
            {
                return false;
            }

            return operand.Value == comparand;
        }

        private static ulong AllOnes(OperandType type)
        {
            switch (type)
            {
                case OperandType.I32: return ~0U;
                case OperandType.I64: return ~0UL;
            }

            throw new ArgumentException("Invalid operand type \"" + type + "\".");
        }
    }
}