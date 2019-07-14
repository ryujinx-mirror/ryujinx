namespace Ryujinx.HLE.HOS.Services.Pctl
{
    [Service("pctl")]
    [Service("pctl:a")]
    [Service("pctl:r")]
    [Service("pctl:s")]
    class IParentalControlServiceFactory : IpcService
    {
        public IParentalControlServiceFactory(ServiceCtx context) { }

        [Command(0)]
        // CreateService(u64, pid) -> object<nn::pctl::detail::ipc::IParentalControlService>
        public ResultCode CreateService(ServiceCtx context)
        {
            MakeObject(context, new IParentalControlService());

            return ResultCode.Success;
        }

        [Command(1)] // 4.0.0+
        // CreateServiceWithoutInitialize(u64, pid) -> object<nn::pctl::detail::ipc::IParentalControlService>
        public ResultCode CreateServiceWithoutInitialize(ServiceCtx context)
        {
            MakeObject(context, new IParentalControlService(false));

            return ResultCode.Success;
        }
    }
}