using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Vi
{
    class IManagerDisplayService : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        public IManagerDisplayService()
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 2010, CreateManagedLayer  },
                { 2011, DestroyManagedLayer },
                { 6000, AddToLayerStack     },
                { 6002, SetLayerVisibility  }
            };
        }

        public static long CreateManagedLayer(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceVi);

            context.ResponseData.Write(0L); //LayerId

            return 0;
        }

        public long DestroyManagedLayer(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceVi);

            return 0;
        }

        public static long AddToLayerStack(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceVi);

            return 0;
        }

        public static long SetLayerVisibility(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceVi);

            return 0;
        }
    }
}