using ARMeilleure.Common;
using ARMeilleure.IntermediateRepresentation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ARMeilleure.CodeGen.RegisterAllocators
{
    class LiveInterval : IComparable<LiveInterval>
    {
        public const int NotFound = -1;

        private LiveInterval _parent;

        private SortedIntegerList _usePositions;

        public int UsesCount => _usePositions.Count;

        private List<LiveRange> _ranges;

        private SortedList<int, LiveInterval> _childs;

        public bool IsSplit => _childs.Count != 0;

        public Operand Local { get; }

        public Register Register { get; set; }

        public int SpillOffset { get; private set; }

        public bool IsSpilled => SpillOffset != -1;
        public bool IsFixed { get; }

        public bool IsEmpty => _ranges.Count == 0;

        public LiveInterval(Operand local = null, LiveInterval parent = null)
        {
            Local   = local;
            _parent = parent ?? this;

            _usePositions = new SortedIntegerList();

            _ranges = new List<LiveRange>();

            _childs = new SortedList<int, LiveInterval>();

            SpillOffset = -1;
        }

        public LiveInterval(Register register) : this()
        {
            IsFixed  = true;
            Register = register;
        }

        public void SetStart(int position)
        {
            if (_ranges.Count != 0)
            {
                Debug.Assert(position != _ranges[0].End);

                _ranges[0] = new LiveRange(position, _ranges[0].End);
            }
            else
            {
                _ranges.Add(new LiveRange(position, position + 1));
            }
        }

        public int GetStart()
        {
            if (_ranges.Count == 0)
            {
                throw new InvalidOperationException("Empty interval.");
            }

            return _ranges[0].Start;
        }

        public void SetEnd(int position)
        {
            if (_ranges.Count != 0)
            {
                int lastIdx = _ranges.Count - 1;

                Debug.Assert(position != _ranges[lastIdx].Start);

                _ranges[lastIdx] = new LiveRange(_ranges[lastIdx].Start, position);
            }
            else
            {
                _ranges.Add(new LiveRange(position, position + 1));
            }
        }

        public int GetEnd()
        {
            if (_ranges.Count == 0)
            {
                throw new InvalidOperationException("Empty interval.");
            }

            return _ranges[_ranges.Count - 1].End;
        }

        public void AddRange(int start, int end)
        {
            if (start >= end)
            {
                throw new ArgumentException("Invalid range start position " + start + ", " + end);
            }

            int index = _ranges.BinarySearch(new LiveRange(start, end));

            if (index >= 0)
            {
                // New range insersects with an existing range, we need to remove
                // all the intersecting ranges before adding the new one.
                // We also extend the new range as needed, based on the values of
                // the existing ranges being removed.
                int lIndex = index;
                int rIndex = index;

                while (lIndex > 0 && _ranges[lIndex - 1].End >= start)
                {
                    lIndex--;
                }

                while (rIndex + 1 < _ranges.Count && _ranges[rIndex + 1].Start <= end)
                {
                    rIndex++;
                }

                if (start > _ranges[lIndex].Start)
                {
                    start = _ranges[lIndex].Start;
                }

                if (end < _ranges[rIndex].End)
                {
                    end = _ranges[rIndex].End;
                }

                _ranges.RemoveRange(lIndex, (rIndex - lIndex) + 1);

                InsertRange(lIndex, start, end);
            }
            else
            {
                InsertRange(~index, start, end);
            }
        }

        private void InsertRange(int index, int start, int end)
        {
            // Here we insert a new range on the ranges list.
            // If possible, we extend an existing range rather than inserting a new one.
            // We can extend an existing range if any of the following conditions are true:
            // - The new range starts right after the end of the previous range on the list.
            // - The new range ends right before the start of the next range on the list.
            // If both cases are true, we can extend either one. We prefer to extend the
            // previous range, and then remove the next one, but theres no specific reason
            // for that, extending either one will do.
            int? extIndex = null;

            if (index > 0 && _ranges[index - 1].End == start)
            {
                start = _ranges[index - 1].Start;

                extIndex = index - 1;
            }

            if (index < _ranges.Count && _ranges[index].Start == end)
            {
                end = _ranges[index].End;

                if (extIndex.HasValue)
                {
                    _ranges.RemoveAt(index);
                }
                else
                {
                    extIndex = index;
                }
            }

            if (extIndex.HasValue)
            {
                _ranges[extIndex.Value] = new LiveRange(start, end);
            }
            else
            {
                _ranges.Insert(index, new LiveRange(start, end));
            }
        }

        public void AddUsePosition(int position)
        {
            // Inserts are in descending order, but ascending is faster for SortedIntegerList<>.
            // We flip the ordering, then iterate backwards when using the final list.
            _usePositions.Add(-position);
        }

        public bool Overlaps(int position)
        {
            return _ranges.BinarySearch(new LiveRange(position, position + 1)) >= 0;
        }

        public bool Overlaps(LiveInterval other)
        {
            foreach (LiveRange range in other._ranges)
            {
                if (_ranges.BinarySearch(range) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        public int GetOverlapPosition(LiveInterval other)
        {
            foreach (LiveRange range in other._ranges)
            {
                int overlapIndex = _ranges.BinarySearch(range);

                if (overlapIndex >= 0)
                {
                    // It's possible that we have multiple overlaps within a single interval,
                    // in this case, we pick the one with the lowest start position, since
                    // we return the first overlap position.
                    while (overlapIndex > 0 && _ranges[overlapIndex - 1].End > range.Start)
                    {
                        overlapIndex--;
                    }

                    LiveRange overlappingRange = _ranges[overlapIndex];

                    return overlappingRange.Start;
                }
            }

            return NotFound;
        }

        public IEnumerable<LiveInterval> SplitChilds()
        {
            return _childs.Values;
        }

        public IList<int> UsePositions()
        {
            return _usePositions.GetList();
        }

        public int FirstUse()
        {
            if (_usePositions.Count == 0)
            {
                return NotFound;
            }

            return -_usePositions.Last();
        }

        public int NextUseAfter(int position)
        {
            int index = _usePositions.FindLessEqualIndex(-position);
            return (index >= 0) ? -_usePositions[index] : NotFound;
        }

        public void RemoveAfter(int position)
        {
            int index = _usePositions.FindLessEqualIndex(-position);
            _usePositions.RemoveRange(0, index + 1);
        }

        public LiveInterval Split(int position)
        {
            LiveInterval right = new LiveInterval(Local, _parent);

            int splitIndex = 0;

            for (; splitIndex < _ranges.Count; splitIndex++)
            {
                LiveRange range = _ranges[splitIndex];

                if (position > range.Start && position < range.End)
                {
                    right._ranges.Add(new LiveRange(position, range.End));

                    range = new LiveRange(range.Start, position);

                    _ranges[splitIndex++] = range;

                    break;
                }

                if (range.Start >= position)
                {
                    break;
                }
            }

            if (splitIndex < _ranges.Count)
            {
                int count = _ranges.Count - splitIndex;

                right._ranges.AddRange(_ranges.GetRange(splitIndex, count));

                _ranges.RemoveRange(splitIndex, count);
            }

            int addAfter = _usePositions.FindLessEqualIndex(-position);
            for (int index = addAfter; index >= 0; index--)
            {
                int usePosition = _usePositions[index];
                right._usePositions.Add(usePosition);
            }

            RemoveAfter(position);

            Debug.Assert(_ranges.Count != 0, "Left interval is empty after split.");

            Debug.Assert(right._ranges.Count != 0, "Right interval is empty after split.");

            AddSplitChild(right);

            return right;
        }

        private void AddSplitChild(LiveInterval child)
        {
            Debug.Assert(!child.IsEmpty, "Trying to insert a empty interval.");

            _parent._childs.Add(child.GetStart(), child);
        }

        public LiveInterval GetSplitChild(int position)
        {
            if (Overlaps(position))
            {
                return this;
            }

            foreach (LiveInterval splitChild in _childs.Values)
            {
                if (splitChild.Overlaps(position))
                {
                    return splitChild;
                }
            }

            return null;
        }

        public bool TrySpillWithSiblingOffset()
        {
            foreach (LiveInterval splitChild in _parent._childs.Values)
            {
                if (splitChild.IsSpilled)
                {
                    Spill(splitChild.SpillOffset);

                    return true;
                }
            }

            return false;
        }

        public void Spill(int offset)
        {
            SpillOffset = offset;
        }

        public int CompareTo(LiveInterval other)
        {
            if (_ranges.Count == 0 || other._ranges.Count == 0)
            {
                return _ranges.Count.CompareTo(other._ranges.Count);
            }

            return _ranges[0].Start.CompareTo(other._ranges[0].Start);
        }

        public override string ToString()
        {
            return string.Join("; ", _ranges);
        }
    }
}