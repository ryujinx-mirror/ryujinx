namespace Ryujinx.Horizon.Sdk.Ngc.Detail
{
    class SparseSet
    {
        private const int BitsPerWord = Set.BitsPerWord;

        private ulong _rangeValuesCount;
        private ulong _rangeStartValue;
        private ulong _rangeEndValue;
        private uint _count;
        private uint _bitfieldLength;
        private uint[] _bitfields;
        private readonly Sbv _sbv = new();

        public ulong RangeValuesCount => _rangeValuesCount;
        public ulong RangeEndValue => _rangeEndValue;

        public bool Import(ref BinaryReader reader)
        {
            if (!reader.Read(out _rangeValuesCount) ||
                !reader.Read(out _rangeStartValue) ||
                !reader.Read(out _rangeEndValue) ||
                !reader.Read(out _count) ||
                !reader.Read(out _bitfieldLength) ||
                !reader.Read(out int arrayLength) ||
                reader.AllocateAndReadArray(ref _bitfields, arrayLength) != arrayLength)
            {
                return false;
            }

            return _sbv.Import(ref reader);
        }

        public bool Has(long index)
        {
            int plainIndex = Rank1(index);

            return plainIndex != 0 && Select1Ex(plainIndex - 1) == index;
        }

        public int Rank1(long index)
        {
            uint count = _count;

            if ((ulong)index < _rangeStartValue || count == 0)
            {
                return 0;
            }

            if (_rangeStartValue == (ulong)index || count < 3)
            {
                return 1;
            }

            if (_rangeEndValue <= (ulong)index)
            {
                return (int)count;
            }

            int left = 0;
            int right = (int)count - 1;

            while (true)
            {
                int range = right - left;
                if (range < 0)
                {
                    range++;
                }

                int middle = left + (range / 2);

                long foundIndex = Select1Ex(middle);

                if ((ulong)foundIndex <= (ulong)index)
                {
                    left = middle;
                }
                else
                {
                    right = middle;
                }

                if (right <= left + 1)
                {
                    break;
                }
            }

            return left + 1;
        }

        public int Select1(int index)
        {
            return (int)Select1Ex(index);
        }

        public long Select1Ex(int index)
        {
            if ((uint)index >= _count)
            {
                return -1L;
            }

            int indexOffset = _sbv.SbvSelect.Select(_sbv.Set, index);
            int bitfieldLength = (int)_bitfieldLength;

            int currentBitIndex = index * bitfieldLength;
            int wordIndex = currentBitIndex / BitsPerWord;
            int wordBitOffset = currentBitIndex % BitsPerWord;

            ulong value = _bitfields[wordIndex];

            if (wordBitOffset + bitfieldLength > BitsPerWord)
            {
                value |= (ulong)_bitfields[wordIndex + 1] << 32;
            }

            value >>= wordBitOffset;
            value &= uint.MaxValue >> (BitsPerWord - bitfieldLength);

            return ((indexOffset - (uint)index) << bitfieldLength) + (int)value;
        }
    }
}
