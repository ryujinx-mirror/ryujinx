using Ryujinx.Core.OsHle.Ipc;
using System.Collections.Generic;

namespace Ryujinx.Core.OsHle.Services.Pl
{
    class ServicePl : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public ServicePl()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0, RequestLoad                  },
                { 1, GetLoadState                 },
                { 2, GetFontSize                  },
                { 3, GetSharedMemoryAddressOffset },
                { 4, GetSharedMemoryNativeHandle  }
            };
        }

        public long RequestLoad(ServiceCtx Context)
        {
            SharedFontType FontType = (SharedFontType)Context.RequestData.ReadInt32();

            return 0;
        }

        public long GetLoadState(ServiceCtx Context)
        {
            Context.ResponseData.Write(1); //Loaded

            return 0;
        }

        public long GetFontSize(ServiceCtx Context)
        {
            Context.ResponseData.Write(Horizon.FontSize);

            return 0;
        }

        public long GetSharedMemoryAddressOffset(ServiceCtx Context)
        {
            Context.ResponseData.Write(0);

            return 0;
        }

        public long GetSharedMemoryNativeHandle(ServiceCtx Context)
        {
            int Handle = Context.Process.HandleTable.OpenHandle(Context.Ns.Os.FontSharedMem);

            Context.Response.HandleDesc = IpcHandleDesc.MakeCopy(Handle);

            return 0;
        }
    }
}