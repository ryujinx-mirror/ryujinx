using System;

namespace Ryujinx.OsHle.Objects
{
    class TimeISystemClock
    {
        private static DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long GetCurrentTime(ServiceCtx Context)
        {
            Context.ResponseData.Write((long)(DateTime.Now - Epoch).TotalSeconds);

            return 0;
        }
    }
}