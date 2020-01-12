namespace Ryujinx.HLE.Loaders.Elf
{
    struct ElfSymbol32
    {
        public uint   NameOffset;
        public uint   ValueAddress;
        public uint   Size;
        public char   Info;
        public char   Other;
        public ushort SectionIndex;
    }
}
