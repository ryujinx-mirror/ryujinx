using System;

namespace Ryujinx.Graphics.GAL
{
    [Flags]
    public enum BufferAccess
    {
        Default = 0,
        HostMemory = 1,
        DeviceMemory = 2,
        DeviceMemoryMapped = 3,

        MemoryTypeMask = 0xf,

        Stream = 1 << 4,
        SparseCompatible = 1 << 5,
    }
}
