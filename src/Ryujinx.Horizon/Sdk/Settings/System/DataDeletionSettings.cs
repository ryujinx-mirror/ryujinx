using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Horizon.Sdk.Settings.System
{
    [Flags]
    enum DataDeletionFlag : uint
    {
        AutomaticDeletionFlag = 1 << 0,
    }

    [StructLayout(LayoutKind.Sequential, Size = 0x8, Pack = 0x4)]
    struct DataDeletionSettings
    {
        public DataDeletionFlag Flags;
        public uint UseCount;
    }
}
