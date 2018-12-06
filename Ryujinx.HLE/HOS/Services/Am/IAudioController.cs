using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Am
{
    class IAudioController : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        public IAudioController()
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 0, SetExpectedMasterVolume              },
                { 1, GetMainAppletExpectedMasterVolume    },
                { 2, GetLibraryAppletExpectedMasterVolume },
                { 3, ChangeMainAppletMasterVolume         },
                { 4, SetTransparentVolumeRate             }
            };
        }

        public long SetExpectedMasterVolume(ServiceCtx context)
        {
            float appletVolume        = context.RequestData.ReadSingle();
            float libraryAppletVolume = context.RequestData.ReadSingle();

            Logger.PrintStub(LogClass.ServiceAm, "Stubbed.");

            return 0;
        }

        public long GetMainAppletExpectedMasterVolume(ServiceCtx context)
        {
            context.ResponseData.Write(1f);

            Logger.PrintStub(LogClass.ServiceAm, "Stubbed.");

            return 0;
        }

        public long GetLibraryAppletExpectedMasterVolume(ServiceCtx context)
        {
            context.ResponseData.Write(1f);

            Logger.PrintStub(LogClass.ServiceAm, "Stubbed.");

            return 0;
        }

        public long ChangeMainAppletMasterVolume(ServiceCtx context)
        {
            float unknown0 = context.RequestData.ReadSingle();
            long  unknown1 = context.RequestData.ReadInt64();

            Logger.PrintStub(LogClass.ServiceAm, "Stubbed.");

            return 0;
        }

        public long SetTransparentVolumeRate(ServiceCtx context)
        {
            float unknown0 = context.RequestData.ReadSingle();

            Logger.PrintStub(LogClass.ServiceAm, "Stubbed.");

            return 0;
        }
    }
}
