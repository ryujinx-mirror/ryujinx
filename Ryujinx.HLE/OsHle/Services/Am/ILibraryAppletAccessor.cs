using Ryujinx.HLE.Logging;
using Ryujinx.HLE.OsHle.Handles;
using Ryujinx.HLE.OsHle.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.OsHle.Services.Am
{
    class ILibraryAppletAccessor : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        private KEvent StateChangedEvent;

        public ILibraryAppletAccessor()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0,   GetAppletStateChangedEvent },
                { 10,  Start                      },
                { 30,  GetResult                  },
                { 100, PushInData                 },
                { 101, PopOutData                 }
            };

            StateChangedEvent = new KEvent();
        }

        public long GetAppletStateChangedEvent(ServiceCtx Context)
        {
            StateChangedEvent.WaitEvent.Set();

            int Handle = Context.Process.HandleTable.OpenHandle(StateChangedEvent);

            Context.Response.HandleDesc = IpcHandleDesc.MakeCopy(Handle);

            Context.Ns.Log.PrintStub(LogClass.ServiceAm, "Stubbed.");

            return 0;
        }

        public long Start(ServiceCtx Context)
        {
            Context.Ns.Log.PrintStub(LogClass.ServiceAm, "Stubbed.");

            return 0;
        }

        public long GetResult(ServiceCtx Context)
        {
            Context.Ns.Log.PrintStub(LogClass.ServiceAm, "Stubbed.");

            return 0;
        }

        public long PushInData(ServiceCtx Context)
        {
            Context.Ns.Log.PrintStub(LogClass.ServiceAm, "Stubbed.");

            return 0;
        }

        public long PopOutData(ServiceCtx Context)
        {
            MakeObject(Context, new IStorage(StorageHelper.MakeLaunchParams()));

            return 0;
        }
    }
}