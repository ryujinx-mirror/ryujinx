using Ryujinx.HLE.HOS.Tamper.Conditions;

namespace Ryujinx.HLE.HOS.Tamper.CodeEmitters
{
    /// <summary>
    /// Code type 1 performs a comparison of the contents of memory to a static value.
    /// If the condition is not met, all instructions until the appropriate conditional block terminator
    /// are skipped.
    /// </summary>
    class MemoryConditional
    {
        private const int OperationWidthIndex = 1;
        private const int MemoryRegionIndex = 2;
        private const int ComparisonTypeIndex = 3;
        private const int OffsetImmediateIndex = 6;
        private const int ValueImmediateIndex = 16;

        private const int OffsetImmediateSize = 10;
        private const int ValueImmediateSize4 = 8;
        private const int ValueImmediateSize8 = 16;

        public static ICondition Emit(byte[] instruction, CompilationContext context)
        {
            // 1TMC00AA AAAAAAAA VVVVVVVV (VVVVVVVV)
            // T: Width of memory write (1, 2, 4, or 8 bytes).
            // M: Memory region to write to (0 = Main NSO, 1 = Heap).
            // C: Condition to use, see below.
            // A: Immediate offset to use from memory region base.
            // V: Value to compare to.

            byte operationWidth = instruction[OperationWidthIndex];
            MemoryRegion memoryRegion = (MemoryRegion)instruction[MemoryRegionIndex];
            Comparison comparison = (Comparison)instruction[ComparisonTypeIndex];

            ulong address = InstructionHelper.GetImmediate(instruction, OffsetImmediateIndex, OffsetImmediateSize);
            Pointer sourceMemory = MemoryHelper.EmitPointer(memoryRegion, address, context);

            int valueSize = operationWidth <= 4 ? ValueImmediateSize4 : ValueImmediateSize8;
            ulong value = InstructionHelper.GetImmediate(instruction, ValueImmediateIndex, valueSize);
            Value<ulong> compareToValue = new(value);

            return InstructionHelper.CreateCondition(comparison, operationWidth, sourceMemory, compareToValue);
        }
    }
}
