using Ryujinx.HLE.Exceptions;
using Ryujinx.HLE.HOS.Tamper.Operations;

namespace Ryujinx.HLE.HOS.Tamper.CodeEmitters
{
    /// <summary>
    /// Code type 10 allows writing a register to memory.
    /// </summary>
    class StoreRegisterToMemory
    {
        private const int OperationWidthIndex = 1;
        private const int SourceRegisterIndex = 2;
        private const int AddressRegisterIndex = 3;
        private const int IncrementAddressRegisterIndex = 4;
        private const int AddressingTypeIndex = 5;
        private const int RegisterOrMemoryRegionIndex = 6;
        private const int OffsetImmediateIndex = 7;

        private const int AddressRegister = 0;
        private const int AddressRegisterWithOffsetRegister = 1;
        private const int OffsetImmediate = 2;
        private const int MemoryRegionWithOffsetRegister = 3;
        private const int MemoryRegionWithOffsetImmediate = 4;
        private const int MemoryRegionWithOffsetRegisterAndImmediate = 5;

        private const int OffsetImmediateSize1 = 1;
        private const int OffsetImmediateSize9 = 9;

        public static void Emit(byte[] instruction, CompilationContext context)
        {
            // ATSRIOxa (aaaaaaaa)
            // T: Width of memory write (1, 2, 4, or 8 bytes).
            // S: Register to write to memory.
            // R: Register to use as base address.
            // I: Increment register flag (0 = do not increment R, 1 = increment R by T).
            // O: Offset type, see below.
            // x: Register used as offset when O is 1, Memory type when O is 3, 4 or 5.
            // a: Value used as offset when O is 2, 4 or 5.

            byte operationWidth = instruction[OperationWidthIndex];
            Register sourceRegister = context.GetRegister(instruction[SourceRegisterIndex]);
            Register addressRegister = context.GetRegister(instruction[AddressRegisterIndex]);
            byte incrementAddressRegister = instruction[IncrementAddressRegisterIndex];
            byte offsetType = instruction[AddressingTypeIndex];
            byte registerOrMemoryRegion = instruction[RegisterOrMemoryRegionIndex];
            int immediateSize = instruction.Length <= 8 ? OffsetImmediateSize1 : OffsetImmediateSize9;
            ulong immediate = InstructionHelper.GetImmediate(instruction, OffsetImmediateIndex, immediateSize);

            Pointer destinationMemory;

            switch (offsetType)
            {
                case AddressRegister:
                    // *($R) = $S
                    destinationMemory = MemoryHelper.EmitPointer(addressRegister, context);
                    break;
                case AddressRegisterWithOffsetRegister:
                    // *($R + $x) = $S
                    Register offsetRegister = context.GetRegister(registerOrMemoryRegion);
                    destinationMemory = MemoryHelper.EmitPointer(addressRegister, offsetRegister, context);
                    break;
                case OffsetImmediate:
                    // *(#a) = $S
                    destinationMemory = MemoryHelper.EmitPointer(addressRegister, immediate, context);
                    break;
                case MemoryRegionWithOffsetRegister:
                    // *(?x + $R) = $S
                    destinationMemory = MemoryHelper.EmitPointer((MemoryRegion)registerOrMemoryRegion, addressRegister, context);
                    break;
                case MemoryRegionWithOffsetImmediate:
                    // *(?x + #a) = $S
                    destinationMemory = MemoryHelper.EmitPointer((MemoryRegion)registerOrMemoryRegion, immediate, context);
                    break;
                case MemoryRegionWithOffsetRegisterAndImmediate:
                    // *(?x + #a + $R) = $S
                    destinationMemory = MemoryHelper.EmitPointer((MemoryRegion)registerOrMemoryRegion, addressRegister, immediate, context);
                    break;
                default:
                    throw new TamperCompilationException($"Invalid offset type {offsetType} in Atmosphere cheat");
            }

            InstructionHelper.EmitMov(operationWidth, context, destinationMemory, sourceRegister);

            switch (incrementAddressRegister)
            {
                case 0:
                    // Don't increment the address register by operationWidth.
                    break;
                case 1:
                    // Increment the address register by operationWidth.
                    IOperand increment = new Value<ulong>(operationWidth);
                    context.CurrentOperations.Add(new OpAdd<ulong>(addressRegister, addressRegister, increment));
                    break;
                default:
                    throw new TamperCompilationException($"Invalid increment mode {incrementAddressRegister} in Atmosphere cheat");
            }
        }
    }
}
