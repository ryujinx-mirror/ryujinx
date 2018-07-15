using ChocolArm64.Memory;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.OsHle.Utilities
{
    class StructWriter
    {
        private AMemory Memory;

        public long Position { get; private set; }

        public StructWriter(AMemory Memory, long Position)
        {
            this.Memory   = Memory;
            this.Position = Position;
        }

        public void Write<T>(T Value) where T : struct
        {
            AMemoryHelper.Write(Memory, Position, Value);

            Position += Marshal.SizeOf<T>();
        }
    }
}
