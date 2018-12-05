using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Vi
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
            Logger.PrintStub(LogClass.ServiceVi, "Stubbed.");

            Context.ResponseData.Write(0L); //LayerId

            return 0;
        }

        public long DestroyManagedLayer(ServiceCtx Context)
        {
            Logger.PrintStub(LogClass.ServiceVi, "Stubbed.");

            return 0;
        }

        public static long AddToLayerStack(ServiceCtx Context)
        {
            Logger.PrintStub(LogClass.ServiceVi, "Stubbed.");

            return 0;
        }

        public static long SetLayerVisibility(ServiceCtx Context)
        {
            Logger.PrintStub(LogClass.ServiceVi, "Stubbed.");

            return 0;
        }
    }
}