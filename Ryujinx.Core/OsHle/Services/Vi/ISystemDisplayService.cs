using Ryujinx.Core.OsHle.Ipc;
using System.Collections.Generic;

namespace Ryujinx.Core.OsHle.Services.Vi
{
    class ISystemDisplayService : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public ISystemDisplayService()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 2205, SetLayerZ },
                { 2207, SetLayerVisibility }
            };
        }

        public static long SetLayerZ(ServiceCtx Context)
        {
            return 0;
        }

        public static long SetLayerVisibility(ServiceCtx Context)
        {
            return 0;
        }
    }
}