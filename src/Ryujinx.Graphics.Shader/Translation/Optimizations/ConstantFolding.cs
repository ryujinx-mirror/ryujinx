using Ryujinx.Common.Utilities;
using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using System;
using static Ryujinx.Graphics.Shader.IntermediateRepresentation.OperandHelper;

namespace Ryujinx.Graphics.Shader.Translation.Optimizations
{
    static class ConstantFolding
    {
        public static void RunPass(ResourceManager resourceManager, Operation operation)
        {
            if (!AreAllSourcesConstant(operation))
            {
                return;
            }

            switch (operation.Inst)
            {
                case Instruction.Add:
                    EvaluateBinary(operation, (x, y) => x + y);
                    break;

                case Instruction.BitCount:
                    EvaluateUnary(operation, (x) => BitCount(x));
                    break;

                case Instruction.BitwiseAnd:
                    EvaluateBinary(operation, (x, y) => x & y);
                    break;

                case Instruction.BitwiseExclusiveOr:
                    EvaluateBinary(operation, (x, y) => x ^ y);
                    break;

                case Instruction.BitwiseNot:
                    EvaluateUnary(operation, (x) => ~x);
                    break;

                case Instruction.BitwiseOr:
                    EvaluateBinary(operation, (x, y) => x | y);
                    break;

                case Instruction.BitfieldExtractS32:
                    BitfieldExtractS32(operation);
                    break;

                case Instruction.BitfieldExtractU32:
                    BitfieldExtractU32(operation);
                    break;

                case Instruction.Clamp:
                    EvaluateTernary(operation, (x, y, z) => Math.Clamp(x, y, z));
                    break;

                case Instruction.ClampU32:
                    EvaluateTernary(operation, (x, y, z) => (int)Math.Clamp((uint)x, (uint)y, (uint)z));
                    break;

                case Instruction.CompareEqual:
                    EvaluateBinary(operation, (x, y) => x == y);
                    break;

                case Instruction.CompareGreater:
                    EvaluateBinary(operation, (x, y) => x > y);
                    break;

                case Instruction.CompareGreaterOrEqual:
                    EvaluateBinary(operation, (x, y) => x >= y);
                    break;

                case Instruction.CompareGreaterOrEqualU32:
                    EvaluateBinary(operation, (x, y) => (uint)x >= (uint)y);
                    break;

                case Instruction.CompareGreaterU32:
                    EvaluateBinary(operation, (x, y) => (uint)x > (uint)y);
                    break;

                case Instruction.CompareLess:
                    EvaluateBinary(operation, (x, y) => x < y);
                    break;

                case Instruction.CompareLessOrEqual:
                    EvaluateBinary(operation, (x, y) => x <= y);
                    break;

                case Instruction.CompareLessOrEqualU32:
                    EvaluateBinary(operation, (x, y) => (uint)x <= (uint)y);
                    break;

                case Instruction.CompareLessU32:
                    EvaluateBinary(operation, (x, y) => (uint)x < (uint)y);
                    break;

                case Instruction.CompareNotEqual:
                    EvaluateBinary(operation, (x, y) => x != y);
                    break;

                case Instruction.Divide:
                    EvaluateBinary(operation, (x, y) => y != 0 ? x / y : 0);
                    break;

                case Instruction.FP32 | Instruction.Add:
                    EvaluateFPBinary(operation, (x, y) => x + y);
                    break;

                case Instruction.FP32 | Instruction.Clamp:
                    EvaluateFPTernary(operation, (x, y, z) => Math.Clamp(x, y, z));
                    break;

                case Instruction.FP32 | Instruction.CompareEqual:
                    EvaluateFPBinary(operation, (x, y) => x == y);
                    break;

                case Instruction.FP32 | Instruction.CompareGreater:
                    EvaluateFPBinary(operation, (x, y) => x > y);
                    break;

                case Instruction.FP32 | Instruction.CompareGreaterOrEqual:
                    EvaluateFPBinary(operation, (x, y) => x >= y);
                    break;

                case Instruction.FP32 | Instruction.CompareLess:
                    EvaluateFPBinary(operation, (x, y) => x < y);
                    break;

                case Instruction.FP32 | Instruction.CompareLessOrEqual:
                    EvaluateFPBinary(operation, (x, y) => x <= y);
                    break;

                case Instruction.FP32 | Instruction.CompareNotEqual:
                    EvaluateFPBinary(operation, (x, y) => x != y);
                    break;

                case Instruction.FP32 | Instruction.Divide:
                    EvaluateFPBinary(operation, (x, y) => x / y);
                    break;

                case Instruction.FP32 | Instruction.Multiply:
                    EvaluateFPBinary(operation, (x, y) => x * y);
                    break;

                case Instruction.FP32 | Instruction.Negate:
                    EvaluateFPUnary(operation, (x) => -x);
                    break;

                case Instruction.FP32 | Instruction.Subtract:
                    EvaluateFPBinary(operation, (x, y) => x - y);
                    break;

                case Instruction.IsNan:
                    EvaluateFPUnary(operation, (x) => float.IsNaN(x));
                    break;

                case Instruction.Load:
                    if (operation.StorageKind == StorageKind.ConstantBuffer && operation.SourcesCount == 4)
                    {
                        int binding = operation.GetSource(0).Value;
                        int fieldIndex = operation.GetSource(1).Value;

                        if (resourceManager.TryGetConstantBufferSlot(binding, out int cbufSlot) && fieldIndex == 0)
                        {
                            int vecIndex = operation.GetSource(2).Value;
                            int elemIndex = operation.GetSource(3).Value;
                            int cbufOffset = vecIndex * 4 + elemIndex;

                            operation.TurnIntoCopy(Cbuf(cbufSlot, cbufOffset));
                        }
                    }
                    break;

                case Instruction.Maximum:
                    EvaluateBinary(operation, (x, y) => Math.Max(x, y));
                    break;

                case Instruction.MaximumU32:
                    EvaluateBinary(operation, (x, y) => (int)Math.Max((uint)x, (uint)y));
                    break;

                case Instruction.Minimum:
                    EvaluateBinary(operation, (x, y) => Math.Min(x, y));
                    break;

                case Instruction.MinimumU32:
                    EvaluateBinary(operation, (x, y) => (int)Math.Min((uint)x, (uint)y));
                    break;

                case Instruction.Multiply:
                    EvaluateBinary(operation, (x, y) => x * y);
                    break;

                case Instruction.Negate:
                    EvaluateUnary(operation, (x) => -x);
                    break;

                case Instruction.ShiftLeft:
                    EvaluateBinary(operation, (x, y) => x << y);
                    break;

                case Instruction.ShiftRightS32:
                    EvaluateBinary(operation, (x, y) => x >> y);
                    break;

                case Instruction.ShiftRightU32:
                    EvaluateBinary(operation, (x, y) => (int)((uint)x >> y));
                    break;

                case Instruction.Subtract:
                    EvaluateBinary(operation, (x, y) => x - y);
                    break;

                case Instruction.UnpackHalf2x16:
                    UnpackHalf2x16(operation);
                    break;
            }
        }

