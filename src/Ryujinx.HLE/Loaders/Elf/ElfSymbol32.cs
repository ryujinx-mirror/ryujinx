namespace Ryujinx.HLE.Loaders.Elf
{
    struct ElfSymbol32
    {
#pragma warning disable CS0649 // Field is never assigned to
        public uint NameOffset;
        public uint ValueAddress;
        public uint Size;
        public byte Info;
        public byte Other;
        public ushort SectionIndex;
#pragma warning restore CS0649
    }
}
