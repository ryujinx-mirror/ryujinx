using Ryujinx.Core.OsHle.Ipc;
using System.Collections.Generic;

namespace Ryujinx.Core.OsHle.Services.Set
{
    class ServiceSetSys : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public ServiceSetSys()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 23, GetColorSetId },
                { 24, SetColorSetId }
            };
        }

        public static long GetColorSetId(ServiceCtx Context)
        {
            Context.ResponseData.Write((int)Context.Ns.Settings.ThemeColor);

            return 0;
        }

        public static long SetColorSetId(ServiceCtx Context)
        {            
            return 0;
        }
    }
}