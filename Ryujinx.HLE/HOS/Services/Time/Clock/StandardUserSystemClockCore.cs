using Ryujinx.HLE.HOS.Kernel.Threading;
using System;

namespace Ryujinx.HLE.HOS.Services.Time.Clock
{
    class StandardUserSystemClockCore : SystemClockCore
    {
        private StandardLocalSystemClockCore   _localSystemClockCore;
        private StandardNetworkSystemClockCore _networkSystemClockCore;
        private bool                           _autoCorrectionEnabled;
        private SteadyClockTimePoint           _autoCorrectionTime;
        private KEvent                         _autoCorrectionEvent;

        public StandardUserSystemClockCore(StandardLocalSystemClockCore localSystemClockCore, StandardNetworkSystemClockCore networkSystemClockCore) : base(localSystemClockCore.GetSteadyClockCore())
        {
            _localSystemClockCore   = localSystemClockCore;
            _networkSystemClockCore = networkSystemClockCore;
            _autoCorrectionEnabled  = false;
            _autoCorrectionTime     = SteadyClockTimePoint.GetRandom();
            _autoCorrectionEvent    = null;
        }

        protected override ResultCode Flush(SystemClockContext context)
        {
            // As UserSystemClock isn't a real system clock, this shouldn't happens.
            throw new NotImplementedException();
        }

        public override ResultCode GetClockContext(KThread thread, out SystemClockContext context)
        {
            ResultCode result = ApplyAutomaticCorrection(thread, false);

            context = new SystemClockContext();

            if (result == ResultCode.Success)
            {
                return _localSystemClockCore.GetClockContext(thread, out context);
            }

            return result;
        }

        public override ResultCode SetClockContext(SystemClockContext context)
        {
            return ResultCode.NotImplemented;
        }

        private ResultCode ApplyAutomaticCorrection(KThread thread, bool autoCorrectionEnabled)
        {
            ResultCode result = ResultCode.Success;

            if (_autoCorrectionEnabled != autoCorrectionEnabled && _networkSystemClockCore.IsClockSetup(thread))
            {
                result = _networkSystemClockCore.GetClockContext(thread, out SystemClockContext context);

                if (result == ResultCode.Success)
                {
                    _localSystemClockCore.SetClockContext(context);
                }
            }

            return result;
        }

        internal void CreateAutomaticCorrectionEvent(Horizon system)
        {
            _autoCorrectionEvent = new KEvent(system.KernelContext);
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

        public KReadableEvent GetAutomaticCorrectionReadableEvent()
        {
            return _autoCorrectionEvent.ReadableEvent;
        }

        public void SetAutomaticCorrectionUpdatedTime(SteadyClockTimePoint steadyClockTimePoint)
        {
            _autoCorrectionTime = steadyClockTimePoint;
        }

        public SteadyClockTimePoint GetAutomaticCorrectionUpdatedTime()
        {
            return _autoCorrectionTime;
        }

        public void SignalAutomaticCorrectionEvent()
        {
            _autoCorrectionEvent.WritableEvent.Signal();
        }
    }
}
