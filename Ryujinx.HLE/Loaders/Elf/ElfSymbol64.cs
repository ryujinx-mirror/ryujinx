namespace Ryujinx.HLE.Loaders.Elf
{
    struct ElfSymbol64
    {
        public uint   NameOffset;
        public char   Info;
        public char   Other;
        public ushort SectionIndex;
        public ulong  ValueAddress;
        public ulong  Size;
    }
}
