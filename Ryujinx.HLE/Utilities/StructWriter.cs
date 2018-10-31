using ChocolArm64.Memory;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.Utilities
{
    class StructWriter
    {
        private MemoryManager Memory;

        public long Position { get; private set; }

        public StructWriter(MemoryManager Memory, long Position)
        {
            this.Memory   = Memory;
            this.Position = Position;
        }

        public void Write<T>(T Value) where T : struct
        {
            MemoryHelper.Write(Memory, Position, Value);

            Position += Marshal.SizeOf<T>();
        }
    }
}
