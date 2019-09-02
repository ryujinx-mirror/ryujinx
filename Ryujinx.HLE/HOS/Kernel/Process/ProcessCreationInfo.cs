namespace Ryujinx.HLE.HOS.Kernel.Process
{
    struct ProcessCreationInfo
    {
        public string Name { get; private set; }

        public int   Category { get; private set; }
        public ulong TitleId  { get; private set; }

        public ulong CodeAddress    { get; private set; }
        public int   CodePagesCount { get; private set; }

        public int MmuFlags                 { get; private set; }
        public int ResourceLimitHandle      { get; private set; }
        public int PersonalMmHeapPagesCount { get; private set; }

        public ProcessCreationInfo(
            string name,
            int    category,
            ulong  titleId,
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