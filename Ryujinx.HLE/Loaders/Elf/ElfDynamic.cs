namespace Ryujinx.HLE.Loaders.Elf
{
    struct ElfDynamic
    {
        public ElfDynamicTag Tag { get; private set; }

        public long Value { get; private set; }

        public ElfDynamic(ElfDynamicTag Tag, long Value)
        {
            this.Tag   = Tag;
            this.Value = Value;
        }
    }
}