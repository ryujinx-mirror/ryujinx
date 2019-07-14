using Ryujinx.Common.Logging;

namespace Ryujinx.HLE.HOS.Services.Prepo
{
    [Service("prepo:a")]
    [Service("prepo:u")]
    class IPrepoService : IpcService
    {
        public IPrepoService(ServiceCtx context) { }

        [Command(10101)]
        // SaveReportWithUser(nn::account::Uid, u64, pid, buffer<u8, 9>, buffer<bytes, 5>)
        public static ResultCode SaveReportWithUser(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServicePrepo);

            return ResultCode.Success;
        }
    }
}