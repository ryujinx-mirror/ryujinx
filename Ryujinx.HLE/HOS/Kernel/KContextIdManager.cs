using Ryujinx.Common;
using System;

namespace Ryujinx.HLE.HOS.Kernel
{
    class KContextIdManager
    {
        private const int IdMasksCount = 8;

        private int[] IdMasks;

        private int NextFreeBitHint;

        public KContextIdManager()
        {
            IdMasks = new int[IdMasksCount];
        }

        public int GetId()
        {
            lock (IdMasks)
            {
                int Id = 0;

                if (!TestBit(NextFreeBitHint))
                {
                    Id = NextFreeBitHint;
                }
                else
                {
                    for (int Index = 0; Index < IdMasksCount; Index++)
                    {
                        int Mask = IdMasks[Index];

                        int FirstFreeBit = BitUtils.CountLeadingZeros32((Mask + 1) & ~Mask);

                        if (FirstFreeBit < 32)
                        {
                            int BaseBit = Index * 32 + 31;

                            Id = BaseBit - FirstFreeBit;

                            break;
                        }
                        else if (Index == IdMasksCount - 1)
                        {
                            throw new InvalidOperationException("Maximum number of Ids reached!");
                        }
                    }
                }

                NextFreeBitHint = Id + 1;

                SetBit(Id);

                return Id;
            }
        }

        public void PutId(int Id)
        {
            lock (IdMasks)
            {
                ClearBit(Id);
            }
        }

        private bool TestBit(int Bit)
        {
            return (IdMasks[NextFreeBitHint / 32] & (1 << (NextFreeBitHint & 31))) != 0;
        }

        private void SetBit(int Bit)
        {
            IdMasks[NextFreeBitHint / 32] |= (1 << (NextFreeBitHint & 31));
        }

        private void ClearBit(int Bit)
        {
            IdMasks[NextFreeBitHint / 32] &= ~(1 << (NextFreeBitHint & 31));
        }
    }
}