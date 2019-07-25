namespace Ryujinx.HLE.HOS.Services.Time.Clock
{
    class StandardLocalSystemClockCore : SystemClockCore
    {
        private static StandardLocalSystemClockCore _instance;

        public static StandardLocalSystemClockCore Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new StandardLocalSystemClockCore(StandardSteadyClockCore.Instance);
                }

                return _instance;
            }
        }

        public StandardLocalSystemClockCore(StandardSteadyClockCore steadyClockCore) : base(steadyClockCore) {}

        public override ResultCode Flush(SystemClockContext context)
        {
            // TODO: set:sys SetUserSystemClockContext

            return ResultCode.Success;
        }
    }
}
