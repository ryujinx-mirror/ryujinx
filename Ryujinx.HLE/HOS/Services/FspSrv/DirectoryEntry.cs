namespace Ryujinx.HLE.HOS.Services.FspSrv
{
    public struct DirectoryEntry
    {
        public string Path { get; }
        public long   Size { get; }

        public DirectoryEntryType EntryType { get; set; }

        public DirectoryEntry(string path, DirectoryEntryType directoryEntryType, long size = 0)
        {
            Path = path;
            EntryType = directoryEntryType;
            Size = size;
        }
    }
}
