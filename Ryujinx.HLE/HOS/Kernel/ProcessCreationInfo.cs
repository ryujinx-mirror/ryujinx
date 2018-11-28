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
            string Name,
            int    Category,
            long   TitleId,
            ulong  CodeAddress,
            int    CodePagesCount,
            int    MmuFlags,
            int    ResourceLimitHandle,
            int    PersonalMmHeapPagesCount)
        {
            this.Name                     = Name;
            this.Category                 = Category;
            this.TitleId                  = TitleId;
            this.CodeAddress              = CodeAddress;
            this.CodePagesCount           = CodePagesCount;
            this.MmuFlags                 = MmuFlags;
            this.ResourceLimitHandle      = ResourceLimitHandle;
            this.PersonalMmHeapPagesCount = PersonalMmHeapPagesCount;
        }
    }
}