using System;

namespace Ryujinx.HLE.HOS.Services.Time
{
    class ISystemClock : IpcService
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private SystemClockType _clockType;
        private DateTime        _systemClockContextEpoch;
        private long            _systemClockTimePoint;
        private byte[]          _systemClockContextEnding;
        private long            _timeOffset;

        public ISystemClock(SystemClockType clockType)
        {
            _clockType                = clockType;
            _systemClockContextEpoch  = System.Diagnostics.Process.GetCurrentProcess().StartTime;
            _systemClockContextEnding = new byte[0x10];
            _timeOffset               = 0;

            if (clockType == SystemClockType.User ||
                clockType == SystemClockType.Network)
            {
                _systemClockContextEpoch = _systemClockContextEpoch.ToUniversalTime();
            }

            _systemClockTimePoint = (long)(_systemClockContextEpoch - Epoch).TotalSeconds;
        }

        [Command(0)]
        // GetCurrentTime() -> nn::time::PosixTime
        public ResultCode GetCurrentTime(ServiceCtx context)
        {
            DateTime currentTime = DateTime.Now;

            if (_clockType == SystemClockType.User ||
                _clockType == SystemClockType.Network)
            {
                currentTime = currentTime.ToUniversalTime();
            }

            context.ResponseData.Write((long)((currentTime - Epoch).TotalSeconds) + _timeOffset);

            return ResultCode.Success;
        }

        [Command(1)]
        // SetCurrentTime(nn::time::PosixTime)
        public ResultCode SetCurrentTime(ServiceCtx context)
        {
            DateTime currentTime = DateTime.Now;

            if (_clockType == SystemClockType.User ||
                _clockType == SystemClockType.Network)
            {
                currentTime = currentTime.ToUniversalTime();
            }

            _timeOffset = (context.RequestData.ReadInt64() - (long)(currentTime - Epoch).TotalSeconds);

            return ResultCode.Success;
        }

        [Command(2)]
        // GetSystemClockContext() -> nn::time::SystemClockContext
        public ResultCode GetSystemClockContext(ServiceCtx context)
        {
            context.ResponseData.Write((long)(_systemClockContextEpoch - Epoch).TotalSeconds);

            // The point in time, TODO: is there a link between epoch and this?
            context.ResponseData.Write(_systemClockTimePoint);

            // This seems to be some kind of identifier?
            for (int i = 0; i < 0x10; i++)
            {
                context.ResponseData.Write(_systemClockContextEnding[i]);
            }

            return ResultCode.Success;
        }

        [Command(3)]
        // SetSystemClockContext(nn::time::SystemClockContext)
        public ResultCode SetSystemClockContext(ServiceCtx context)
        {
            long newSystemClockEpoch     = context.RequestData.ReadInt64();
            long newSystemClockTimePoint = context.RequestData.ReadInt64();

            _systemClockContextEpoch     = Epoch.Add(TimeSpan.FromSeconds(newSystemClockEpoch));
            _systemClockTimePoint        = newSystemClockTimePoint;
            _systemClockContextEnding    = context.RequestData.ReadBytes(0x10);

            return ResultCode.Success;
        }
    }
}