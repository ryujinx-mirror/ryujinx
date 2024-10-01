namespace Ryujinx.Horizon.Sdk.MmNv
{
    class Session
    {
        public Module Module { get; }
        public uint Id { get; }
        public bool IsAutoClearEvent { get; }
        public uint ClockRateMin { get; private set; }
        public int ClockRateMax { get; private set; }

        public Session(uint id, Module module, bool isAutoClearEvent)
        {
            Module = module;
            Id = id;
            IsAutoClearEvent = isAutoClearEvent;
            ClockRateMin = 0;
            ClockRateMax = -1;
        }

        public void SetAndWait(uint clockRateMin, int clockRateMax)
        {
            ClockRateMin = clockRateMin;
            ClockRateMax = clockRateMax;
        }
    }
}
