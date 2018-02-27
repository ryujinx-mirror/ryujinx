using ChocolArm64.Memory;

namespace Ryujinx.Core.OsHle
{
    static class MemoryRegions
    {
        public const long AddrSpaceStart = 0x08000000;

        public const long MapRegionAddress = 0x10000000;
        public const long MapRegionSize    = 0x40000000;

        public const long MainStackSize = 0x100000;

        public const long MainStackAddress = AMemoryMgr.AddrSize - MainStackSize;

        public const long TlsPagesSize = 0x4000;

        public const long TlsPagesAddress = MainStackAddress - TlsPagesSize;

        public const long HeapRegionAddress = MapRegionAddress + MapRegionSize;

        public const long TotalMemoryUsed = HeapRegionAddress + TlsPagesSize + MainStackSize;

        public const long TotalMemoryAvailable = AMemoryMgr.RamSize - AddrSpaceStart;

        public const long AddrSpaceSize = AMemoryMgr.AddrSize - AddrSpaceStart;
    }
}