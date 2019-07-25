namespace Ryujinx.HLE.HOS.Services.Time.Clock
{
    class EphemeralNetworkSystemClockCore : SystemClockCore
    {
        private static EphemeralNetworkSystemClockCore _instance;

        public static EphemeralNetworkSystemClockCore Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new EphemeralNetworkSystemClockCore(TickBasedSteadyClockCore.Instance);
                }

                return _instance;
            }
        }

        public EphemeralNetworkSystemClockCore(SteadyClockCore steadyClockCore) : base(steadyClockCore) { }

        public override ResultCode Flush(SystemClockContext context)
        {
            return ResultCode.Success;
        }
    }
}