        private static bool AreAllSourcesConstant(Operation operation)
        {
            for (int index = 0; index < operation.SourcesCount; index++)
            {
                if (operation.GetSource(index).Type != OperandType.Constant)
                {
                    return false;
                }
            }

            return true;
        }

        private static int BitCount(int value)
        {
            int count = 0;

            for (int bit = 0; bit < 32; bit++)
            {
                if (value.Extract(bit))
                {
                    count++;
                }
            }

            return count;
        }

        private static void BitfieldExtractS32(Operation operation)
        {
            int value = GetBitfieldExtractValue(operation);

            int shift = 32 - operation.GetSource(2).Value;

            value = (value << shift) >> shift;

            operation.TurnIntoCopy(Const(value));
        }

        private static void BitfieldExtractU32(Operation operation)
        {
            operation.TurnIntoCopy(Const(GetBitfieldExtractValue(operation)));
        }

        private static int GetBitfieldExtractValue(Operation operation)
        {
            int value = operation.GetSource(0).Value;
            int lsb = operation.GetSource(1).Value;
            int length = operation.GetSource(2).Value;

            return value.Extract(lsb, length);
        }

        private static void UnpackHalf2x16(Operation operation)
        {
            int value = operation.GetSource(0).Value;

            value = (value >> operation.Index * 16) & 0xffff;

            operation.TurnIntoCopy(ConstF((float)BitConverter.UInt16BitsToHalf((ushort)value)));
        }

        private static void EvaluateUnary(Operation operation, Func<int, int> op)
        {
            int x = operation.GetSource(0).Value;

            operation.TurnIntoCopy(Const(op(x)));
        }

        private static void EvaluateFPUnary(Operation operation, Func<float, float> op)
        {
            float x = operation.GetSource(0).AsFloat();

            operation.TurnIntoCopy(ConstF(op(x)));
        }

        private static void EvaluateFPUnary(Operation operation, Func<float, bool> op)
        {
            float x = operation.GetSource(0).AsFloat();

            operation.TurnIntoCopy(Const(op(x) ? IrConsts.True : IrConsts.False));
        }

        private static void EvaluateBinary(Operation operation, Func<int, int, int> op)
        {
            int x = operation.GetSource(0).Value;
            int y = operation.GetSource(1).Value;

            operation.TurnIntoCopy(Const(op(x, y)));
        }

        private static void EvaluateBinary(Operation operation, Func<int, int, bool> op)
        {
            int x = operation.GetSource(0).Value;
            int y = operation.GetSource(1).Value;

            operation.TurnIntoCopy(Const(op(x, y) ? IrConsts.True : IrConsts.False));
        }

        private static void EvaluateFPBinary(Operation operation, Func<float, float, float> op)
        {
            float x = operation.GetSource(0).AsFloat();
            float y = operation.GetSource(1).AsFloat();

            operation.TurnIntoCopy(ConstF(op(x, y)));
        }

        private static void EvaluateFPBinary(Operation operation, Func<float, float, bool> op)
        {
            float x = operation.GetSource(0).AsFloat();
            float y = operation.GetSource(1).AsFloat();

            operation.TurnIntoCopy(Const(op(x, y) ? IrConsts.True : IrConsts.False));
        }

        private static void EvaluateTernary(Operation operation, Func<int, int, int, int> op)
        {
            int x = operation.GetSource(0).Value;
            int y = operation.GetSource(1).Value;
            int z = operation.GetSource(2).Value;

            operation.TurnIntoCopy(Const(op(x, y, z)));
        }

        private static void EvaluateFPTernary(Operation operation, Func<float, float, float, float> op)
        {
            float x = operation.GetSource(0).AsFloat();
            float y = operation.GetSource(1).AsFloat();
            float z = operation.GetSource(2).AsFloat();

            operation.TurnIntoCopy(ConstF(op(x, y, z)));
        }
    }
}
