using Ryujinx.Core.OsHle.Ipc;
using System.Collections.Generic;

using static Ryujinx.Core.OsHle.IpcServices.ObjHelper;

namespace Ryujinx.Core.OsHle.IpcServices.Vi
{
    class ServiceVi : IIpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public ServiceVi()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 2, GetDisplayService }
            };
        }

        public long GetDisplayService(ServiceCtx Context)
        {
            int Unknown = Context.RequestData.ReadInt32();

            MakeObject(Context, new IApplicationDisplayService());

            return 0;
        }
    }
}