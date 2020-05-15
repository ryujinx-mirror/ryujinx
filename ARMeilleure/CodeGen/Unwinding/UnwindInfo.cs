namespace ARMeilleure.CodeGen.Unwinding
{
    struct UnwindInfo
    {
        public UnwindPushEntry[] PushEntries { get; }
        public int PrologSize { get; }

        public UnwindInfo(UnwindPushEntry[] pushEntries, int prologSize)
        {
            PushEntries = pushEntries;
            PrologSize = prologSize;
        }
    }
}