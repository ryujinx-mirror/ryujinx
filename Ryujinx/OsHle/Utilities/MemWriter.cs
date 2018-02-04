using ChocolArm64.Memory;

namespace Ryujinx.OsHle.Utilities
{
    class MemWriter
    {
        private AMemory Memory;

        public long Position { get; private set; }

        public MemWriter(AMemory Memory, long Position)
        {
            this.Memory   = Memory;
            this.Position = Position;
        }

        public void WriteByte(byte Value)
        {
            Memory.WriteByte(Position, Value);

            Position++;
        }

        public void WriteInt32(int Value)
        {
            Memory.WriteInt32(Position, Value);

            Position += 4;
        }

        public void WriteInt64(long Value)
        {
            Memory.WriteInt64(Position, Value);

            Position += 8;
        }
    }
}