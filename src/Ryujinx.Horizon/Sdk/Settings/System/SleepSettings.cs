using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Horizon.Sdk.Settings.System
{
    [Flags]
    enum SleepFlag : uint
    {
        SleepsWhilePlayingMedia = 1 << 0,
        WakesAtPowerStateChange = 1 << 1,
    }

    enum HandheldSleepPlan : uint
    {
        At1Min,
        At3Min,
        At5Min,
        At10Min,
        At30Min,
        Never,
    }

    enum ConsoleSleepPlan : uint
    {
        At1Hour,
        At2Hour,
        At3Hour,
        At6Hour,
        At12Hour,
        Never,
    }

    [StructLayout(LayoutKind.Sequential, Size = 0xC, Pack = 0x4)]
    struct SleepSettings
    {
        public SleepFlag Flags;
        public HandheldSleepPlan HandheldSleepPlan;
        public ConsoleSleepPlan ConsoleSleepPlan;
    }
}
