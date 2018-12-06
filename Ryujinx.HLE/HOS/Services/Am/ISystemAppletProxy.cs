using Ryujinx.HLE.HOS.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Am
{
    class ISystemAppletProxy : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        public ISystemAppletProxy()
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 0,    GetCommonStateGetter     },
                { 1,    GetSelfController        },
                { 2,    GetWindowController      },
                { 3,    GetAudioController       },
                { 4,    GetDisplayController     },
                { 11,   GetLibraryAppletCreator  },
                { 20,   GetHomeMenuFunctions     },
                { 21,   GetGlobalStateController },
                { 22,   GetApplicationCreator    },
                { 1000, GetDebugFunctions        }
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

        public long GetHomeMenuFunctions(ServiceCtx context)
        {
            MakeObject(context, new IHomeMenuFunctions(context.Device.System));

            return 0;
        }

        public long GetGlobalStateController(ServiceCtx context)
        {
            MakeObject(context, new IGlobalStateController());

            return 0;
        }

        public long GetApplicationCreator(ServiceCtx context)
        {
            MakeObject(context, new IApplicationCreator());

            return 0;
        }

        public long GetDebugFunctions(ServiceCtx context)
        {
            MakeObject(context, new IDebugFunctions());

            return 0;
        }
    }
}