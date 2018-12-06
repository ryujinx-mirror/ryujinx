using Ryujinx.HLE.HOS.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Am
{
    class IApplicationProxy : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        public IApplicationProxy()
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 0,    GetCommonStateGetter    },
                { 1,    GetSelfController       },
                { 2,    GetWindowController     },
                { 3,    GetAudioController      },
                { 4,    GetDisplayController    },
                { 11,   GetLibraryAppletCreator },
                { 20,   GetApplicationFunctions },
                { 1000, GetDebugFunctions       }
            };
        }

        public long GetCommonStateGetter(ServiceCtx context)
        {
            MakeObject(context, new ICommonStateGetter(context.Device.System));

            return 0;
        }

        public long GetSelfController(ServiceCtx context)
        {
            MakeObject(context, new ISelfController(context.Device.System));

            return 0;
        }

        public long GetWindowController(ServiceCtx context)
        {
            MakeObject(context, new IWindowController());

            return 0;
        }

        public long GetAudioController(ServiceCtx context)
        {
            MakeObject(context, new IAudioController());

            return 0;
        }

        public long GetDisplayController(ServiceCtx context)
        {
            MakeObject(context, new IDisplayController());

            return 0;
        }

        public long GetLibraryAppletCreator(ServiceCtx context)
        {
            MakeObject(context, new ILibraryAppletCreator());

            return 0;
        }

        public long GetApplicationFunctions(ServiceCtx context)
        {
            MakeObject(context, new IApplicationFunctions());

            return 0;
        }

        public long GetDebugFunctions(ServiceCtx context)
        {
            MakeObject(context, new IDebugFunctions());

            return 0;
        }
    }
}