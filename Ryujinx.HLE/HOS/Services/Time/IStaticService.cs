using Ryujinx.HLE.HOS.Ipc;
using System;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Time
{
    class IStaticService : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        private static readonly DateTime StartupDate = DateTime.UtcNow;

        public IStaticService()
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 0,   GetStandardUserSystemClock                 },
                { 1,   GetStandardNetworkSystemClock              },
                { 2,   GetStandardSteadyClock                     },
                { 3,   GetTimeZoneService                         },
                { 4,   GetStandardLocalSystemClock                },
                { 300, CalculateMonotonicSystemClockBaseTimePoint }
            };
        }

        public long GetStandardUserSystemClock(ServiceCtx context)
        {
            MakeObject(context, new ISystemClock(SystemClockType.User));

            return 0;
        }

        public long GetStandardNetworkSystemClock(ServiceCtx context)
        {
            MakeObject(context, new ISystemClock(SystemClockType.Network));

            return 0;
        }

        public long GetStandardSteadyClock(ServiceCtx context)
        {
            MakeObject(context, new ISteadyClock());

            return 0;
        }

        public long GetTimeZoneService(ServiceCtx context)
        {
            MakeObject(context, new ITimeZoneService());

            return 0;
        }

        public long GetStandardLocalSystemClock(ServiceCtx context)
        {
            MakeObject(context, new ISystemClock(SystemClockType.Local));

            return 0;
        }

        public long CalculateMonotonicSystemClockBaseTimePoint(ServiceCtx context)
        {
            long timeOffset              = (long)(DateTime.UtcNow - StartupDate).TotalSeconds;
            long systemClockContextEpoch = context.RequestData.ReadInt64();

            context.ResponseData.Write(timeOffset + systemClockContextEpoch);

            return 0;
        }

    }
}