using Ryujinx.HLE.OsHle.Ipc;
using System;
using System.Collections.Generic;

namespace Ryujinx.HLE.OsHle.Services.Time
{
    class IStaticService : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        private static readonly DateTime StartupDate = DateTime.UtcNow;

        public IStaticService()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0,   GetStandardUserSystemClock                 },
                { 1,   GetStandardNetworkSystemClock              },
                { 2,   GetStandardSteadyClock                     },
                { 3,   GetTimeZoneService                         },
                { 4,   GetStandardLocalSystemClock                },
                { 300, CalculateMonotonicSystemClockBaseTimePoint }
            };
        }

        public long GetStandardUserSystemClock(ServiceCtx Context)
        {
            MakeObject(Context, new ISystemClock(SystemClockType.User));

            return 0;
        }

        public long GetStandardNetworkSystemClock(ServiceCtx Context)
        {
            MakeObject(Context, new ISystemClock(SystemClockType.Network));

            return 0;
        }

        public long GetStandardSteadyClock(ServiceCtx Context)
        {
            MakeObject(Context, new ISteadyClock());

            return 0;
        }

        public long GetTimeZoneService(ServiceCtx Context)
        {
            MakeObject(Context, new ITimeZoneService());

            return 0;
        }

        public long GetStandardLocalSystemClock(ServiceCtx Context)
        {
            MakeObject(Context, new ISystemClock(SystemClockType.Local));

            return 0;
        }

        public long CalculateMonotonicSystemClockBaseTimePoint(ServiceCtx Context)
        {
            long TimeOffset              = (long)(DateTime.UtcNow - StartupDate).TotalSeconds;
            long SystemClockContextEpoch = Context.RequestData.ReadInt64();

            Context.ResponseData.Write(TimeOffset + SystemClockContextEpoch);

            return 0;
        }

    }
}