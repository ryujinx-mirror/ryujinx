using System.Collections;
using System.Collections.Generic;

namespace ARMeilleure.Common
{
    class BitMap : IEnumerable<int>
    {
        private const int IntSize = 32;
        private const int IntMask = IntSize - 1;

        private List<int> _masks;

        public BitMap(int initialCapacity)
        {
            int count = (initialCapacity + IntMask) / IntSize;

            _masks = new List<int>(count);

            while (count-- > 0)
            {
                _masks.Add(0);
            }
        }

        public bool Set(int bit)
        {
            EnsureCapacity(bit + 1);

            int wordIndex = bit / IntSize;
            int wordBit   = bit & IntMask;

            int wordMask = 1 << wordBit;

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
            int wordBit   = bit & IntMask;

            int wordMask = 1 << wordBit;

            _masks[wordIndex] &= ~wordMask;
        }

        public bool IsSet(int bit)
        {
            EnsureCapacity(bit + 1);

            int wordIndex = bit / IntSize;
            int wordBit   = bit & IntMask;

            return (_masks[wordIndex] & (1 << wordBit)) != 0;
        }

        public bool Set(BitMap map)
        {
            EnsureCapacity(map._masks.Count * IntSize);

            bool modified = false;

            for (int index = 0; index < _masks.Count; index++)
            {
                int newValue = _masks[index] | map._masks[index];

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
                int newValue = _masks[index] & ~map._masks[index];

                if (_masks[index] != newValue)
                {
                    _masks[index] = newValue;

                    modified = true;
                }
            }

            return modified;
        }

        private void EnsureCapacity(int size)
        {
            while (_masks.Count * IntSize < size)
            {
                _masks.Add(0);
            }
        }

        public IEnumerator<int> GetEnumerator()
        {
            for (int index = 0; index < _masks.Count; index++)
            {
                int mask = _masks[index];

                while (mask != 0)
                {
                    int bit = BitUtils.LowestBitSet(mask);

                    mask &= ~(1 << bit);

                    yield return index * IntSize + bit;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}