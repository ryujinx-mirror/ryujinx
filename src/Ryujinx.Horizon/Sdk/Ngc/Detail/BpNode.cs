namespace Ryujinx.Horizon.Sdk.Ngc.Detail
{
    class BpNode
    {
        private readonly Set _set = new();
        private SparseSet _sparseSet;
        private BpNode _nextNode;

        public Set Set => _set;

        public bool Import(ref BinaryReader reader)
        {
            if (!_set.Import(ref reader))
            {
                return false;
            }

            if (!reader.Read(out byte hasNext))
            {
                return false;
            }

            if (hasNext == 0)
            {
                return true;
            }

            _sparseSet = new();
            _nextNode = new();

            return _sparseSet.Import(ref reader) && _nextNode.Import(ref reader);
        }

        public int FindOpen(int index)
        {
            uint membershipBits = _set.BitVector.Array[index / Set.BitsPerWord];

            int wordBitOffset = index % Set.BitsPerWord;
            int unsetBits = 1;

            for (int bit = wordBitOffset - 1; bit >= 0; bit--)
            {
                if (((membershipBits >> bit) & 1) != 0)
                {
                    if (--unsetBits == 0)
                    {
                        return (index & ~(Set.BitsPerWord - 1)) | bit;
                    }
                }
                else
                {
                    unsetBits++;
                }
            }

            int plainIndex = _sparseSet.Rank1(index);
            if (plainIndex == 0)
            {
                return -1;
            }

            int newIndex = index;

            if (!_sparseSet.Has(index))
            {
                if (plainIndex == 0 || _nextNode == null)
                {
                    return -1;
                }

                newIndex = _sparseSet.Select1(plainIndex);
                if (newIndex < 0)
                {
                    return -1;
                }
            }
            else
            {
                plainIndex--;
            }

            int openIndex = _nextNode.FindOpen(plainIndex);
            if (openIndex < 0)
            {
                return -1;
            }

            int openSparseIndex = _sparseSet.Select1(openIndex);
            if (openSparseIndex < 0)
            {
                return -1;
            }

            if (newIndex != index)
            {
                unsetBits = 1;

                for (int bit = newIndex % Set.BitsPerWord - 1; bit > wordBitOffset; bit--)
                {
                    unsetBits += ((membershipBits >> bit) & 1) != 0 ? -1 : 1;
                }

                int bestCandidate = -1;

                membershipBits = _set.BitVector.Array[openSparseIndex / Set.BitsPerWord];

                for (int bit = openSparseIndex % Set.BitsPerWord + 1; bit < Set.BitsPerWord; bit++)
                {
                    if (unsetBits - 1 == 0)
                    {
                        bestCandidate = bit;
                    }

                    unsetBits += ((membershipBits >> bit) & 1) != 0 ? -1 : 1;
                }

                return (openSparseIndex & ~(Set.BitsPerWord - 1)) | bestCandidate;
            }
            else
            {
                return openSparseIndex;
            }
        }

        public int Enclose(int index)
        {
            uint membershipBits = _set.BitVector.Array[index / Set.BitsPerWord];

            int unsetBits = 1;

            for (int bit = index % Set.BitsPerWord - 1; bit >= 0; bit--)
            {
                if (((membershipBits >> bit) & 1) != 0)
                {
                    if (--unsetBits == 0)
                    {
                        return (index & ~(Set.BitsPerWord - 1)) + bit;
                    }
                }
                else
                {
                    unsetBits++;
                }
            }

            int setBits = 2;

            for (int bit = index % Set.BitsPerWord + 1; bit < Set.BitsPerWord; bit++)
            {
                if (((membershipBits >> bit) & 1) != 0)
                {
                    setBits++;
                }
                else
                {
                    if (--setBits == 0)
                    {
                        return FindOpen((index & ~(Set.BitsPerWord - 1)) + bit);
                    }
                }
            }

            int newIndex = index;

            if (!_sparseSet.Has(index))
            {
                newIndex = _sparseSet.Select1(_sparseSet.Rank1(index));
                if (newIndex < 0)
                {
                    return -1;
                }
            }

            if (!_set.Has(newIndex))
            {
                newIndex = FindOpen(newIndex);
                if (newIndex < 0)
                {
                    return -1;
                }
            }
            else
            {
                newIndex = _nextNode.Enclose(_sparseSet.Rank1(newIndex) - 1);
                if (newIndex < 0)
                {
                    return -1;
                }

                newIndex = _sparseSet.Select1(newIndex);
            }

            int nearestIndex = _sparseSet.Select1(_sparseSet.Rank1(newIndex));
            if (nearestIndex < 0)
            {
                return -1;
            }

            setBits = 0;

            membershipBits = _set.BitVector.Array[newIndex / Set.BitsPerWord];

            if ((newIndex / Set.BitsPerWord) == (nearestIndex / Set.BitsPerWord))
            {
                for (int bit = nearestIndex % Set.BitsPerWord - 1; bit >= newIndex % Set.BitsPerWord; bit--)
                {
                    if (((membershipBits >> bit) & 1) != 0)
                    {
                        if (++setBits > 0)
                        {
                            return (newIndex & ~(Set.BitsPerWord - 1)) + bit;
                        }
                    }
                    else
                    {
                        setBits--;
                    }
                }
            }
            else
            {
                for (int bit = Set.BitsPerWord - 1; bit >= newIndex % Set.BitsPerWord; bit--)
                {
                    if (((membershipBits >> bit) & 1) != 0)
                    {
                        if (++setBits > 0)
                        {
                            return (newIndex & ~(Set.BitsPerWord - 1)) + bit;
                        }
                    }
                    else
                    {
                        setBits--;
                    }
                }
            }

            return -1;
        }
    }
}
