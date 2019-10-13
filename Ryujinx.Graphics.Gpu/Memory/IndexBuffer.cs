using Ryujinx.Graphics.GAL;

namespace Ryujinx.Graphics.Gpu.Memory
{
    struct IndexBuffer
    {
        public ulong Address;
        public ulong Size;

        public IndexType Type;
    }
}