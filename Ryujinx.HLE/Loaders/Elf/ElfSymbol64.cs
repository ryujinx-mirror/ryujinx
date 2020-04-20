namespace Ryujinx.HLE.Loaders.Elf
{
    struct ElfSymbol64
    {
#pragma warning disable CS0649
        public uint   NameOffset;
        public char   Info;
        public char   Other;
        public ushort SectionIndex;
        public ulong  ValueAddress;
        public ulong  Size;
#pragma warning restore CS0649
    }
}
