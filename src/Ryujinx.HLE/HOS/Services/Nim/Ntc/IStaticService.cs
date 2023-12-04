using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Services.Nim.Ntc.StaticService;

namespace Ryujinx.HLE.HOS.Services.Nim.Ntc
{
    [Service("ntc")]
    class IStaticService : IpcService
    {
        public IStaticService(ServiceCtx context) { }

        [CommandCmif(0)]
        // OpenEnsureNetworkClockAvailabilityService(u64) -> object<nn::ntc::detail::service::IEnsureNetworkClockAvailabilityService>
        public ResultCode CreateAsyncInterface(ServiceCtx context)
        {
            ulong unknown = context.RequestData.ReadUInt64();

            MakeObject(context, new IEnsureNetworkClockAvailabilityService(context));

            Logger.Stub?.PrintStub(LogClass.ServiceNtc, new { unknown });

            return ResultCode.Success;
        }
    }
}
