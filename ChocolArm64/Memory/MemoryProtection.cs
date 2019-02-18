using System;

namespace ChocolArm64.Memory
{
    [Flags]
    public enum MemoryProtection
    {
        None    = 0,
        Read    = 1 << 0,
        Write   = 1 << 1,
        Execute = 1 << 2,

        ReadAndWrite   = Read | Write,
        ReadAndExecute = Read | Execute
    }
}