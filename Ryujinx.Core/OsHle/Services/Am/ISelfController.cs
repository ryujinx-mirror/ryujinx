using Ryujinx.Core.Logging;
using Ryujinx.Core.OsHle.Handles;
using Ryujinx.Core.OsHle.Ipc;
using System.Collections.Generic;

namespace Ryujinx.Core.OsHle.Services.Am
{
    class ISelfController : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        private KEvent LaunchableEvent;

        public ISelfController()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 1,  LockExit                              },
                { 9,  GetLibraryAppletLaunchableEvent       },
                { 10, SetScreenShotPermission               },
                { 11, SetOperationModeChangedNotification   },
                { 12, SetPerformanceModeChangedNotification },
                { 13, SetFocusHandlingMode                  },
                { 14, SetRestartMessageEnabled              },
                { 16, SetOutOfFocusSuspendingEnabled        },
                { 50, SetHandlesRequestToDisplay            }
            };

            LaunchableEvent = new KEvent();
        }

        public long LockExit(ServiceCtx Context)
        {
            return 0;
        }

        public long GetLibraryAppletLaunchableEvent(ServiceCtx Context)
        {
            LaunchableEvent.WaitEvent.Set();

            int Handle = Context.Process.HandleTable.OpenHandle(LaunchableEvent);

            Context.Response.HandleDesc = IpcHandleDesc.MakeCopy(Handle);

            Context.Ns.Log.PrintStub(LogClass.ServiceAm, "Stubbed.");

            return 0;
        }

        public long SetScreenShotPermission(ServiceCtx Context)
        {
            bool Enable = Context.RequestData.ReadByte() != 0 ? true : false;

            Context.Ns.Log.PrintStub(LogClass.ServiceAm, "Stubbed.");

            return 0;
        }

        public long SetOperationModeChangedNotification(ServiceCtx Context)
        {
            bool Enable = Context.RequestData.ReadByte() != 0 ? true : false;

            Context.Ns.Log.PrintStub(LogClass.ServiceAm, "Stubbed.");

            return 0;
        }

        public long SetPerformanceModeChangedNotification(ServiceCtx Context)
        {
            bool Enable = Context.RequestData.ReadByte() != 0 ? true : false;

            Context.Ns.Log.PrintStub(LogClass.ServiceAm, "Stubbed.");

            return 0;
        }

        public long SetFocusHandlingMode(ServiceCtx Context)
        {
            bool Flag1 = Context.RequestData.ReadByte() != 0 ? true : false;
            bool Flag2 = Context.RequestData.ReadByte() != 0 ? true : false;
            bool Flag3 = Context.RequestData.ReadByte() != 0 ? true : false;

            Context.Ns.Log.PrintStub(LogClass.ServiceAm, "Stubbed.");

            return 0;
        }

        public long SetRestartMessageEnabled(ServiceCtx Context)
        {
            bool Enable = Context.RequestData.ReadByte() != 0 ? true : false;

            Context.Ns.Log.PrintStub(LogClass.ServiceAm, "Stubbed.");

            return 0;
        }

        public long SetOutOfFocusSuspendingEnabled(ServiceCtx Context)
        {
            bool Enable = Context.RequestData.ReadByte() != 0 ? true : false;

            Context.Ns.Log.PrintStub(LogClass.ServiceAm, "Stubbed.");

            return 0;
        }

        public long SetHandlesRequestToDisplay(ServiceCtx Context)
        {
            bool Enable = Context.RequestData.ReadByte() != 0 ? true : false;

            Context.Ns.Log.PrintStub(LogClass.ServiceAm, "Stubbed.");

            return 0;
        }
    }
}