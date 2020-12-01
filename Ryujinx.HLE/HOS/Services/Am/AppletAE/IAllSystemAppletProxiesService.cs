using Ryujinx.HLE.HOS.Services.Am.AppletAE.AllSystemAppletProxiesService;

namespace Ryujinx.HLE.HOS.Services.Am.AppletAE
{
    [Service("appletAE")]
    class IAllSystemAppletProxiesService : IpcService
    {
        public IAllSystemAppletProxiesService(ServiceCtx context) { }

        [Command(100)]
        // OpenSystemAppletProxy(u64, pid, handle<copy>) -> object<nn::am::service::ISystemAppletProxy>
        public ResultCode OpenSystemAppletProxy(ServiceCtx context)
        {
            MakeObject(context, new ISystemAppletProxy(context.Request.HandleDesc.PId));

            return ResultCode.Success;
        }
    }
}