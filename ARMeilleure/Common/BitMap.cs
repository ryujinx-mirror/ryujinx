using System.Collections;
using System.Collections.Generic;
using System.Numerics;

namespace ARMeilleure.Common
{
    class BitMap : IEnumerator<int>, IEnumerable<int>
    {
        private const int IntSize = 64;
        private const int IntMask = IntSize - 1;

        private readonly List<long> _masks;

        private int _enumIndex;
        private long _enumMask;
        private int _enumBit;

        public int Current => _enumIndex * IntSize + _enumBit;
        object IEnumerator.Current => Current;

        public BitMap()
        {
            _masks = new List<long>(0);
        }

        public BitMap(int initialCapacity)
        {
            int count = (initialCapacity + IntMask) / IntSize;

            _masks = new List<long>(count);

            while (count-- > 0)
            {
                _masks.Add(0);
            }
        }

        public BitMap Reset(int initialCapacity)
        {
            int count = (initialCapacity + IntMask) / IntSize;

            if (count > _masks.Capacity)
            {
                _masks.Capacity = count;
            }

            _masks.Clear();

            while (count-- > 0)
            {
                _masks.Add(0);
            }

            return this;
        }

        public bool Set(int bit)
        {
            EnsureCapacity(bit + 1);

            int wordIndex = bit / IntSize;
            int wordBit = bit & IntMask;

            long wordMask = 1L << wordBit;

            if ((_masks[wordIndex] & wordMask) != 0)
            {
                return false;
            }

            _masks[wordIndex] |= wordMask;

            return true;
        }

        public void Clear(int bit)
        {
            EnsureCapacity(bit + 1);

            int wordIndex = bit / IntSize;
            int wordBit = bit & IntMask;

            long wordMask = 1L << wordBit;

            _masks[wordIndex] &= ~wordMask;
        }

        public bool IsSet(int bit)
        {
            EnsureCapacity(bit + 1);

            int wordIndex = bit / IntSize;
            int wordBit = bit & IntMask;

            return (_masks[wordIndex] & (1L << wordBit)) != 0;
        }

        public int FindFirstUnset()
        {
            for (int index = 0; index < _masks.Count; index++)
            {
                long mask = _masks[index];

                if (mask != -1L)
                {
                    return BitOperations.TrailingZeroCount(~mask) + index * IntSize;
                }
            }

            return _masks.Count * IntSize;
        }

        public bool Set(BitMap map)
        {
            EnsureCapacity(map._masks.Count * IntSize);

            bool modified = false;

            for (int index = 0; index < _masks.Count; index++)
            {
                long newValue = _masks[index] | map._masks[index];

                if (_masks[index] != newValue)
                {
                    _masks[index] = newValue;

                    modified = true;
                }
            }

            return modified;
        }

        public bool Clear(BitMap map)
        {
            EnsureCapacity(map._masks.Count * IntSize);

            bool modified = false;

            for (int index = 0; index < _masks.Count; index++)
            {
                long newValue = _masks[index] & ~map._masks[index];

                if (_masks[index] != newValue)
                {
                    _masks[index] = newValue;

                    modified = true;
                }
            }

            return modified;
        }

        #region IEnumerable<long> Methods

        // Note: The bit enumerator is embedded in this class to avoid creating garbage when enumerating.

        private void EnsureCapacity(int size)
        {
            while (_masks.Count * IntSize < size)
            {
                _masks.Add(0);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<int> GetEnumerator()
        {
            Reset();
            return this;
        }

        public bool MoveNext()
        {
            if (_enumMask != 0)
            {
                _enumMask &= ~(1L << _enumBit);
            }
            while (_enumMask == 0)
            {
                if (++_enumIndex >= _masks.Count)
                {
                    return false;
                }
                _enumMask = _masks[_enumIndex];
            }
            _enumBit = BitOperations.TrailingZeroCount(_enumMask);
            return true;
        }

        public void Reset()
        {
            _enumIndex = -1;
            _enumMask = 0;
            _enumBit = 0;
        }

        public void Dispose() { }

        #endregion
    }
}