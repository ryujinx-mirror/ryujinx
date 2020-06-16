namespace ARMeilleure.Translation.PTC
{
    struct RelocEntry
    {
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
