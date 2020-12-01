using System;

namespace Ryujinx.HLE.HOS.Kernel.Memory
{
    [Flags]
    enum KMemoryPermission : byte
    {
        None = 0,
        Mask = 0xff,

        Read    = 1 << 0,
        Write   = 1 << 1,
        Execute = 1 << 2,

        ReadAndWrite   = Read | Write,
        ReadAndExecute = Read | Execute
    }
}