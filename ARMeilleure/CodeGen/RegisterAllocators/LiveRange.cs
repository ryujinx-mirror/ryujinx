using System;

namespace ARMeilleure.CodeGen.RegisterAllocators
{
    struct LiveRange : IComparable<LiveRange>
    {
        public int Start { get; }
        public int End   { get; }

        public LiveRange(int start, int end)
        {
            Start = start;
            End   = end;
        }

        public int CompareTo(LiveRange other)
        {
            if (Start < other.End && other.Start < End)
            {
                return 0;
            }

            return Start.CompareTo(other.Start);
        }

        public override string ToString()
        {
            return $"[{Start}, {End}[";
        }
    }
}