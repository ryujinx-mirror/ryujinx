using System.Collections.Generic;

namespace Ryujinx.Graphics
{
    class ValueRangeSet<T>
    {
        private List<ValueRange<T>> Ranges;

        public ValueRangeSet()
        {
            Ranges = new List<ValueRange<T>>();
        }

        public void Add(ValueRange<T> Range)
        {
            if (Range.End <= Range.Start)
            {
                //Empty or invalid range, do nothing.
                return;
            }

            int First = BinarySearchFirstIntersection(Range);

            if (First == -1)
            {
                //No intersections case.
                //Find first greater than range (after the current one).
                //If found, add before, otherwise add to the end of the list.
                int GtIndex = BinarySearchGt(Range);

                if (GtIndex != -1)
                {
                    Ranges.Insert(GtIndex, Range);
                }
                else
                {
                    Ranges.Add(Range);
                }

                return;
            }

            (int Start, int End) = GetAllIntersectionRanges(Range, First);

            ValueRange<T> Prev = Ranges[Start];
            ValueRange<T> Next = Ranges[End];

            Ranges.RemoveRange(Start, (End - Start) + 1);

            InsertNextNeighbour(Start, Range, Next);

            int NewIndex = Start;

            Ranges.Insert(Start, Range);

            InsertPrevNeighbour(Start, Range, Prev);

            //Try merging neighbours if the value is equal.
            if (NewIndex > 0)
            {
                Prev = Ranges[NewIndex - 1];

                if (Prev.End == Range.Start && CompareValues(Prev, Range))
                {
                    Ranges.RemoveAt(--NewIndex);

                    Ranges[NewIndex] = new ValueRange<T>(Prev.Start, Range.End, Range.Value);
                }
            }

            if (NewIndex < Ranges.Count - 1)
            {
                Next = Ranges[NewIndex + 1];

                if (Next.Start == Range.End && CompareValues(Next, Range))
                {
                    Ranges.RemoveAt(NewIndex + 1);

                    Ranges[NewIndex] = new ValueRange<T>(Ranges[NewIndex].Start, Next.End, Range.Value);
                }
            }
        }

        private bool CompareValues(ValueRange<T> LHS, ValueRange<T> RHS)
        {
            return LHS.Value?.Equals(RHS.Value) ?? RHS.Value == null;
        }

        public void Remove(ValueRange<T> Range)
        {
            int First = BinarySearchFirstIntersection(Range);

            if (First == -1)
            {
                //Nothing to remove.
                return;
            }

            (int Start, int End) = GetAllIntersectionRanges(Range, First);

            ValueRange<T> Prev = Ranges[Start];
            ValueRange<T> Next = Ranges[End];

            Ranges.RemoveRange(Start, (End - Start) + 1);

            InsertNextNeighbour(Start, Range, Next);
            InsertPrevNeighbour(Start, Range, Prev);
        }

        private void InsertNextNeighbour(int Index, ValueRange<T> Range, ValueRange<T> Next)
        {
            //Split last intersection (ordered by Start) if necessary.
            if (Range.End < Next.End)
            {
                InsertNewRange(Index, Range.End, Next.End, Next.Value);
            }
        }

        private void InsertPrevNeighbour(int Index, ValueRange<T> Range, ValueRange<T> Prev)
        {
            //Split first intersection (ordered by Start) if necessary.
            if (Range.Start > Prev.Start)
            {
                InsertNewRange(Index, Prev.Start, Range.Start, Prev.Value);
            }
        }

        private void InsertNewRange(int Index, long Start, long End, T Value)
        {
            Ranges.Insert(Index, new ValueRange<T>(Start, End, Value));
        }

        public ValueRange<T>[] GetAllIntersections(ValueRange<T> Range)
        {
            int First = BinarySearchFirstIntersection(Range);

            if (First == -1)
            {
                return new ValueRange<T>[0];
            }

            (int Start, int End) = GetAllIntersectionRanges(Range, First);

            return Ranges.GetRange(Start, (End - Start) + 1).ToArray();
        }

        private (int Start, int End) GetAllIntersectionRanges(ValueRange<T> Range, int BaseIndex)
        {
            int Start = BaseIndex;
            int End   = BaseIndex;

            while (Start > 0 && Intersects(Range, Ranges[Start - 1]))
            {
                Start--;
            }

            while (End < Ranges.Count - 1 && Intersects(Range, Ranges[End + 1]))
            {
                End++;
            }

            return (Start, End);
        }

        private int BinarySearchFirstIntersection(ValueRange<T> Range)
        {
            int Left  = 0;
            int Right = Ranges.Count - 1;

            while (Left <= Right)
            {
                int Size = Right - Left;

                int Middle = Left + (Size >> 1);

                ValueRange<T> Current = Ranges[Middle];

                if (Intersects(Range, Current))
                {
                    return Middle;
                }

                if (Range.Start < Current.Start)
                {
                    Right = Middle - 1;
                }
                else
                {
                    Left = Middle + 1;
                }
            }

            return -1;
        }

        private int BinarySearchGt(ValueRange<T> Range)
        {
            int GtIndex = -1;

            int Left  = 0;
            int Right = Ranges.Count - 1;

            while (Left <= Right)
            {
                int Size = Right - Left;

                int Middle = Left + (Size >> 1);

                ValueRange<T> Current = Ranges[Middle];

                if (Range.Start < Current.Start)
                {
                    Right = Middle - 1;

                    if (GtIndex == -1 || Current.Start < Ranges[GtIndex].Start)
                    {
                        GtIndex = Middle;
                    }
                }
                else
                {
                    Left = Middle + 1;
                }
            }

            return GtIndex;
        }

        private bool Intersects(ValueRange<T> LHS, ValueRange<T> RHS)
        {
            return LHS.Start < RHS.End && RHS.Start < LHS.End;
        }
    }
}