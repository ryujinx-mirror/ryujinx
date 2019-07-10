using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Common;
using System;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Time
{
    [Service("time:a")]
    [Service("time:s")]
    [Service("time:u")]
    class IStaticService : IpcService
    {
        private int _timeSharedMemoryNativeHandle = 0;

        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        private static readonly DateTime StartupDate = DateTime.UtcNow;

        public IStaticService(ServiceCtx context)
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 0,   GetStandardUserSystemClock                               },
                { 1,   GetStandardNetworkSystemClock                            },
                { 2,   GetStandardSteadyClock                                   },
                { 3,   GetTimeZoneService                                       },
                { 4,   GetStandardLocalSystemClock                              },
              //{ 5,   GetEphemeralNetworkSystemClock                           }, // 4.0.0+
                { 20,  GetSharedMemoryNativeHandle                              }, // 6.0.0+
              //{ 30,  GetStandardNetworkClockOperationEventReadableHandle      }, // 6.0.0+
              //{ 31,  GetEphemeralNetworkClockOperationEventReadableHandle     }, // 6.0.0+
              //{ 50,  SetStandardSteadyClockInternalOffset                     }, // 4.0.0+
              //{ 100, IsStandardUserSystemClockAutomaticCorrectionEnabled      },
              //{ 101, SetStandardUserSystemClockAutomaticCorrectionEnabled     },
              //{ 102, GetStandardUserSystemClockInitialYear                    }, // 5.0.0+
              //{ 200, IsStandardNetworkSystemClockAccuracySufficient           }, // 3.0.0+
              //{ 201, GetStandardUserSystemClockAutomaticCorrectionUpdatedTime }, // 6.0.0+
                { 300, CalculateMonotonicSystemClockBaseTimePoint               }, // 4.0.0+
              //{ 400, GetClockSnapshot                                         }, // 4.0.0+
              //{ 401, GetClockSnapshotFromSystemClockContext                   }, // 4.0.0+
              //{ 500, CalculateStandardUserSystemClockDifferenceByUser         }, // 4.0.0+
              //{ 501, CalculateSpanBetween                                     }, // 4.0.0+
            };
        }

        // GetStandardUserSystemClock() -> object<nn::timesrv::detail::service::ISystemClock>
        public long GetStandardUserSystemClock(ServiceCtx context)
        {
            MakeObject(context, new ISystemClock(SystemClockType.User));

            return 0;
        }

        // GetStandardNetworkSystemClock() -> object<nn::timesrv::detail::service::ISystemClock>
        public long GetStandardNetworkSystemClock(ServiceCtx context)
        {
            MakeObject(context, new ISystemClock(SystemClockType.Network));

            return 0;
        }

        // GetStandardSteadyClock() -> object<nn::timesrv::detail::service::ISteadyClock>
        public long GetStandardSteadyClock(ServiceCtx context)
        {
            MakeObject(context, new ISteadyClock());

            return 0;
        }

        // GetTimeZoneService() -> object<nn::timesrv::detail::service::ITimeZoneService>
        public long GetTimeZoneService(ServiceCtx context)
        {
            MakeObject(context, new ITimeZoneService());

            return 0;
        }

        // GetStandardLocalSystemClock() -> object<nn::timesrv::detail::service::ISystemClock>
        public long GetStandardLocalSystemClock(ServiceCtx context)
        {
            MakeObject(context, new ISystemClock(SystemClockType.Local));

            return 0;
        }

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