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
        V128,
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
            return type switch
            {
                OperandType.FP32 => RegisterType.Vector,
                OperandType.FP64 => RegisterType.Vector,
                OperandType.I32 => RegisterType.Integer,
                OperandType.I64 => RegisterType.Integer,
                OperandType.V128 => RegisterType.Vector,
                _ => throw new InvalidOperationException($"Invalid operand type \"{type}\"."),
            };
        }

        public static int GetSizeInBytes(this OperandType type)
        {
            return type switch
            {
                OperandType.FP32 => 4,
                OperandType.FP64 => 8,
                OperandType.I32 => 4,
                OperandType.I64 => 8,
                OperandType.V128 => 16,
                _ => throw new InvalidOperationException($"Invalid operand type \"{type}\"."),
            };
        }

        public static int GetSizeInBytesLog2(this OperandType type)
        {
            return type switch
            {
                OperandType.FP32 => 2,
                OperandType.FP64 => 3,
                OperandType.I32 => 2,
                OperandType.I64 => 3,
                OperandType.V128 => 4,
                _ => throw new InvalidOperationException($"Invalid operand type \"{type}\"."),
            };
        }
    }
}
