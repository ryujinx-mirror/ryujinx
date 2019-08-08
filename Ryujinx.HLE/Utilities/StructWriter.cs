using ARMeilleure.Memory;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.Utilities
{
    class StructWriter
    {
        private IMemoryManager _memory;

        public long Position { get; private set; }

        public StructWriter(IMemoryManager memory, long position)
        {
            _memory  = memory;
            Position = position;
        }

        public void Write<T>(T value) where T : struct
        {
            MemoryHelper.Write(_memory, Position, value);

            Position += Marshal.SizeOf<T>();
        }
    }
}
