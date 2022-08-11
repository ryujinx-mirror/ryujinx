using System;
using System.Runtime.InteropServices;

namespace Spv.Generator
{
    public struct InstructionOperands
    {
        private const int InternalCount = 5;

        public int Count;
        public Operand Operand1;
        public Operand Operand2;
        public Operand Operand3;
        public Operand Operand4;
        public Operand Operand5;
        public Operand[] Overflow;

        public Span<Operand> AsSpan()
        {
            if (Count > InternalCount)
            {
                return MemoryMarshal.CreateSpan(ref this.Overflow[0], Count);
            }
            else
            {
                return MemoryMarshal.CreateSpan(ref this.Operand1, Count);
            }
        }

        public void Add(Operand operand)
        {
            if (Count < InternalCount)
            {
                MemoryMarshal.CreateSpan(ref this.Operand1, Count + 1)[Count] = operand;
                Count++;
            }
            else
            {
                if (Overflow == null)
                {
                    Overflow = new Operand[InternalCount * 2];
                    MemoryMarshal.CreateSpan(ref this.Operand1, InternalCount).CopyTo(Overflow.AsSpan());
                }
                else if (Count == Overflow.Length)
                {
                    Array.Resize(ref Overflow, Overflow.Length * 2);
                }

                Overflow[Count++] = operand;
            }
        }
    }
}
