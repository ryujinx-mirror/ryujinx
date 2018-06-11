namespace Ryujinx.HLE.Loaders
{
    struct ElfSym
    {
        public string Name { get; private set; }

        public ElfSymType       Type       { get; private set; }
        public ElfSymBinding    Binding    { get; private set; }
        public ElfSymVisibility Visibility { get; private set; }

        public bool IsFuncOrObject =>
            Type == ElfSymType.STT_FUNC ||
            Type == ElfSymType.STT_OBJECT;

        public bool IsGlobalOrWeak =>
            Binding == ElfSymBinding.STB_GLOBAL ||
            Binding == ElfSymBinding.STB_WEAK;

        public int  SHIdx { get; private set; }
        public long Value { get; private set; }
        public long Size  { get; private set; }

        public ElfSym(
            string Name,
            int    Info,
            int    Other,
            int    SHIdx,
            long   Value,
            long   Size)
        {
            this.Name       = Name;
            this.Type       = (ElfSymType)(Info & 0xf);
            this.Binding    = (ElfSymBinding)(Info >> 4);
            this.Visibility = (ElfSymVisibility)Other;
            this.SHIdx      = SHIdx;
            this.Value      = Value;
            this.Size       = Size;
        }
    }
}