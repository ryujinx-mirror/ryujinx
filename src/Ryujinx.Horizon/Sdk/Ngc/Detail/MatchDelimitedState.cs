namespace Ryujinx.Horizon.Sdk.Ngc.Detail
{
    struct MatchDelimitedState
    {
        public bool Matched;
        public readonly bool PrevCharIsWordSeparator;
        public readonly bool NextCharIsWordSeparator;
        public readonly Sbv NoSeparatorMap;
        public readonly AhoCorasick DelimitedWordsTrie;

        public MatchDelimitedState(
            bool prevCharIsWordSeparator,
            bool nextCharIsWordSeparator,
            Sbv noSeparatorMap,
            AhoCorasick delimitedWordsTrie)
        {
            Matched = false;
            PrevCharIsWordSeparator = prevCharIsWordSeparator;
            NextCharIsWordSeparator = nextCharIsWordSeparator;
            NoSeparatorMap = noSeparatorMap;
            DelimitedWordsTrie = delimitedWordsTrie;
        }
    }
}
