using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Ryujinx.Cpu.AppleHv
{
    struct MachTimebaseInfo
    {
        public uint Numer;
        public uint Denom;
    }

    [SupportedOSPlatform("macos")]
    static partial class TimeApi
    {
        [LibraryImport("libc", SetLastError = true)]
        public static partial ulong mach_absolute_time();

        [LibraryImport("libc", SetLastError = true)]
        public static partial int mach_timebase_info(out MachTimebaseInfo info);
    }
}
