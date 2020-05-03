using Ryujinx.Cpu;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.Utilities
{
    class StructWriter
    {
        private MemoryManager _memory;

        public long Position { get; private set; }

        public StructWriter(MemoryManager memory, long position)
        {
            _memory  = memory;
            Position = position;
        }

        public void Write<T>(T value) where T : struct
        {
            MemoryHelper.Write(_memory, Position, value);

            Position += Marshal.SizeOf<T>();
        }

        public void SkipBytes(long count)
        {
            Position += count;
        }
    }
}
