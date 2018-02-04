using Ryujinx.OsHle.Objects;

using static Ryujinx.OsHle.Objects.ObjHelper;

namespace Ryujinx.OsHle.Services
{
    static partial class Service
    {
        public static long TimeGetStandardUserSystemClock(ServiceCtx Context)
        {
            MakeObject(Context, new TimeISystemClock());

            return 0;
        }

        public static long TimeGetStandardNetworkSystemClock(ServiceCtx Context)
        {
            MakeObject(Context, new TimeISystemClock());

            return 0;
        }

        public static long TimeGetStandardSteadyClock(ServiceCtx Context)
        {
            MakeObject(Context, new TimeISteadyClock());

            return 0;
        }

        public static long TimeGetTimeZoneService(ServiceCtx Context)
        {
            MakeObject(Context, new TimeITimeZoneService());

            return 0;
        }
    }
}