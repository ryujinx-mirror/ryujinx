using System;
using System.Numerics;

namespace Ryujinx.Horizon.Sdk.Ngc.Detail
{
    class SbvRank
    {
        private const int BitsPerWord = Set.BitsPerWord;
        private const int Rank1Entries = 8;
        private const int BitsPerRank0Entry = BitsPerWord * Rank1Entries;

        private uint[] _rank0;
        private byte[] _rank1;

        public SbvRank()
        {
        }

        public SbvRank(ReadOnlySpan<uint> bitmap, int setCapacity)
        {
            Build(bitmap, setCapacity);
        }

        public void Build(ReadOnlySpan<uint> bitmap, int setCapacity)
        {
            _rank0 = new uint[CalculateRank0Length(setCapacity)];
            _rank1 = new byte[CalculateRank1Length(setCapacity)];

            BuildRankDictionary(_rank0, _rank1, (setCapacity + BitsPerWord - 1) / BitsPerWord, bitmap);
        }

        private static void BuildRankDictionary(Span<uint> rank0, Span<byte> rank1, int length, ReadOnlySpan<uint> bitmap)
        {
            uint rank0Count;
            uint rank1Count = 0;

            for (int index = 0; index < length; index++)
            {
                if ((index % Rank1Entries) != 0)
                {
                    rank0Count = rank0[index / Rank1Entries];
                }
                else
                {
                    rank0[index / Rank1Entries] = rank1Count;
                    rank0Count = rank1Count;
                }

                rank1[index] = (byte)(rank1Count - rank0Count);

                rank1Count += (uint)BitOperations.PopCount(bitmap[index]);
            }
        }

        public bool Import(ref BinaryReader reader, int setCapacity)
        {
            if (setCapacity == 0)
            {
                return true;
            }

            int rank0Length = CalculateRank0Length(setCapacity);
            int rank1Length = CalculateRank1Length(setCapacity);

            return reader.AllocateAndReadArray(ref _rank0, rank0Length) == rank0Length &&
                reader.AllocateAndReadArray(ref _rank1, rank1Length) == rank1Length;
        }

        public int CalcRank1(int index, uint[] membershipBitmap)
        {
            int rank0Index = index / BitsPerRank0Entry;
            int rank1Index = index / BitsPerWord;

            uint membershipBits = membershipBitmap[rank1Index] & (uint.MaxValue >> (BitsPerWord - 1 - (index % BitsPerWord)));

            return (int)_rank0[rank0Index] + _rank1[rank1Index] + BitOperations.PopCount(membershipBits);
        }

        public int CalcSelect0(int index, int length, uint[] membershipBitmap)
        {
            int rank0Index;

            if (length > BitsPerRank0Entry)
            {
                int left = 0;
                int right = (length + BitsPerRank0Entry - 1) / BitsPerRank0Entry;

                while (true)
                {
                    int range = right - left;
                    if (range < 0)
                    {
                        range++;
                    }

                    int middle = left + (range / 2);

                    int foundIndex = middle * BitsPerRank0Entry - (int)_rank0[middle];

                    if ((uint)foundIndex <= (uint)index)
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

                rank0Index = left;
            }
            else
            {
                rank0Index = 0;
            }

            int lengthInWords = (length + BitsPerWord - 1) / BitsPerWord;
            int rank1WordsCount = rank0Index == (length / BitsPerRank0Entry) && (lengthInWords % Rank1Entries) != 0
                ? lengthInWords % Rank1Entries
                : Rank1Entries;

            int baseIndex = (int)_rank0[rank0Index] + rank0Index * -BitsPerRank0Entry + index;
            int plainIndex;
            int count;
            int remainingBits;
            uint membershipBits;

            for (plainIndex = rank0Index * Rank1Entries - 1, count = 0; count < rank1WordsCount; plainIndex++, count++)
            {
                int currentIndex = baseIndex + count * -BitsPerWord;

                if (_rank1[plainIndex + 1] + currentIndex < 0)
                {
                    remainingBits = _rank1[plainIndex] + currentIndex + BitsPerWord;
                    membershipBits = ~membershipBitmap[plainIndex];

                    return plainIndex * BitsPerWord + SbvSelect.SelectPos(membershipBits, remainingBits);
                }
            }

            remainingBits = _rank1[plainIndex] + baseIndex + (rank1WordsCount - 1) * -BitsPerWord;
            membershipBits = ~membershipBitmap[plainIndex];

            return plainIndex * BitsPerWord + SbvSelect.SelectPos(membershipBits, remainingBits);
        }

        private static int CalculateRank0Length(int setCapacity)
        {
            return (setCapacity / (BitsPerWord * Rank1Entries)) + 1;
        }

        private static int CalculateRank1Length(int setCapacity)
        {
            return (setCapacity / BitsPerWord) + 1;
        }
    }
}
