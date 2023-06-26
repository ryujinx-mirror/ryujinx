namespace ARMeilleure.CodeGen.RegisterAllocators
{
    readonly struct AllocationResult
    {
        public int IntUsedRegisters { get; }
        public int VecUsedRegisters { get; }
        public int SpillRegionSize { get; }

        public AllocationResult(
            int intUsedRegisters,
            int vecUsedRegisters,
            int spillRegionSize)
        {
            IntUsedRegisters = intUsedRegisters;
            VecUsedRegisters = vecUsedRegisters;
            SpillRegionSize = spillRegionSize;
        }
    }
}
