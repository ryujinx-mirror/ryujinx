using Ryujinx.OsHle.Ipc;
using System.Collections.Generic;

namespace Ryujinx.OsHle.Objects.Am
{
    class ICommonStateGetter : IIpcInterface
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public ICommonStateGetter()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0, GetEventHandle       },
                { 1, ReceiveMessage       },
                { 5, GetOperationMode     },
                { 6, GetPerformanceMode   },
                { 9, GetCurrentFocusState },
            };
        }

        private enum FocusState
        {
            InFocus    = 1,
            OutOfFocus = 2
        }

        private enum OperationMode
        {
            Handheld = 0,
            Docked   = 1
        }

        public long GetEventHandle(ServiceCtx Context)
        {
            Context.ResponseData.Write(0L);

            return 0;
        }

        public long ReceiveMessage(ServiceCtx Context)
        {
            //Program expects 0xF at 0x17ae70 on puyo sdk,
            //otherwise runs on a infinite loop until it reads said value.
            //What it means is still unknown.
            Context.ResponseData.Write(0xfL);

            return 0; //0x680;
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
            Context.ResponseData.Write((byte)FocusState.InFocus);

            return 0;
        }
    }
}