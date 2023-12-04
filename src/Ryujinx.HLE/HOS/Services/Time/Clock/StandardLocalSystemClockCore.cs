namespace Ryujinx.HLE.HOS.Services.Time.Clock
{
    class StandardLocalSystemClockCore : SystemClockCore
    {
        public StandardLocalSystemClockCore(StandardSteadyClockCore steadyClockCore) : base(steadyClockCore) { }
    }
}
