using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Services.Ptm.Ts.Types;

namespace Ryujinx.HLE.HOS.Services.Ptm.Ts
{
    [Service("ts")]
    class IMeasurementServer : IpcService
    {
        private const uint DefaultTemperature = 42000u;

        public IMeasurementServer(ServiceCtx context) { }

        [Command(3)]
        // GetTemperatureMilliC(Location location) -> u32
        public ResultCode GetTemperatureMilliC(ServiceCtx context)
        {
            Location location = (Location)context.RequestData.ReadByte();

            Logger.Stub?.PrintStub(LogClass.ServicePtm, new { location });

            context.ResponseData.Write(DefaultTemperature);

            return ResultCode.Success;
        }
    }
}