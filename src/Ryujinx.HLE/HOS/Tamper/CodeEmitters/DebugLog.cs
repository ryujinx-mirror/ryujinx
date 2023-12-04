using Ryujinx.HLE.Exceptions;
using Ryujinx.HLE.HOS.Tamper.Operations;

namespace Ryujinx.HLE.HOS.Tamper.CodeEmitters
{
    /// <summary>
    /// Code type 0xFFF writes a debug log.
    /// </summary>
    class DebugLog
    {
        private const int OperationWidthIndex = 3;
        private const int LogIdIndex = 4;
        private const int OperandTypeIndex = 5;
        private const int RegisterOrMemoryRegionIndex = 6;
        private const int OffsetRegisterOrImmediateIndex = 7;

        private const int MemoryRegionWithOffsetImmediate = 0;
        private const int MemoryRegionWithOffsetRegister = 1;
        private const int AddressRegisterWithOffsetImmediate = 2;
        private const int AddressRegisterWithOffsetRegister = 3;
        private const int ValueRegister = 4;

        private const int OffsetImmediateSize = 9;

        public static void Emit(byte[] instruction, CompilationContext context)
        {
            // FFFTIX##
            // FFFTI0Ma aaaaaaaa
            // FFFTI1Mr
            // FFFTI2Ra aaaaaaaa
            // FFFTI3Rr
            // FFFTI4V0
            // T: Width of memory write (1, 2, 4, or 8 bytes).
            // I: Log id.
            // X: Operand Type, see below.
            // M: Memory Type (operand types 0 and 1).
            // R: Address Register (operand types 2 and 3).
            // a: Relative Address (operand types 0 and 2).
            // r: Offset Register (operand types 1 and 3).
            // V: Value Register (operand type 4).

            byte operationWidth = instruction[OperationWidthIndex];
            byte logId = instruction[LogIdIndex];
            byte operandType = instruction[OperandTypeIndex];
            byte registerOrMemoryRegion = instruction[RegisterOrMemoryRegionIndex];
            byte offsetRegisterIndex = instruction[OffsetRegisterOrImmediateIndex];
            ulong immediate;
            Register addressRegister;
            Register offsetRegister;
            IOperand sourceOperand;

            switch (operandType)
            {
                case MemoryRegionWithOffsetImmediate:
                    // *(?x + #a)
                    immediate = InstructionHelper.GetImmediate(instruction, OffsetRegisterOrImmediateIndex, OffsetImmediateSize);
                    sourceOperand = MemoryHelper.EmitPointer((MemoryRegion)registerOrMemoryRegion, immediate, context);
                    break;
                case MemoryRegionWithOffsetRegister:
                    // *(?x + $r)
                    offsetRegister = context.GetRegister(offsetRegisterIndex);
                    sourceOperand = MemoryHelper.EmitPointer((MemoryRegion)registerOrMemoryRegion, offsetRegister, context);
                    break;
                case AddressRegisterWithOffsetImmediate:
                    // *($R + #a)
                    addressRegister = context.GetRegister(registerOrMemoryRegion);
                    immediate = InstructionHelper.GetImmediate(instruction, OffsetRegisterOrImmediateIndex, OffsetImmediateSize);
                    sourceOperand = MemoryHelper.EmitPointer(addressRegister, immediate, context);
                    break;
                case AddressRegisterWithOffsetRegister:
                    // *($R + $r)
                    addressRegister = context.GetRegister(registerOrMemoryRegion);
                    offsetRegister = context.GetRegister(offsetRegisterIndex);
                    sourceOperand = MemoryHelper.EmitPointer(addressRegister, offsetRegister, context);
                    break;
                case ValueRegister:
                    // $V
                    sourceOperand = context.GetRegister(registerOrMemoryRegion);
                    break;
                default:
                    throw new TamperCompilationException($"Invalid operand type {operandType} in Atmosphere cheat");
            }

            InstructionHelper.Emit(typeof(OpLog<>), operationWidth, context, logId, sourceOperand);
        }
    }
}
