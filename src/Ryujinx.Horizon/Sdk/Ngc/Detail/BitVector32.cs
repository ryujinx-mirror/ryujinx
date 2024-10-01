namespace Ryujinx.Horizon.Sdk.Ngc.Detail
{
    class BitVector32
    {
        private const int BitsPerWord = Set.BitsPerWord;

        private int _bitLength;
        private uint[] _array;

        public int BitLength => _bitLength;
        public uint[] Array => _array;

        public BitVector32()
        {
            _bitLength = 0;
            _array = null;
        }

        public BitVector32(int length)
        {
            _bitLength = length;
            _array = new uint[(length + BitsPerWord - 1) / BitsPerWord];
        }

        public bool Has(int index)
        {
            if ((uint)index < (uint)_bitLength)
            {
                int wordIndex = index / BitsPerWord;
                int wordBitOffset = index % BitsPerWord;

                return ((_array[wordIndex] >> wordBitOffset) & 1u) != 0;
            }

            return false;
        }

        public bool TurnOn(int index, int count)
        {
            for (int bit = 0; bit < count; bit++)
            {
                if (!TurnOn(index + bit))
                {
                    return false;
                }
            }

            return true;
        }

        public bool TurnOn(int index)
        {
            if ((uint)index < (uint)_bitLength)
            {
                int wordIndex = index / BitsPerWord;
                int wordBitOffset = index % BitsPerWord;

                _array[wordIndex] |= 1u << wordBitOffset;

                return true;
            }

            return false;
        }

        public bool Import(ref BinaryReader reader)
        {
            if (!reader.Read(out _bitLength))
            {
                return false;
            }

            int arrayLength = (_bitLength + BitsPerWord - 1) / BitsPerWord;

            return reader.AllocateAndReadArray(ref _array, arrayLength) == arrayLength;
        }
    }
}
