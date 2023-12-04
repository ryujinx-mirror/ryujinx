using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Sdb.Pdm.QueryService.Types
{
    [StructLayout(LayoutKind.Sequential, Size = 0x18)]
    struct ApplicationPlayStatistics
    {
        public ulong TitleId;
        public long TotalPlayTime; // In nanoseconds.
        public long TotalLaunchCount;
    }
}
