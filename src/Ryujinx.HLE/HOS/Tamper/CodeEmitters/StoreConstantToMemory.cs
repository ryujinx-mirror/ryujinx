using Ryujinx.HLE.Exceptions;
using Ryujinx.HLE.HOS.Tamper.Operations;

namespace Ryujinx.HLE.HOS.Tamper.CodeEmitters
{
    /// <summary>
    /// Code type 6 allows writing a fixed value to a memory address specified by a register.
    /// </summary>
    class StoreConstantToMemory
    {
        private const int OperationWidthIndex = 1;
        private const int AddressRegisterIndex = 3;
        private const int IncrementAddressRegisterIndex = 4;
        private const int UseOffsetRegisterIndex = 5;
        private const int OffsetRegisterIndex = 6;
        private const int ValueImmediateIndex = 8;

        private const int ValueImmediateSize = 16;

        public static void Emit(byte[] instruction, CompilationContext context)
        {
            // 6T0RIor0 VVVVVVVV VVVVVVVV
            // T: Width of memory write(1, 2, 4, or 8 bytes).
            // R: Register used as base memory address.
            // I: Increment register flag(0 = do not increment R, 1 = increment R by T).
            // o: Offset register enable flag(0 = do not add r to address, 1 = add r to address).
            // r: Register used as offset when o is 1.
            // V: Value to write to memory.

            byte operationWidth = instruction[OperationWidthIndex];
            Register sourceRegister = context.GetRegister(instruction[AddressRegisterIndex]);
            byte incrementAddressRegister = instruction[IncrementAddressRegisterIndex];
            byte useOffsetRegister = instruction[UseOffsetRegisterIndex];
            ulong immediate = InstructionHelper.GetImmediate(instruction, ValueImmediateIndex, ValueImmediateSize);
            Value<ulong> storeValue = new(immediate);

            Pointer destinationMemory;

            switch (useOffsetRegister)
            {
                case 0:
                    // Don't offset the address register by another register.
                    destinationMemory = MemoryHelper.EmitPointer(sourceRegister, context);
                    break;
                case 1:
                    // Replace the source address by the sum of the base and offset registers.
                    Register offsetRegister = context.GetRegister(instruction[OffsetRegisterIndex]);
                    destinationMemory = MemoryHelper.EmitPointer(sourceRegister, offsetRegister, context);
                    break;
                default:
                    throw new TamperCompilationException($"Invalid offset mode {useOffsetRegister} in Atmosphere cheat");
            }

            InstructionHelper.EmitMov(operationWidth, context, destinationMemory, storeValue);

            switch (incrementAddressRegister)
            {
                case 0:
                    // Don't increment the address register by operationWidth.
                    break;
                case 1:
                    // Increment the address register by operationWidth.
                    IOperand increment = new Value<ulong>(operationWidth);
                    context.CurrentOperations.Add(new OpAdd<ulong>(sourceRegister, sourceRegister, increment));
                    break;
                default:
                    throw new TamperCompilationException($"Invalid increment mode {incrementAddressRegister} in Atmosphere cheat");
            }
        }
    }
}
