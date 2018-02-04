using Ryujinx.OsHle.Handles;
using Ryujinx.OsHle.Ipc;

namespace Ryujinx.OsHle.Objects
{
    class HidIAppletResource
    {
        public HSharedMem Handle;

        public HidIAppletResource(HSharedMem Handle)
        {
            this.Handle = Handle;
        }

        public static long GetSharedMemoryHandle(ServiceCtx Context)
        {
            Context.Response.HandleDesc = IpcHandleDesc.MakeCopy(Context.Ns.Os.HidHandle);

            return 0;
        }
    }
}