using System;

namespace Ryujinx.HLE.HOS.Services.Bpc
{
    [Service("bpc:r")]
    class IRtcManager : IpcService
    {
        public IRtcManager(ServiceCtx context) { }

        [Command(0)]
        // GetRtcTime() -> u64
        public ResultCode GetRtcTime(ServiceCtx context)
        {
            ResultCode result = GetExternalRtcValue(out ulong rtcValue);

            if (result == ResultCode.Success)
            {
                context.ResponseData.Write(rtcValue);
            }

            return result;
        }

        public static ResultCode GetExternalRtcValue(out ulong rtcValue)
        {
            // TODO: emulate MAX77620/MAX77812 RTC
            DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            rtcValue = (ulong)(DateTime.Now.ToUniversalTime() - unixEpoch).TotalSeconds;

            return ResultCode.Success;
        }
    }
}
