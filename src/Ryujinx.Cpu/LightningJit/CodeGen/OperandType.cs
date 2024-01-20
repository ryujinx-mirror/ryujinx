using System;

namespace Ryujinx.Cpu.LightningJit.CodeGen
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
            return type == OperandType.I32 || type == OperandType.I64;
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
    }
}
