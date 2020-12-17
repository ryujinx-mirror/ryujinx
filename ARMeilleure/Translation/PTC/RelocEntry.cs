namespace ARMeilleure.Translation.PTC
{
    struct RelocEntry
    {
        public const int Stride = 8; // Bytes.

        public int Position;
        public int Index;

        public RelocEntry(int position, int index)
        {
            Position = position;
            Index    = index;
        }

        public override string ToString()
        {
            return $"({nameof(Position)} = {Position}, {nameof(Index)} = {Index})";
        }
    }
}
