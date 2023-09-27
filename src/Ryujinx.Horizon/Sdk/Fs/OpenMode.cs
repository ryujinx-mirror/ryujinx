using System;

namespace Ryujinx.Horizon.Sdk.Fs
{
    [Flags]
    public enum OpenMode
    {
        Read = 1,
        Write = 2,
        AllowAppend = 4,
        ReadWrite = 3,
        All = 7,
    }
}
