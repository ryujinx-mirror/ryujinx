using ARMeilleure.CodeGen.Unwinding;

namespace ARMeilleure.Translation
{
    struct JitCacheEntry
    {
        public int Offset { get; }
        public int Size   { get; }

        public UnwindInfo UnwindInfo { get; }

        public JitCacheEntry(int offset, int size, UnwindInfo unwindInfo)
        {
            Offset     = offset;
            Size       = size;
            UnwindInfo = unwindInfo;
        }
    }
}