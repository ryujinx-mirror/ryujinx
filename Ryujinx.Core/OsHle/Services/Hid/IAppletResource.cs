using Ryujinx.Core.OsHle.Handles;
using Ryujinx.Core.OsHle.Ipc;
using System.Collections.Generic;

namespace Ryujinx.Core.OsHle.Services.Hid
{
    class IAppletResource : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        private HSharedMem HidSharedMem;

        public IAppletResource(HSharedMem HidSharedMem)
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0, GetSharedMemoryHandle }
            };

            this.HidSharedMem = HidSharedMem;
        }

        public long GetSharedMemoryHandle(ServiceCtx Context)
        {
            int Handle = Context.Process.HandleTable.OpenHandle(HidSharedMem);

            Context.Response.HandleDesc = IpcHandleDesc.MakeCopy(Handle);

            return 0;
        }
    }
}