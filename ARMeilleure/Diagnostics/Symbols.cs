using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

namespace ARMeilleure.Diagnostics
{
    static class Symbols
    {
        private struct RangedSymbol
        {
            public readonly ulong Start;
            public readonly ulong End;
            public readonly ulong ElementSize;
            public readonly string Name;

            public RangedSymbol(ulong start, ulong end, ulong elemSize, string name)
            {
                Start = start;
                End = end;
                ElementSize = elemSize;
                Name = name;
            }
        }

        private static readonly ConcurrentDictionary<ulong, string> _symbols;
        private static readonly List<RangedSymbol> _rangedSymbols;

        static Symbols()
        {
            _symbols = new ConcurrentDictionary<ulong, string>();
            _rangedSymbols = new List<RangedSymbol>();
        }

        public static string Get(ulong address)
        {
            string result;

            if (_symbols.TryGetValue(address, out result))
            {
                return result;
            }

            lock (_rangedSymbols)
            {
                foreach (RangedSymbol symbol in _rangedSymbols)
                {
                    if (address >= symbol.Start && address <= symbol.End)
                    {
                        ulong diff = address - symbol.Start;
                        ulong rem = diff % symbol.ElementSize;

                        result = symbol.Name + "_" + diff / symbol.ElementSize;

                        if (rem != 0)
                        {
                            result += "+" + rem;
                        }

                        _symbols.TryAdd(address, result);

                        return result;
                    }
                }
            }

            return null;
        }

        [Conditional("M_DEBUG")]
        public static void Add(ulong address, string name)
        {
            _symbols.TryAdd(address, name);
        }

        [Conditional("M_DEBUG")]
        public static void Add(ulong address, ulong size, ulong elemSize, string name)
        {
            lock (_rangedSymbols)
            {
                _rangedSymbols.Add(new RangedSymbol(address, address + size, elemSize, name));
            }
        }
    }
}
