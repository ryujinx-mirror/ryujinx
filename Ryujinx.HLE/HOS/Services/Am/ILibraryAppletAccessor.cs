using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel;
using Ryujinx.HLE.Logging;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Am
{
    class ILibraryAppletAccessor : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        private KEvent StateChangedEvent;

        public ILibraryAppletAccessor(Horizon System)
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0,   GetAppletStateChangedEvent },
                { 10,  Start                      },
                { 30,  GetResult                  },
                { 100, PushInData                 },
                { 101, PopOutData                 }
            };

            StateChangedEvent = new KEvent(System);
        }

        public long GetAppletStateChangedEvent(ServiceCtx Context)
        {
            StateChangedEvent.Signal();

            int Handle = Context.Process.HandleTable.OpenHandle(StateChangedEvent);

            Context.Response.HandleDesc = IpcHandleDesc.MakeCopy(Handle);

            Context.Device.Log.PrintStub(LogClass.ServiceAm, "Stubbed.");

            return 0;
        }

        public long Start(ServiceCtx Context)
        {
            Context.Device.Log.PrintStub(LogClass.ServiceAm, "Stubbed.");

            return 0;
        }

        public long GetResult(ServiceCtx Context)
        {
            Context.Device.Log.PrintStub(LogClass.ServiceAm, "Stubbed.");

            return 0;
        }

        public long PushInData(ServiceCtx Context)
        {
            Context.Device.Log.PrintStub(LogClass.ServiceAm, "Stubbed.");

            return 0;
        }

        public long PopOutData(ServiceCtx Context)
        {
            MakeObject(Context, new IStorage(StorageHelper.MakeLaunchParams()));

            return 0;
        }
    }
}