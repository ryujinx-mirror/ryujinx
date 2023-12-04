using System;
using System.Numerics;

namespace Ryujinx.Horizon.Sdk.Ngc.Detail
{
    class SbvSelect
    {
        private uint[] _array;
        private BitVector32 _bv1;
        private BitVector32 _bv2;
        private SbvRank _sbvRank1;
        private SbvRank _sbvRank2;

        public bool Import(ref BinaryReader reader)
        {
            if (!reader.Read(out int arrayLength) ||
                reader.AllocateAndReadArray(ref _array, arrayLength) != arrayLength)
            {
                return false;
            }

            _bv1 = new();
            _bv2 = new();
            _sbvRank1 = new();
            _sbvRank2 = new();

            return _bv1.Import(ref reader) &&
                _bv2.Import(ref reader) &&
                _sbvRank1.Import(ref reader, _bv1.BitLength) &&
                _sbvRank2.Import(ref reader, _bv2.BitLength);
        }

        public void Build(ReadOnlySpan<uint> bitmap, int length)
        {
            int lengthInWords = (length + Set.BitsPerWord - 1) / Set.BitsPerWord;

            int rank0Length = 0;
            int rank1Length = 0;

            if (lengthInWords != 0)
            {
                for (int index = 0; index < bitmap.Length; index++)
                {
                    uint value = bitmap[index];

                    if (value != 0)
                    {
                        rank0Length++;
                        rank1Length += BitOperations.PopCount(value);
                    }
                }
            }

            _bv1 = new(rank0Length);
            _bv2 = new(rank1Length);
            _array = new uint[rank0Length];

            bool setSequence = false;
            int arrayIndex = 0;
            uint unsetCount = 0;
            rank0Length = 0;
            rank1Length = 0;

            if (lengthInWords != 0)
            {
                for (int index = 0; index < bitmap.Length; index++)
                {
                    uint value = bitmap[index];

                    if (value != 0)
                    {
                        if (!setSequence)
                        {
                            _bv1.TurnOn(rank0Length);
                            _array[arrayIndex++] = unsetCount;
                            setSequence = true;
                        }

                        _bv2.TurnOn(rank1Length);

                        rank0Length++;
                        rank1Length += BitOperations.PopCount(value);
                    }
                    else
                    {
                        unsetCount++;
                        setSequence = false;
                    }
                }
            }

            _sbvRank1 = new(_bv1.Array, _bv1.BitLength);
            _sbvRank2 = new(_bv2.Array, _bv2.BitLength);
        }

        public int Select(Set set, int index)
        {
            if (index < _bv2.BitLength)
            {
                int rank1PlainIndex = _sbvRank2.CalcRank1(index, _bv2.Array);
                int rank0PlainIndex = _sbvRank1.CalcRank1(rank1PlainIndex - 1, _bv1.Array);

                int value = (int)_array[rank0PlainIndex - 1] + (rank1PlainIndex - 1);

                int baseBitIndex = 0;

                if (value != 0)
                {
                    baseBitIndex = value * 32;

                    int setBvLength = set.BitVector.BitLength;
                    int bitIndexBounded = baseBitIndex - 1;

                    if (bitIndexBounded >= setBvLength)
                    {
                        bitIndexBounded = setBvLength - 1;
                    }

                    index -= set.SbvRank.CalcRank1(bitIndexBounded, set.BitVector.Array);
                }

                return SelectPos(set.BitVector.Array[value], index) + baseBitIndex;
            }

            return -1;
        }

        public static int SelectPos(uint membershipBits, int bitIndex)
        {
            // Skips "bitIndex" set bits, and returns the bit index of the next set bit.
            // If there is no set bit after skipping the specified amount, returns 32.

            int bit;
            int bitCount = bitIndex;

            for (bit = 0; bit < sizeof(uint) * 8;)
            {
                if (((membershipBits >> bit) & 1) != 0)
                {
                    if (bitCount-- == 0)
                    {
                        break;
                    }

                    bit++;
                }
                else
                {
                    bit += BitOperations.TrailingZeroCount(membershipBits >> bit);
                }
            }

            return bit;
        }
    }
}
