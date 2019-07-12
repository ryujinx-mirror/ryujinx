namespace Ryujinx.HLE.HOS.Services.Am
{
    [Service("appletAE")]
    class IAllSystemAppletProxiesService : IpcService
    {
        public IAllSystemAppletProxiesService(ServiceCtx context) { }

        [Command(100)]
        // OpenSystemAppletProxy(u64, pid, handle<copy>) -> object<nn::am::service::ISystemAppletProxy>
        public long OpenSystemAppletProxy(ServiceCtx context)
        {
            MakeObject(context, new ISystemAppletProxy());

            return 0;
        }
    }
}