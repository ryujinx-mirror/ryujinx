namespace Ryujinx.Memory
{
    static class MemoryConstants
    {
        public const int PageBits = 12;
        public const int PageSize = 1 << PageBits;
        public const int PageMask = PageSize - 1;
    }
}
