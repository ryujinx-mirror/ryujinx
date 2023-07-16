using System;
using System.Numerics;

namespace Ryujinx.HLE.HOS.Kernel.Process
{
    class KContextIdManager
    {
        private const int IdMasksCount = 8;

        private readonly int[] _idMasks;

        private int _nextFreeBitHint;

        public KContextIdManager()
        {
            _idMasks = new int[IdMasksCount];
        }

        public int GetId()
        {
            lock (_idMasks)
            {
                int id = 0;

                if (!TestBit(_nextFreeBitHint))
                {
                    id = _nextFreeBitHint;
                }
                else
                {
                    for (int index = 0; index < IdMasksCount; index++)
                    {
                        int mask = _idMasks[index];

                        int firstFreeBit = BitOperations.LeadingZeroCount((uint)((mask + 1) & ~mask));

                        if (firstFreeBit < 32)
                        {
                            int baseBit = index * 32 + 31;

                            id = baseBit - firstFreeBit;

                            break;
                        }
                        else if (index == IdMasksCount - 1)
                        {
                            throw new InvalidOperationException("Maximum number of Ids reached!");
                        }
                    }
                }

                _nextFreeBitHint = id + 1;

                SetBit(id);

                return id;
            }
        }

        public void PutId(int id)
        {
            lock (_idMasks)
            {
                ClearBit(id);
            }
        }

        private bool TestBit(int bit)
        {
            return (_idMasks[bit / 32] & (1 << (bit & 31))) != 0;
        }

        private void SetBit(int bit)
        {
            _idMasks[bit / 32] |= (1 << (bit & 31));
        }

        private void ClearBit(int bit)
        {
            _idMasks[bit / 32] &= ~(1 << (bit & 31));
        }
    }
}
