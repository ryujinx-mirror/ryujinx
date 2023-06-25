using System;

namespace Ryujinx.Tests.Unicorn
{
    [Flags]
    public enum MemoryPermission
    {
        None = 0,
        Read = 1,
        Write = 2,
        Exec = 4,
        All = 7,
    }
}
