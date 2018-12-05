namespace Ryujinx.HLE.Loaders.Elf
{
    struct ElfSymbol
    {
        public string Name { get; private set; }

        public ElfSymbolType       Type       { get; private set; }
        public ElfSymbolBinding    Binding    { get; private set; }
        public ElfSymbolVisibility Visibility { get; private set; }

        public bool IsFuncOrObject =>
            Type == ElfSymbolType.STT_FUNC ||
            Type == ElfSymbolType.STT_OBJECT;

        public bool IsGlobalOrWeak =>
            Binding == ElfSymbolBinding.STB_GLOBAL ||
            Binding == ElfSymbolBinding.STB_WEAK;

        public int  SHIdx { get; private set; }
        public long Value { get; private set; }
        public long Size  { get; private set; }

        public ElfSymbol(
            string Name,
            int    Info,
            int    Other,
            int    SHIdx,
            long   Value,
            long   Size)
        {
            this.Name       = Name;
            this.Type       = (ElfSymbolType)(Info & 0xf);
            this.Binding    = (ElfSymbolBinding)(Info >> 4);
            this.Visibility = (ElfSymbolVisibility)Other;
            this.SHIdx      = SHIdx;
            this.Value      = Value;
            this.Size       = Size;
        }
    }
}