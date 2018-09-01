using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Tests.Unicorn.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct UnicornMemoryRegion
    {
        public UInt64 begin; // begin address of the region (inclusive)
        public UInt64 end;   // end address of the region (inclusive)
        public UInt32 perms; // memory permissions of the region
    }
}
