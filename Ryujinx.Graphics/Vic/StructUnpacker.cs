using Ryujinx.Graphics.Memory;
using System;

namespace Ryujinx.Graphics.Vic
{
    class StructUnpacker
    {
        private NvGpuVmm Vmm;

        private long Position;

        private ulong Buffer;
        private int   BuffPos;

        public StructUnpacker(NvGpuVmm Vmm, long Position)
        {
            this.Vmm      = Vmm;
            this.Position = Position;

            BuffPos = 64;
        }

        public int Read(int Bits)
        {
            if ((uint)Bits > 32)
            {
                throw new ArgumentOutOfRangeException(nameof(Bits));
            }

            int Value = 0;

            while (Bits > 0)
            {
                RefillBufferIfNeeded();

                int ReadBits = Bits;

                int MaxReadBits = 64 - BuffPos;

                if (ReadBits > MaxReadBits)
                {
                    ReadBits = MaxReadBits;
                }

                Value <<= ReadBits;

                Value |= (int)(Buffer >> BuffPos) & (int)(0xffffffff >> (32 - ReadBits));

                BuffPos += ReadBits;

                Bits -= ReadBits;
            }

            return Value;
        }

        private void RefillBufferIfNeeded()
        {
            if (BuffPos >= 64)
            {
                Buffer = Vmm.ReadUInt64(Position);

                Position += 8;

                BuffPos = 0;
            }
        }
    }
}