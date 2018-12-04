namespace Ryujinx.HLE.Loaders.Elf
{
    struct ElfSymbol
    {
        public string Name { get; }

        public ElfSymbolType       Type       { get; }
        public ElfSymbolBinding    Binding    { get; }
        public ElfSymbolVisibility Visibility { get; }

        public bool IsFuncOrObject =>
            Type == ElfSymbolType.SttFunc ||
            Type == ElfSymbolType.SttObject;

        public bool IsGlobalOrWeak =>
            Binding == ElfSymbolBinding.StbGlobal ||
            Binding == ElfSymbolBinding.StbWeak;

        public int  ShIdx { get; }
        public long Value { get; }
        public long Size  { get; }

        public ElfSymbol(
            string name,
            int    info,
            int    other,
            int    shIdx,
            long   value,
            long   size)
        {
            Name       = name;
            Type       = (ElfSymbolType)(info & 0xf);
            Binding    = (ElfSymbolBinding)(info >> 4);
            Visibility = (ElfSymbolVisibility)other;
            ShIdx      = shIdx;
            Value      = value;
            Size       = size;
        }
    }
}