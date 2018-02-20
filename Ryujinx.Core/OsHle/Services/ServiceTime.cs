using Ryujinx.Core.OsHle.Objects.Time;

using static Ryujinx.Core.OsHle.Objects.ObjHelper;

namespace Ryujinx.Core.OsHle.Services
{
    static partial class Service
    {
        public static long TimeGetStandardUserSystemClock(ServiceCtx Context)
        {
            MakeObject(Context, new ISystemClock(SystemClockType.User));

            return 0;
        }

        public static long TimeGetStandardNetworkSystemClock(ServiceCtx Context)
        {
            MakeObject(Context, new ISystemClock(SystemClockType.Network));

            return 0;
        }

        public static long TimeGetStandardSteadyClock(ServiceCtx Context)
        {
            MakeObject(Context, new ISteadyClock());

            return 0;
        }

        public static long TimeGetTimeZoneService(ServiceCtx Context)
        {
            MakeObject(Context, new ITimeZoneService());

            return 0;
        }

        public static long TimeGetStandardLocalSystemClock(ServiceCtx Context)
        {
            MakeObject(Context, new ISystemClock(SystemClockType.Local));

            return 0;
        }

    }
}