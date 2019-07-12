using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Common;
using System;

namespace Ryujinx.HLE.HOS.Services.Time
{
    [Service("time:a")]
    [Service("time:s")]
    [Service("time:u")]
    class IStaticService : IpcService
    {
        private int _timeSharedMemoryNativeHandle = 0;

        private static readonly DateTime StartupDate = DateTime.UtcNow;

        public IStaticService(ServiceCtx context) { }

        [Command(0)]
        // GetStandardUserSystemClock() -> object<nn::timesrv::detail::service::ISystemClock>
        public long GetStandardUserSystemClock(ServiceCtx context)
        {
            MakeObject(context, new ISystemClock(SystemClockType.User));

            return 0;
        }

        [Command(1)]
        // GetStandardNetworkSystemClock() -> object<nn::timesrv::detail::service::ISystemClock>
        public long GetStandardNetworkSystemClock(ServiceCtx context)
        {
            MakeObject(context, new ISystemClock(SystemClockType.Network));

            return 0;
        }

        [Command(2)]
        // GetStandardSteadyClock() -> object<nn::timesrv::detail::service::ISteadyClock>
        public long GetStandardSteadyClock(ServiceCtx context)
        {
            MakeObject(context, new ISteadyClock());

            return 0;
        }

        [Command(3)]
        // GetTimeZoneService() -> object<nn::timesrv::detail::service::ITimeZoneService>
        public long GetTimeZoneService(ServiceCtx context)
        {
            MakeObject(context, new ITimeZoneService());

            return 0;
        }

        [Command(4)]
        // GetStandardLocalSystemClock() -> object<nn::timesrv::detail::service::ISystemClock>
        public long GetStandardLocalSystemClock(ServiceCtx context)
        {
            MakeObject(context, new ISystemClock(SystemClockType.Local));

            return 0;
        }

        [Command(20)] // 6.0.0+
        // GetSharedMemoryNativeHandle() -> handle<copy>
        public long GetSharedMemoryNativeHandle(ServiceCtx context)
        {
            if (_timeSharedMemoryNativeHandle == 0)
            {
                if (context.Process.HandleTable.GenerateHandle(context.Device.System.TimeSharedMem, out _timeSharedMemoryNativeHandle) != KernelResult.Success)
                {
                    throw new InvalidOperationException("Out of handles!");
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_timeSharedMemoryNativeHandle);

            return 0;
        }

        [Command(300)] // 4.0.0+
        // CalculateMonotonicSystemClockBaseTimePoint(nn::time::SystemClockContext) -> u64
        public long CalculateMonotonicSystemClockBaseTimePoint(ServiceCtx context)
        {
            long timeOffset              = (long)(DateTime.UtcNow - StartupDate).TotalSeconds;
            long systemClockContextEpoch = context.RequestData.ReadInt64();

            context.ResponseData.Write(timeOffset + systemClockContextEpoch);

            return 0;
        }

    }
}