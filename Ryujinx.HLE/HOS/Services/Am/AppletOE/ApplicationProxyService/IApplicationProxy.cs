using Ryujinx.HLE.HOS.Services.Am.AppletAE.AllSystemAppletProxiesService.SystemAppletProxy;
using Ryujinx.HLE.HOS.Services.Am.AppletOE.ApplicationProxyService.ApplicationProxy;

namespace Ryujinx.HLE.HOS.Services.Am.AppletOE.ApplicationProxyService
{
    class IApplicationProxy : IpcService
    {
        private readonly long _pid;

        public IApplicationProxy(long pid)
        {
            _pid = pid;
        }

        [Command(0)]
        // GetCommonStateGetter() -> object<nn::am::service::ICommonStateGetter>
        public ResultCode GetCommonStateGetter(ServiceCtx context)
        {
            MakeObject(context, new ICommonStateGetter(context));

            return ResultCode.Success;
        }

        [Command(1)]
        // GetSelfController() -> object<nn::am::service::ISelfController>
        public ResultCode GetSelfController(ServiceCtx context)
        {
            MakeObject(context, new ISelfController(context.Device.System, _pid));

            return ResultCode.Success;
        }

        [Command(2)]
        // GetWindowController() -> object<nn::am::service::IWindowController>
        public ResultCode GetWindowController(ServiceCtx context)
        {
            MakeObject(context, new IWindowController(_pid));

            return ResultCode.Success;
        }

        [Command(3)]
        // GetAudioController() -> object<nn::am::service::IAudioController>
        public ResultCode GetAudioController(ServiceCtx context)
        {
            MakeObject(context, new IAudioController());

            return ResultCode.Success;
        }

        [Command(4)]
        // GetDisplayController() -> object<nn::am::service::IDisplayController>
        public ResultCode GetDisplayController(ServiceCtx context)
        {
            MakeObject(context, new IDisplayController());

            return ResultCode.Success;
        }

        [Command(11)]
        // GetLibraryAppletCreator() -> object<nn::am::service::ILibraryAppletCreator>
        public ResultCode GetLibraryAppletCreator(ServiceCtx context)
        {
            MakeObject(context, new ILibraryAppletCreator());

            return ResultCode.Success;
        }

        [Command(20)]
        // GetApplicationFunctions() -> object<nn::am::service::IApplicationFunctions>
        public ResultCode GetApplicationFunctions(ServiceCtx context)
        {
            MakeObject(context, new IApplicationFunctions(context.Device.System));

            return ResultCode.Success;
        }

        [Command(1000)]
        // GetDebugFunctions() -> object<nn::am::service::IDebugFunctions>
        public ResultCode GetDebugFunctions(ServiceCtx context)
        {
            MakeObject(context, new IDebugFunctions());

            return ResultCode.Success;
        }
    }
}