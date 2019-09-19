using Ryujinx.HLE.HOS.Services.Lm.LogService;

namespace Ryujinx.HLE.HOS.Services.Lm
{
    [Service("lm")]
    class ILogService : IpcService
    {
        public ILogService(ServiceCtx context) { }

        [Command(0)]
        // Initialize(u64, pid) -> object<nn::lm::ILogger>
        public ResultCode Initialize(ServiceCtx context)
        {
            MakeObject(context, new ILogger());

            return ResultCode.Success;
        }
    }
}