namespace Ryujinx.HLE.HOS.Services.Am
{
    class IApplicationProxy : IpcService
    {
        public IApplicationProxy() { }

        [Command(0)]
        // GetCommonStateGetter() -> object<nn::am::service::ICommonStateGetter>
        public long GetCommonStateGetter(ServiceCtx context)
        {
            MakeObject(context, new ICommonStateGetter(context.Device.System));

            return 0;
        }

        [Command(1)]
        // GetSelfController() -> object<nn::am::service::ISelfController>
        public long GetSelfController(ServiceCtx context)
        {
            MakeObject(context, new ISelfController(context.Device.System));

            return 0;
        }

        [Command(2)]
        // GetWindowController() -> object<nn::am::service::IWindowController>
        public long GetWindowController(ServiceCtx context)
        {
            MakeObject(context, new IWindowController());

            return 0;
        }

        [Command(3)]
        // GetAudioController() -> object<nn::am::service::IAudioController>
        public long GetAudioController(ServiceCtx context)
        {
            MakeObject(context, new IAudioController());

            return 0;
        }

        [Command(4)]
        // GetDisplayController() -> object<nn::am::service::IDisplayController>
        public long GetDisplayController(ServiceCtx context)
        {
            MakeObject(context, new IDisplayController());

            return 0;
        }

        [Command(11)]
        // GetLibraryAppletCreator() -> object<nn::am::service::ILibraryAppletCreator>
        public long GetLibraryAppletCreator(ServiceCtx context)
        {
            MakeObject(context, new ILibraryAppletCreator());

            return 0;
        }

        [Command(20)]
        // GetApplicationFunctions() -> object<nn::am::service::IApplicationFunctions>
        public long GetApplicationFunctions(ServiceCtx context)
        {
            MakeObject(context, new IApplicationFunctions());

            return 0;
        }

        [Command(1000)]
        // GetDebugFunctions() -> object<nn::am::service::IDebugFunctions>
        public long GetDebugFunctions(ServiceCtx context)
        {
            MakeObject(context, new IDebugFunctions());

            return 0;
        }
    }
}