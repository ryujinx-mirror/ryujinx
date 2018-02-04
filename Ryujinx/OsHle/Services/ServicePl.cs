using Ryujinx.OsHle.Ipc;

namespace Ryujinx.OsHle.Services
{
    static partial class Service
    {
        public static long PlGetLoadState(ServiceCtx Context)
        {
            Context.ResponseData.Write(1); //Loaded

            return 0;
        }

        public static long PlGetFontSize(ServiceCtx Context)
        {
            Context.ResponseData.Write(Horizon.FontSize);

            return 0;
        }

        public static long PlGetSharedMemoryAddressOffset(ServiceCtx Context)
        {
            Context.ResponseData.Write(0);

            return 0;
        }

        public static long PlGetSharedMemoryNativeHandle(ServiceCtx Context)
        {
            Context.Response.HandleDesc = IpcHandleDesc.MakeCopy(Context.Ns.Os.FontHandle);

            return 0;
        }
    }
}