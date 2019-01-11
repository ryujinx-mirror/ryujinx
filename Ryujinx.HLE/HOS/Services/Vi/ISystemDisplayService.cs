using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Vi
{
    class ISystemDisplayService : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        public ISystemDisplayService(IApplicationDisplayService applicationDisplayService)
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 2205, SetLayerZ                                  },
                { 2207, SetLayerVisibility                         },
                { 2312, applicationDisplayService.CreateStrayLayer },
                { 3200, GetDisplayMode                             }
            };
        }

        public static long SetLayerZ(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceVi);

            return 0;
        }

        public static long SetLayerVisibility(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceVi);

            return 0;
        }

        public static long GetDisplayMode(ServiceCtx context)
        {
            //TODO: De-hardcode resolution.
            context.ResponseData.Write(1280);
            context.ResponseData.Write(720);
            context.ResponseData.Write(60.0f);
            context.ResponseData.Write(0);

            return 0;
        }
    }
}