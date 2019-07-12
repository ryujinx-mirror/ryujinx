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
        public long CreateService(ServiceCtx context)
        {
            MakeObject(context, new IParentalControlService());

            return 0;
        }

        [Command(1)] // 4.0.0+
        // CreateServiceWithoutInitialize(u64, pid) -> object<nn::pctl::detail::ipc::IParentalControlService>
        public long CreateServiceWithoutInitialize(ServiceCtx context)
        {
            MakeObject(context, new IParentalControlService(false));

            return 0;
        }
    }
}