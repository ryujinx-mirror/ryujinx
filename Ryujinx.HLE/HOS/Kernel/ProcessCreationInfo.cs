namespace Ryujinx.HLE.HOS.Kernel
{
    struct ProcessCreationInfo
    {
        public string Name { get; private set; }

        public int  Category { get; private set; }
        public long TitleId  { get; private set; }

        public ulong CodeAddress    { get; private set; }
        public int   CodePagesCount { get; private set; }

        public int MmuFlags                 { get; private set; }
        public int ResourceLimitHandle      { get; private set; }
        public int PersonalMmHeapPagesCount { get; private set; }

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