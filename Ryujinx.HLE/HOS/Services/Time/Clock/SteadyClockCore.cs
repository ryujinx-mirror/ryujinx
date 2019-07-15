using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.HOS.Services.Bpc;
using Ryujinx.HLE.Utilities;
using System;

namespace Ryujinx.HLE.HOS.Services.Time.Clock
{
    class SteadyClockCore
    {
        private long         _setupValue;
        private ResultCode   _setupResultCode;
        private bool         _isRtcResetDetected;
        private TimeSpanType _testOffset;
        private TimeSpanType _internalOffset;
        private UInt128      _clockSourceId;

        private static SteadyClockCore instance;

        public static SteadyClockCore Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SteadyClockCore();
                }

                return instance;
            }
        }

        private SteadyClockCore()
        {
            _testOffset     = new TimeSpanType(0);
            _internalOffset = new TimeSpanType(0);
            _clockSourceId  = new UInt128(Guid.NewGuid().ToByteArray());
        }

        private SteadyClockTimePoint GetTimePoint(KThread thread)
        {
            SteadyClockTimePoint result = new SteadyClockTimePoint
            {
                TimePoint     = 0,
                ClockSourceId = _clockSourceId
            };

            TimeSpanType ticksTimeSpan = TimeSpanType.FromTicks(thread.Context.ThreadState.CntpctEl0, thread.Context.ThreadState.CntfrqEl0);

            result.TimePoint = _setupValue + ticksTimeSpan.ToSeconds();

            return result;
        }

        public UInt128 GetClockSourceId()
        {
            return _clockSourceId;
        }

        public SteadyClockTimePoint GetCurrentTimePoint(KThread thread)
        {
            SteadyClockTimePoint result = GetTimePoint(thread);

            result.TimePoint += _testOffset.ToSeconds();
            result.TimePoint += _internalOffset.ToSeconds();

            return result;
        }

        public TimeSpanType GetTestOffset()
        {
            return _testOffset;
        }

        public void SetTestOffset(TimeSpanType testOffset)
        {
            _testOffset = testOffset;
        }

        public ResultCode GetRtcValue(out ulong rtcValue)
        {
            return (ResultCode)IRtcManager.GetExternalRtcValue(out rtcValue);
        }

        public bool IsRtcResetDetected()
        {
            return _isRtcResetDetected;
        }

        public ResultCode GetSetupResultCode()
        {
            return _setupResultCode;
        }

        public TimeSpanType GetInternalOffset()
        {
            return _internalOffset;
        }

        public void SetInternalOffset(TimeSpanType internalOffset)
        {
            _internalOffset = internalOffset;
        }

        public ResultCode GetSetupResultValue()
        {
            return _setupResultCode;
        }

        public void ConfigureSetupValue()
        {
            int retry = 0;

            ResultCode result = ResultCode.Success;

            while (retry < 20)
            {
                result = (ResultCode)IRtcManager.GetExternalRtcValue(out ulong rtcValue);

                if (result == ResultCode.Success)
                {
                    _setupValue = (long)rtcValue;
                    break;
                }

                retry++;
            }

            _setupResultCode = result;
        }
    }
}
