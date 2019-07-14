using Ryujinx.HLE.HOS.Kernel.Threading;

namespace Ryujinx.HLE.HOS.Services.Time.Clock
{
    class StandardUserSystemClockCore : SystemClockCore
    {
        private StandardLocalSystemClockCore   _localSystemClockCore;
        private StandardNetworkSystemClockCore _networkSystemClockCore;
        private bool                           _autoCorrectionEnabled;

        private static StandardUserSystemClockCore instance;

        public static StandardUserSystemClockCore Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new StandardUserSystemClockCore(StandardLocalSystemClockCore.Instance, StandardNetworkSystemClockCore.Instance);
                }

                return instance;
            }
        }

        public StandardUserSystemClockCore(StandardLocalSystemClockCore localSystemClockCore, StandardNetworkSystemClockCore networkSystemClockCore)
        {
            _localSystemClockCore   = localSystemClockCore;
            _networkSystemClockCore = networkSystemClockCore;
            _autoCorrectionEnabled  = false;
        }

        public override ResultCode Flush(SystemClockContext context)
        {
            return ResultCode.NotImplemented;
        }

        public override SteadyClockCore GetSteadyClockCore()
        {
            return _localSystemClockCore.GetSteadyClockCore();
        }

        public override ResultCode GetSystemClockContext(KThread thread, out SystemClockContext context)
        {
            ResultCode result = ApplyAutomaticCorrection(thread, false);

            context = new SystemClockContext();

            if (result == ResultCode.Success)
            {
                return _localSystemClockCore.GetSystemClockContext(thread, out context);
            }

            return result;
        }

        public override ResultCode SetSystemClockContext(SystemClockContext context)
        {
            return ResultCode.NotImplemented;
        }

        private ResultCode ApplyAutomaticCorrection(KThread thread, bool autoCorrectionEnabled)
        {
            ResultCode result = ResultCode.Success;

            if (_autoCorrectionEnabled != autoCorrectionEnabled && _networkSystemClockCore.IsClockSetup(thread))
            {
                result = _networkSystemClockCore.GetSystemClockContext(thread, out SystemClockContext context);

                if (result == ResultCode.Success)
                {
                    _localSystemClockCore.SetSystemClockContext(context);
                }
            }

            return result;
        }

        public ResultCode SetAutomaticCorrectionEnabled(KThread thread, bool autoCorrectionEnabled)
        {
            ResultCode result = ApplyAutomaticCorrection(thread, autoCorrectionEnabled);

            if (result == ResultCode.Success)
            {
                _autoCorrectionEnabled = autoCorrectionEnabled;
            }

            return result;
        }

        public bool IsAutomaticCorrectionEnabled()
        {
            return _autoCorrectionEnabled;
        }
    }
}
