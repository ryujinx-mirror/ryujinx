namespace Ryujinx.Horizon.Sdk.Ngc.Detail
{
    class Set
    {
        public const int BitsPerWord = 32;

        private readonly BitVector32 _bitVector;
        private readonly SbvRank _sbvRank;

        public BitVector32 BitVector => _bitVector;
        public SbvRank SbvRank => _sbvRank;

        public Set()
        {
            _bitVector = new();
            _sbvRank = new();
        }

        public Set(int length)
        {
            _bitVector = new(length);
            _sbvRank = new();
        }

        public void Build()
        {
            _sbvRank.Build(_bitVector.Array, _bitVector.BitLength);
        }

        public bool Import(ref BinaryReader reader)
        {
            return _bitVector.Import(ref reader) && _sbvRank.Import(ref reader, _bitVector.BitLength);
        }

        public bool Has(int index)
        {
            return _bitVector.Has(index);
        }

        public bool TurnOn(int index, int count)
        {
            return _bitVector.TurnOn(index, count);
        }

        public bool TurnOn(int index)
        {
            return _bitVector.TurnOn(index);
        }

        public int Rank1(int index)
        {
            if ((uint)index >= (uint)_bitVector.BitLength)
            {
                index = _bitVector.BitLength - 1;
            }

            return _sbvRank.CalcRank1(index, _bitVector.Array);
        }

        public int Select0(int index)
        {
            int length = _bitVector.BitLength;
            int rankIndex = _sbvRank.CalcRank1(length - 1, _bitVector.Array);

            if ((uint)index < (uint)(length - rankIndex))
            {
                return _sbvRank.CalcSelect0(index, length, _bitVector.Array);
            }

            return -1;
        }
    }
}
