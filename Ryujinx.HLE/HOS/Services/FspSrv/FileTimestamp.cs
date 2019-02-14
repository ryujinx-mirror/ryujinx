using System;

namespace Ryujinx.HLE.HOS.Services.FspSrv
{
    struct FileTimestamp
    {
        public DateTime CreationDateTime;
        public DateTime ModifiedDateTime;
        public DateTime LastAccessDateTime;
    }
}