namespace Ryujinx.HLE.HOS.Services.Vi
{
    [Service("vi:m")]
    class IManagerRootService : IpcService
    {
        public IManagerRootService(ServiceCtx context) { }

        [Command(2)]
        // GetDisplayService(u32) -> object<nn::visrv::sf::IApplicationDisplayService>
        public long GetDisplayService(ServiceCtx context)
        {
            int serviceType = context.RequestData.ReadInt32();

            MakeObject(context, new IApplicationDisplayService());

            return 0;
        }
    }
}