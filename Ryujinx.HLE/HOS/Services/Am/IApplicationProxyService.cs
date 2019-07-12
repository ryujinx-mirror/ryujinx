namespace Ryujinx.HLE.HOS.Services.Am
{
    [Service("appletOE")]
    class IApplicationProxyService : IpcService
    {
        public IApplicationProxyService(ServiceCtx context) { }

        [Command(0)]
        // OpenApplicationProxy(u64, pid, handle<copy>) -> object<nn::am::service::IApplicationProxy>
        public long OpenApplicationProxy(ServiceCtx context)
        {
            MakeObject(context, new IApplicationProxy());

            return 0;
        }
    }
}