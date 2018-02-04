using ChocolArm64.Memory;

namespace Ryujinx.OsHle.Utilities
{
    class MemReader
    {
        private AMemory Memory;

        public long Position { get; private set; }

        public MemReader(AMemory Memory, long Position)
        {
            this.Memory   = Memory;
            this.Position = Position;
        }

        public byte ReadByte()
        {
            byte Value = Memory.ReadByte(Position);

            Position++;

            return Value;
        }

        public int ReadInt32()
        {
            int Value = Memory.ReadInt32(Position);

            Position += 4;

            return Value;
        }

        public long ReadInt64()
        {
            long Value = Memory.ReadInt64(Position);

            Position += 8;

            return Value;
        }
    }
}