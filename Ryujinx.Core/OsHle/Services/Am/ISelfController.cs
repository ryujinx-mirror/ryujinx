using Ryujinx.Core.OsHle.Ipc;
using System.Collections.Generic;

namespace Ryujinx.Core.OsHle.Services.Am
{
    class ISelfController : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public ISelfController()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 1,  LockExit                              },
                { 10, SetScreenShotPermission               },
                { 11, SetOperationModeChangedNotification   },
                { 12, SetPerformanceModeChangedNotification },
                { 13, SetFocusHandlingMode                  },
                { 14, SetRestartMessageEnabled              },
                { 16, SetOutOfFocusSuspendingEnabled        },
                { 50, SetHandlesRequestToDisplay            }
            };
        }

        public long LockExit(ServiceCtx Context)
        {
            return 0;
        }

        public long SetScreenShotPermission(ServiceCtx Context)
        {
            bool Enable = Context.RequestData.ReadByte() != 0 ? true : false;

            Logging.Stub(LogClass.ServiceAm, $"ScreenShot Allowed = {Enable}");

            return 0;
        }

        public long SetOperationModeChangedNotification(ServiceCtx Context)
        {
            bool Enable = Context.RequestData.ReadByte() != 0 ? true : false;

            Logging.Stub(LogClass.ServiceAm, $"OperationMode Changed = {Enable}");

            return 0;
        }

        public long SetPerformanceModeChangedNotification(ServiceCtx Context)
        {
            bool Enable = Context.RequestData.ReadByte() != 0 ? true : false;

            Logging.Stub(LogClass.ServiceAm, $"PerformanceMode Changed = {Enable}");

            return 0;
        }

        public long SetFocusHandlingMode(ServiceCtx Context)
        {
            bool Flag1 = Context.RequestData.ReadByte() != 0 ? true : false;
            bool Flag2 = Context.RequestData.ReadByte() != 0 ? true : false;
            bool Flag3 = Context.RequestData.ReadByte() != 0 ? true : false;

            Logging.Stub(LogClass.ServiceAm, $"Focus Handling Mode Flags = {{{Flag1}|{Flag2}|{Flag3}}}");

            return 0;
        }

        public long SetRestartMessageEnabled(ServiceCtx Context)
        {
            bool Enable = Context.RequestData.ReadByte() != 0 ? true : false;

            Logging.Stub(LogClass.ServiceAm, $"Restart Message Enabled = {Enable}");

            return 0;
        }

        public long SetOutOfFocusSuspendingEnabled(ServiceCtx Context)
        {
            bool Enable = Context.RequestData.ReadByte() != 0 ? true : false;

            Logging.Stub(LogClass.ServiceAm, $"Out Of Focus Suspending Enabled = {Enable}");

            return 0;
        }

        public long SetHandlesRequestToDisplay(ServiceCtx Context)
        {
            bool Enable = Context.RequestData.ReadByte() != 0 ? true : false;

            Logging.Stub(LogClass.ServiceAm, $"HandlesRequestToDisplay Allowed = {Enable}");

            return 0;
        }
    }
}