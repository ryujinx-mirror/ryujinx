using Ryujinx.HLE.HOS.Kernel.Memory;

namespace Ryujinx.HLE.HOS.Kernel.Ipc
{
    class KBufferDescriptor
    {
        public ulong ClientAddress { get; }
        public ulong ServerAddress { get; }
        public ulong Size { get; }
        public MemoryState State { get; }

        public KBufferDescriptor(ulong src, ulong dst, ulong size, MemoryState state)
        {
            ClientAddress = src;
            ServerAddress = dst;
            Size = size;
            State = state;
        }
    }
}
