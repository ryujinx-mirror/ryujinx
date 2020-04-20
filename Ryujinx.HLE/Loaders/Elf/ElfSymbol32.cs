namespace Ryujinx.HLE.Loaders.Elf
{
    struct ElfSymbol32
    {
#pragma warning disable CS0649
        public uint   NameOffset;
        public uint   ValueAddress;
        public uint   Size;
        public char   Info;
        public char   Other;
        public ushort SectionIndex;
#pragma warning restore CS0649
    }
}
