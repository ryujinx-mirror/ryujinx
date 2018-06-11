using ChocolArm64.Memory;

namespace Ryujinx.HLE.OsHle
{
    static class MemoryRegions
    {
        public const long AddrSpaceStart = 0x08000000;

        public const long MapRegionAddress = 0x10000000;
        public const long MapRegionSize    = 0x20000000;

        public const long HeapRegionAddress = MapRegionAddress + MapRegionSize;
        public const long HeapRegionSize    = TlsPagesAddress - HeapRegionAddress;

        public const long MainStackSize = 0x100000;

        public const long MainStackAddress = AMemoryMgr.AddrSize - MainStackSize;

        public const long TlsPagesSize = 0x20000;

        public const long TlsPagesAddress = MainStackAddress - TlsPagesSize;

        public const long TotalMemoryUsed = HeapRegionAddress + TlsPagesSize + MainStackSize;

        public const long TotalMemoryAvailable = AMemoryMgr.RamSize - AddrSpaceStart;

        public const long AddrSpaceSize = AMemoryMgr.AddrSize - AddrSpaceStart;
    }
}