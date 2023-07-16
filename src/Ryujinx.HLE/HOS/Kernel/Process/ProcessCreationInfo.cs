namespace Ryujinx.HLE.HOS.Kernel.Process
{
    readonly struct ProcessCreationInfo
    {
        public string Name { get; }

        public int Version { get; }
        public ulong TitleId { get; }

        public ulong CodeAddress { get; }
        public int CodePagesCount { get; }

        public ProcessCreationFlags Flags { get; }
        public int ResourceLimitHandle { get; }
        public int SystemResourcePagesCount { get; }

        public ProcessCreationInfo(
            string name,
            int version,
            ulong titleId,
            ulong codeAddress,
            int codePagesCount,
            ProcessCreationFlags flags,
            int resourceLimitHandle,
            int systemResourcePagesCount)
        {
            Name = name;
            Version = version;
            TitleId = titleId;
            CodeAddress = codeAddress;
            CodePagesCount = codePagesCount;
            Flags = flags;
            ResourceLimitHandle = resourceLimitHandle;
            SystemResourcePagesCount = systemResourcePagesCount;
        }
    }
}
