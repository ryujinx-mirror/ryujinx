namespace ARMeilleure.CodeGen.Unwinding
{
    struct UnwindPushEntry
    {
        public const int Stride = 16; // Bytes.

        public UnwindPseudoOp PseudoOp { get; }
        public int PrologOffset { get; }
        public int RegIndex { get; }
        public int StackOffsetOrAllocSize { get; }

        public UnwindPushEntry(UnwindPseudoOp pseudoOp, int prologOffset, int regIndex = -1, int stackOffsetOrAllocSize = -1)
        {
            PseudoOp = pseudoOp;
            PrologOffset = prologOffset;
            RegIndex = regIndex;
            StackOffsetOrAllocSize = stackOffsetOrAllocSize;
        }
    }
}