using System.IO;

namespace Ryujinx.Graphics.VDec
{
    class BitStreamWriter
    {
        private const int BufferSize = 8;

        private Stream BaseStream;

        private int Buffer;
        private int BufferPos;

        public BitStreamWriter(Stream BaseStream)
        {
            this.BaseStream = BaseStream;
        }

        public void WriteBit(bool Value)
        {
            WriteBits(Value ? 1 : 0, 1);
        }

        public void WriteBits(int Value, int ValueSize)
        {
            int ValuePos = 0;

            int Remaining = ValueSize;

            while (Remaining > 0)
            {
                int CopySize = Remaining;

                int Free = GetFreeBufferBits();

                if (CopySize > Free)
                {
                    CopySize = Free;
                }

                int Mask = (1 << CopySize) - 1;

                int SrcShift = (ValueSize  - ValuePos)  - CopySize;
                int DstShift = (BufferSize - BufferPos) - CopySize;

                Buffer |= ((Value >> SrcShift) & Mask) << DstShift;

                ValuePos  += CopySize;
                BufferPos += CopySize;
                Remaining -= CopySize;
            }
        }

        private int GetFreeBufferBits()
        {
            if (BufferPos == BufferSize)
            {
                Flush();
            }

            return BufferSize - BufferPos;
        }

        public void Flush()
        {
            if (BufferPos != 0)
            {
                BaseStream.WriteByte((byte)Buffer);

                Buffer    = 0;
                BufferPos = 0;
            }
        }
    }
}