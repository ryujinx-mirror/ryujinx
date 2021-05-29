namespace ARMeilleure.Translation.PTC
{
    struct RelocEntry
    {
        public const int Stride = 13; // Bytes.

        public int Position;
        public Symbol Symbol;

        public RelocEntry(int position, Symbol symbol)
        {
            Position = position;
            Symbol = symbol;
        }

        public override string ToString()
        {
            return $"({nameof(Position)} = {Position}, {nameof(Symbol)} = {Symbol})";
        }
    }
}