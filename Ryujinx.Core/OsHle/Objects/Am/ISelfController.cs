using Ryujinx.Core.OsHle.Ipc;
using System.Collections.Generic;

namespace Ryujinx.Core.OsHle.Objects.Am
{
    class ISelfController : IIpcInterface
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public ISelfController()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 11, SetOperationModeChangedNotification   },
                { 12, SetPerformanceModeChangedNotification },
                { 13, SetFocusHandlingMode                  },
                { 16, SetOutOfFocusSuspendingEnabled        }
            };
        }

        public long SetOperationModeChangedNotification(ServiceCtx Context)
        {
            bool Enable = Context.RequestData.ReadByte() != 0 ? true : false;

            return 0;
        }

        public long SetPerformanceModeChangedNotification(ServiceCtx Context)
        {
            bool Enable = Context.RequestData.ReadByte() != 0 ? true : false;

            return 0;
        }

        public long SetFocusHandlingMode(ServiceCtx Context)
        {
            bool Flag1 = Context.RequestData.ReadByte() != 0 ? true : false;
            bool Flag2 = Context.RequestData.ReadByte() != 0 ? true : false;
            bool Flag3 = Context.RequestData.ReadByte() != 0 ? true : false;

            return 0;
        }

        public long SetOutOfFocusSuspendingEnabled(ServiceCtx Context)
        {
            bool Enable = Context.RequestData.ReadByte() != 0 ? true : false;

            return 0;
        }
    }
}