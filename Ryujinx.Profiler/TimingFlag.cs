namespace Ryujinx.Profiler
{
    public enum TimingFlagType
    {
        FrameSwap   = 0,
        SystemFrame = 1,

        // Update this for new flags
        Count       = 2,
    }

    public struct TimingFlag
    {
        public TimingFlagType FlagType;
        public long Timestamp;
    }
}
