namespace ARMeilleure.CodeGen.Linking
{
    /// <summary>
    /// Represents a relocation.
    /// </summary>
    readonly struct RelocEntry
    {
        public const int Stride = 13; // Bytes.

        /// <summary>
        /// Gets the position of the relocation.
        /// </summary>
        public int Position { get; }

        /// <summary>
        /// Gets the <see cref="Symbol"/> of the relocation.
        /// </summary>
        public Symbol Symbol { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelocEntry"/> struct with the specified position and
        /// <see cref="Symbol"/>.
        /// </summary>
        /// <param name="position">Position of relocation</param>
        /// <param name="symbol">Symbol of relocation</param>
        public RelocEntry(int position, Symbol symbol)
        {
            Position = position;
            Symbol = symbol;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"({nameof(Position)} = {Position}, {nameof(Symbol)} = {Symbol})";
        }
    }
}
