using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;

namespace ARMeilleure.Common
{
    unsafe class BitMap : IEnumerable<int>, IDisposable
    {
        private const int IntSize = 64;
        private const int IntMask = IntSize - 1;

        private int _count;
        private long* _masks;
        private readonly Allocator _allocator;

        public BitMap(Allocator allocator)
        {
            _allocator = allocator;
        }

        public BitMap(Allocator allocator, int capacity) : this(allocator)
        {
            EnsureCapacity(capacity);
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
            for (int index = 0; index < _count; index++)
            {
                long mask = _masks[index];

                if (mask != -1L)
                {
                    return BitOperations.TrailingZeroCount(~mask) + index * IntSize;
                }
            }

            return _count * IntSize;
        }

        public bool Set(BitMap map)
        {
            EnsureCapacity(map._count * IntSize);

            bool modified = false;

            for (int index = 0; index < _count; index++)
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
            EnsureCapacity(map._count * IntSize);

            bool modified = false;

            for (int index = 0; index < _count; index++)
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

        private void EnsureCapacity(int size)
        {
            int count = (size + IntMask) / IntSize;

            if (count > _count)
            {
                var oldMask = _masks;
                var oldSpan = new Span<long>(_masks, _count);

                _masks = _allocator.Allocate<long>((uint)count);
                _count = count;

                var newSpan = new Span<long>(_masks, _count);

                oldSpan.CopyTo(newSpan);
                newSpan.Slice(oldSpan.Length).Clear();

                _allocator.Free(oldMask);
            }
        }

        public void Dispose()
        {
            if (_masks != null)
            {
                _allocator.Free(_masks);

                _masks = null;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator<int> IEnumerable<int>.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        public struct Enumerator : IEnumerator<int>
        {
            private int _index;
            private long _mask;
            private int _bit;
            private readonly BitMap _map;

            public int Current => _index * IntSize + _bit;
            object IEnumerator.Current => Current;

            public Enumerator(BitMap map)
            {
                _index = -1;
                _mask = 0;
                _bit = 0;
                _map = map;
            }

            public bool MoveNext()
            {
                if (_mask != 0)
                {
                    _mask &= ~(1L << _bit);
                }

                while (_mask == 0)
                {
                    if (++_index >= _map._count)
                    {
                        return false;
                    }

                    _mask = _map._masks[_index];
                }

                _bit = BitOperations.TrailingZeroCount(_mask);

                return true;
            }

            public void Reset() { }

            public void Dispose() { }
        }
    }
}