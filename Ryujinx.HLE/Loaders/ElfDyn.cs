namespace Ryujinx.HLE.Loaders
{
    struct ElfDyn
    {
        public ElfDynTag Tag { get; private set; }

        public long Value { get; private set; }

        public ElfDyn(ElfDynTag Tag, long Value)
        {
            this.Tag   = Tag;
            this.Value = Value;
        }
    }
}