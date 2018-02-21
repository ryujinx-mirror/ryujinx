using Ryujinx.Core.OsHle.Handles;
using Ryujinx.Core.OsHle.Ipc;
using System.Collections.Generic;

namespace Ryujinx.Core.OsHle.Objects.Hid
{
    class IAppletResource : IIpcInterface
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        private HSharedMem Handle;

        public IAppletResource(HSharedMem Handle)
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0, GetSharedMemoryHandle }
            };

            this.Handle = Handle;
        }

        public static long GetSharedMemoryHandle(ServiceCtx Context)
        {
            Context.Response.HandleDesc = IpcHandleDesc.MakeCopy(Context.Ns.Os.HidHandle);

            return 0;
        }
    }
}