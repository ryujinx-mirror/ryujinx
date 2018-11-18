using System;
using System.Collections.Generic;
using System.Text;

namespace Ryujinx.HLE.HOS.Services.FspSrv
{
    public struct DirectoryEntry
    {
        public string Path { get; private set; }
        public long   Size { get; private set; }

        public DirectoryEntryType EntryType { get; set; }

        public DirectoryEntry(string Path, DirectoryEntryType DirectoryEntryType, long Size = 0)
        {
            this.Path = Path;
            EntryType = DirectoryEntryType;
            this.Size = Size;
        }
    }
}
