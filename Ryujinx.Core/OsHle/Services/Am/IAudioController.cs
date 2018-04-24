using Ryujinx.Core.Logging;
using Ryujinx.Core.OsHle.Ipc;
using System.Collections.Generic;

namespace Ryujinx.Core.OsHle.Services.Am
{
    class IAudioController : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public IAudioController()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0, SetExpectedMasterVolume              },
                { 1, GetMainAppletExpectedMasterVolume    },
                { 2, GetLibraryAppletExpectedMasterVolume },
                { 3, ChangeMainAppletMasterVolume         },
                { 4, SetTransparentVolumeRate             }
            };
        }

        public long SetExpectedMasterVolume(ServiceCtx Context)
        {
            float AppletVolume        = Context.RequestData.ReadSingle();
            float LibraryAppletVolume = Context.RequestData.ReadSingle();

            Context.Ns.Log.PrintStub(LogClass.ServiceAm, "Stubbed.");

            return 0;
        }

        public long GetMainAppletExpectedMasterVolume(ServiceCtx Context)
        {
            Context.ResponseData.Write(1f);

            Context.Ns.Log.PrintStub(LogClass.ServiceAm, "Stubbed.");

            return 0;
        }

        public long GetLibraryAppletExpectedMasterVolume(ServiceCtx Context)
        {
            Context.ResponseData.Write(1f);

            Context.Ns.Log.PrintStub(LogClass.ServiceAm, "Stubbed.");

            return 0;
        }

        public long ChangeMainAppletMasterVolume(ServiceCtx Context)
        {
            float Unknown0 = Context.RequestData.ReadSingle();
            long  Unknown1 = Context.RequestData.ReadInt64();

            Context.Ns.Log.PrintStub(LogClass.ServiceAm, "Stubbed.");

            return 0;
        }

        public long SetTransparentVolumeRate(ServiceCtx Context)
        {
            float Unknown0 = Context.RequestData.ReadSingle();

            Context.Ns.Log.PrintStub(LogClass.ServiceAm, "Stubbed.");

            return 0;
        }
    }
}
