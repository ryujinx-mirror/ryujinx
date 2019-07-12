namespace Ryujinx.HLE.HOS.Services.Vi
{
    [Service("vi:u")]
    class IApplicationRootService : IpcService
    {
        public IApplicationRootService(ServiceCtx context) { }

        [Command(0)]
        // GetDisplayService(u32) -> object<nn::visrv::sf::IApplicationDisplayService>
        public long GetDisplayService(ServiceCtx context)
        {
            int serviceType = context.RequestData.ReadInt32();

            MakeObject(context, new IApplicationDisplayService());

            return 0;
        }
    }
}