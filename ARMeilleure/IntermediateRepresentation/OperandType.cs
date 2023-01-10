using System;

namespace ARMeilleure.IntermediateRepresentation
{
    enum OperandType
    {
        None,
        I32,
        I64,
        FP32,
        FP64,
        V128
    }

    static class OperandTypeExtensions
    {
        public static bool IsInteger(this OperandType type)
        {
            return type == OperandType.I32 ||
                   type == OperandType.I64;
        }

        public static RegisterType ToRegisterType(this OperandType type)
        {
            switch (type)
            {
                case OperandType.FP32: return RegisterType.Vector;
                case OperandType.FP64: return RegisterType.Vector;
                case OperandType.I32:  return RegisterType.Integer;
                case OperandType.I64:  return RegisterType.Integer;
                case OperandType.V128: return RegisterType.Vector;
            }

            throw new InvalidOperationException($"Invalid operand type \"{type}\".");
        }

        public static int GetSizeInBytes(this OperandType type)
        {
            switch (type)
            {
                case OperandType.FP32: return 4;
                case OperandType.FP64: return 8;
                case OperandType.I32:  return 4;
                case OperandType.I64:  return 8;
                case OperandType.V128: return 16;
            }

            throw new InvalidOperationException($"Invalid operand type \"{type}\".");
        }

        public static int GetSizeInBytesLog2(this OperandType type)
        {
            switch (type)
            {
                case OperandType.FP32: return 2;
                case OperandType.FP64: return 3;
                case OperandType.I32:  return 2;
                case OperandType.I64:  return 3;
                case OperandType.V128: return 4;
            }

            throw new InvalidOperationException($"Invalid operand type \"{type}\".");
        }
    }
}