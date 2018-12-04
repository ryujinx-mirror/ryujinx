namespace Ryujinx.HLE.HOS.Kernel
{
    struct ProcessCreationInfo
    {
        public string Name { get; }

        public int  Category { get; }
        public long TitleId  { get; }

        public ulong CodeAddress    { get; }
        public int   CodePagesCount { get; }

        public int MmuFlags                 { get; }
        public int ResourceLimitHandle      { get; }
        public int PersonalMmHeapPagesCount { get; }

        public ProcessCreationInfo(
            string name,
            int    category,
            long   titleId,
            ulong  codeAddress,
            int    codePagesCount,
            int    mmuFlags,
            int    resourceLimitHandle,
            int    personalMmHeapPagesCount)
        {
            Name                     = name;
            Category                 = category;
            TitleId                  = titleId;
            CodeAddress              = codeAddress;
            CodePagesCount           = codePagesCount;
            MmuFlags                 = mmuFlags;
            ResourceLimitHandle      = resourceLimitHandle;
            PersonalMmHeapPagesCount = personalMmHeapPagesCount;
        }
    }
}