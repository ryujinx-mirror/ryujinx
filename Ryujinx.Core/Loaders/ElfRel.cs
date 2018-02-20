namespace Ryujinx.Core.Loaders
{
    struct ElfRel
    {
        public long Offset { get; private set; }
        public long Addend { get; private set; }

        public ElfSym     Symbol { get; private set; }
        public ElfRelType Type   { get; private set; }

        public ElfRel(long Offset, long Addend, ElfSym Symbol, ElfRelType Type)
        {
            this.Offset = Offset;
            this.Addend = Addend;
            this.Symbol = Symbol;
            this.Type   = Type;
        }
    }
}