using Ryujinx.Core.OsHle.Handles;
using Ryujinx.Core.OsHle.Ipc;
using System.Collections.Generic;

using static Ryujinx.Core.OsHle.ErrorCode;

namespace Ryujinx.Core.OsHle.Services.Am
{
    class ICommonStateGetter : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public ICommonStateGetter()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0, GetEventHandle       },
                { 1, ReceiveMessage       },
                { 5, GetOperationMode     },
                { 6, GetPerformanceMode   },
                { 9, GetCurrentFocusState }
            };
        }

        public long GetEventHandle(ServiceCtx Context)
        {
            KEvent Event = Context.Process.AppletState.MessageEvent;

            int Handle = Context.Process.HandleTable.OpenHandle(Event);

            Context.Response.HandleDesc = IpcHandleDesc.MakeCopy(Handle);

            return 0;
        }

        public long ReceiveMessage(ServiceCtx Context)
        {
            if (!Context.Process.AppletState.TryDequeueMessage(out MessageInfo Message))
            {
                return MakeError(ErrorModule.Am, AmErr.NoMessages);
            }

            Context.ResponseData.Write((int)Message);

            return 0;
        }

        public long GetOperationMode(ServiceCtx Context)
        {
            Context.ResponseData.Write((byte)OperationMode.Handheld);

            return 0;
        }

        public long GetPerformanceMode(ServiceCtx Context)
        {
            Context.ResponseData.Write((byte)0);

            return 0;
        }

        public long GetCurrentFocusState(ServiceCtx Context)
        {
            Context.ResponseData.Write((byte)Context.Process.AppletState.FocusState);

            return 0;
        }
    }
}