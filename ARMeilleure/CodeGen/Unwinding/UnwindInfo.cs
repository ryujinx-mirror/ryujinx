namespace ARMeilleure.CodeGen.Unwinding
{
    struct UnwindInfo
    {
        public UnwindPushEntry[] PushEntries { get; }

        public int PrologueSize { get; }

        public int FixedAllocSize { get; }

        public UnwindInfo(UnwindPushEntry[] pushEntries, int prologueSize, int fixedAllocSize)
        {
            PushEntries    = pushEntries;
            PrologueSize   = prologueSize;
            FixedAllocSize = fixedAllocSize;
        }
    }
}