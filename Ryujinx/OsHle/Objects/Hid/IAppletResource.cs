using Ryujinx.OsHle.Handles;
using Ryujinx.OsHle.Ipc;
using System.Collections.Generic;

namespace Ryujinx.OsHle.Objects.Hid
{
    class IAppletResource : IIpcInterface
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public HSharedMem Handle;

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