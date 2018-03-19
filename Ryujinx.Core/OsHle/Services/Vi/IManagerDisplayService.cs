using Ryujinx.Core.OsHle.Ipc;
using System.Collections.Generic;

namespace Ryujinx.Core.OsHle.IpcServices.Vi
{
    class IManagerDisplayService : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public IManagerDisplayService()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 2010, CreateManagedLayer  },
                { 2011, DestroyManagedLayer },
                { 6000, AddToLayerStack     }
            };
        }

        public static long CreateManagedLayer(ServiceCtx Context)
        {
            Context.ResponseData.Write(0L); //LayerId

            return 0;
        }

        public long DestroyManagedLayer(ServiceCtx Context)
        {
            return 0;
        }

        public static long AddToLayerStack(ServiceCtx Context)
        {
            return 0;
        }
    }
}