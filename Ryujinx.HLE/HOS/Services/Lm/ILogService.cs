namespace Ryujinx.HLE.HOS.Services.Lm
{
    [Service("lm")]
    class ILogService : IpcService
    {
        public ILogService(ServiceCtx context) { }

        [Command(0)]
        // Initialize(u64, pid) -> object<nn::lm::ILogger>
        public long Initialize(ServiceCtx context)
        {
            MakeObject(context, new ILogger());

            return 0;
        }
    }
}