using ChocolArm64.Memory;

namespace Ryujinx.Core.OsHle
{
    static class MemoryRegions
    {
        public const long MapRegionAddress = 0x80000000;
        public const long MapRegionSize    = 0x40000000;

        public const long HeapRegionAddress = MapRegionAddress + MapRegionSize;
        public const long HeapRegionSize    = 0x40000000;
    }
}