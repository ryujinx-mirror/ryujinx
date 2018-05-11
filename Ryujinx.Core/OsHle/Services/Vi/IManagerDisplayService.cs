using Ryujinx.Core.OsHle.Ipc;
using System.Collections.Generic;
using Ryujinx.Core.Logging;

namespace Ryujinx.Core.OsHle.Services.Vi
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
                { 6000, AddToLayerStack     },
                { 6002, SetLayerVisibility  }
            };
        }

        public static long CreateManagedLayer(ServiceCtx Context)
        {
            Context.Ns.Log.PrintStub(LogClass.ServiceVi, "Stubbed.");
            Context.ResponseData.Write(0L); //LayerId
            return 0;
        }

        public long DestroyManagedLayer(ServiceCtx Context)
        {
            Context.Ns.Log.PrintStub(LogClass.ServiceVi, "Stubbed.");
            return 0;
        }

        public static long AddToLayerStack(ServiceCtx Context)
        {
            Context.Ns.Log.PrintStub(LogClass.ServiceVi, "Stubbed.");
            return 0;
        }

        public static long SetLayerVisibility(ServiceCtx Context)
        {
            Context.Ns.Log.PrintStub(LogClass.ServiceVi, "Stubbed.");
            return 0;
        }
    }
}