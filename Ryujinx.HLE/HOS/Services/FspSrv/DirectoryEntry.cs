namespace Ryujinx.HLE.HOS.Services.FspSrv
{
    public struct DirectoryEntry
    {
        public string Path { get; private set; }
        public long   Size { get; private set; }

        public DirectoryEntryType EntryType { get; set; }

        public DirectoryEntry(string path, DirectoryEntryType directoryEntryType, long size = 0)
        {
            Path = path;
            EntryType = directoryEntryType;
            Size = size;
        }
    }
}
