using Ryujinx.Core.OsHle.Ipc;
using System.Collections.Generic;

namespace Ryujinx.Core.OsHle.IpcServices.Pl
{
    class ServicePl : IIpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public ServicePl()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 1, GetLoadState                 },
                { 2, GetFontSize                  },
                { 3, GetSharedMemoryAddressOffset },
                { 4, GetSharedMemoryNativeHandle  }
            };
        }

        public static long GetLoadState(ServiceCtx Context)
        {
            Context.ResponseData.Write(1); //Loaded

            return 0;
        }

        public static long GetFontSize(ServiceCtx Context)
        {
            Context.ResponseData.Write(Horizon.FontSize);

            return 0;
        }

        public static long GetSharedMemoryAddressOffset(ServiceCtx Context)
        {
            Context.ResponseData.Write(0);

            return 0;
        }

        public static long GetSharedMemoryNativeHandle(ServiceCtx Context)
        {
            Context.Response.HandleDesc = IpcHandleDesc.MakeCopy(Context.Ns.Os.FontHandle);

            return 0;
        }
    }
}