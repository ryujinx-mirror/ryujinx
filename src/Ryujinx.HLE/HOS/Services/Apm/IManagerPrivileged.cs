namespace Ryujinx.HLE.HOS.Services.Apm
{
    // NOTE: This service doesn’t exist anymore after firmware 7.0.1. But some outdated homebrew still uses it.

    [Service("apm:p")] // 1.0.0-7.0.1
    class IManagerPrivileged : IpcService
    {
        public IManagerPrivileged(ServiceCtx context) { }

        [CommandCmif(0)]
        // OpenSession() -> object<nn::apm::ISession>
        public ResultCode OpenSession(ServiceCtx context)
        {
            MakeObject(context, new SessionServer(context));

            return ResultCode.Success;
        }
    }
}
