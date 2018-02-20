using ChocolArm64.Memory;

namespace Ryujinx.Core.OsHle
{
    struct MemoryInfo
    {
        public long BaseAddress;
        public long Size;
        public int  MemType;
        public int  MemAttr;
        public int  MemPerm;
        public int  IpcRefCount;
        public int  DeviceRefCount;
        public int  Padding; //SBZ

        public MemoryInfo(AMemoryMapInfo MapInfo)
        {
            BaseAddress    = MapInfo.Position;
            Size           = MapInfo.Size;
            MemType        = MapInfo.Type;
            MemAttr        = MapInfo.Attr;
            MemPerm        = (int)MapInfo.Perm;
            IpcRefCount    = 0;
            DeviceRefCount = 0;
            Padding        = 0;
        }
    }
}