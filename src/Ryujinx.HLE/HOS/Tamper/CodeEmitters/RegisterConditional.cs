using Ryujinx.HLE.Exceptions;
using Ryujinx.HLE.HOS.Tamper.Conditions;
using Ryujinx.HLE.HOS.Tamper.Operations;

namespace Ryujinx.HLE.HOS.Tamper.CodeEmitters
{
    /// <summary>
    /// Code type 0xC0 performs a comparison of the contents of a register and another value.
    /// This code support multiple operand types, see below. If the condition is not met,
    /// all instructions until the appropriate conditional block terminator are skipped.
    /// </summary>
    class RegisterConditional
    {
        private const int OperationWidthIndex = 2;
        private const int ComparisonTypeIndex = 3;
        private const int SourceRegisterIndex = 4;
        private const int OperandTypeIndex = 5;
        private const int RegisterOrMemoryRegionIndex = 6;
        private const int OffsetImmediateIndex = 7;
        private const int ValueImmediateIndex = 8;

        private const int MemoryRegionWithOffsetImmediate = 0;
        private const int MemoryRegionWithOffsetRegister = 1;
        private const int AddressRegisterWithOffsetImmediate = 2;
        private const int AddressRegisterWithOffsetRegister = 3;
        private const int OffsetImmediate = 4;
        private const int AddressRegister = 5;

        private const int OffsetImmediateSize = 9;
        private const int ValueImmediateSize8 = 8;
        private const int ValueImmediateSize16 = 16;

        public static ICondition Emit(byte[] instruction, CompilationContext context)
        {
            // C0TcSX##
            // C0TcS0Ma aaaaaaaa
            // C0TcS1Mr
            // C0TcS2Ra aaaaaaaa
            // C0TcS3Rr
            // C0TcS400 VVVVVVVV (VVVVVVVV)
            // C0TcS5X0
            // T: Width of memory write(1, 2, 4, or 8 bytes).
            // c: Condition to use, see below.
            // S: Source Register.
            // X: Operand Type, see below.
            // M: Memory Type(operand types 0 and 1).
            // R: Address Register(operand types 2 and 3).
            // a: Relative Address(operand types 0 and 2).
            // r: Offset Register(operand types 1 and 3).
            // X: Other Register(operand type 5).
            // V: Value to compare to(operand type 4).

            byte operationWidth = instruction[OperationWidthIndex];
            Comparison comparison = (Comparison)instruction[ComparisonTypeIndex];
            Register sourceRegister = context.GetRegister(instruction[SourceRegisterIndex]);
            byte operandType = instruction[OperandTypeIndex];
            byte registerOrMemoryRegion = instruction[RegisterOrMemoryRegionIndex];
            byte offsetRegisterIndex = instruction[OffsetImmediateIndex];
            ulong offsetImmediate;
            ulong valueImmediate;
            int valueImmediateSize;
            Register addressRegister;
            Register offsetRegister;
            IOperand sourceOperand;

            switch (operandType)
            {
                case MemoryRegionWithOffsetImmediate:
                    // *(?x + #a)
                    offsetImmediate = InstructionHelper.GetImmediate(instruction, OffsetImmediateIndex, OffsetImmediateSize);
                    sourceOperand = MemoryHelper.EmitPointer((MemoryRegion)registerOrMemoryRegion, offsetImmediate, context);
                    break;
                case MemoryRegionWithOffsetRegister:
                    // *(?x + $r)
                    offsetRegister = context.GetRegister(offsetRegisterIndex);
                    sourceOperand = MemoryHelper.EmitPointer((MemoryRegion)registerOrMemoryRegion, offsetRegister, context);
                    break;
                case AddressRegisterWithOffsetImmediate:
                    // *($R + #a)
                    addressRegister = context.GetRegister(registerOrMemoryRegion);
                    offsetImmediate = InstructionHelper.GetImmediate(instruction, OffsetImmediateIndex, OffsetImmediateSize);
                    sourceOperand = MemoryHelper.EmitPointer(addressRegister, offsetImmediate, context);
                    break;
                case AddressRegisterWithOffsetRegister:
                    // *($R + $r)
                    addressRegister = context.GetRegister(registerOrMemoryRegion);
                    offsetRegister = context.GetRegister(offsetRegisterIndex);
                    sourceOperand = MemoryHelper.EmitPointer(addressRegister, offsetRegister, context);
                    break;
                case OffsetImmediate:
                    valueImmediateSize = operationWidth <= 4 ? ValueImmediateSize8 : ValueImmediateSize16;
                    valueImmediate = InstructionHelper.GetImmediate(instruction, ValueImmediateIndex, valueImmediateSize);
                    sourceOperand = new Value<ulong>(valueImmediate);
                    break;
                case AddressRegister:
                    // $V
                    sourceOperand = context.GetRegister(registerOrMemoryRegion);
                    break;
                default:
                    throw new TamperCompilationException($"Invalid operand type {operandType} in Atmosphere cheat");
            }

            return InstructionHelper.CreateCondition(comparison, operationWidth, sourceRegister, sourceOperand);
        }
    }
}
