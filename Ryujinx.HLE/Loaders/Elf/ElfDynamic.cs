namespace Ryujinx.HLE.Loaders.Elf
{
    struct ElfDynamic
    {
        public ElfDynamicTag Tag { get; }

        public long Value { get; }

        public ElfDynamic(ElfDynamicTag tag, long value)
        {
            Tag   = tag;
            Value = value;
        }
    }
}