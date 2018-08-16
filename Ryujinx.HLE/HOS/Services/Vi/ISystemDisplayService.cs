using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.Logging;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Vi
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
                { 2207, SetLayerVisibility },
                { 3200, GetDisplayMode }
            };
        }

        public static long SetLayerZ(ServiceCtx Context)
        {
            Context.Device.Log.PrintStub(LogClass.ServiceVi, "Stubbed.");

            return 0;
        }

        public static long SetLayerVisibility(ServiceCtx Context)
        {
            Context.Device.Log.PrintStub(LogClass.ServiceVi, "Stubbed.");

            return 0;
        }

        public static long GetDisplayMode(ServiceCtx Context)
        {
            //TODO: De-hardcode resolution.
            Context.ResponseData.Write(1280);
            Context.ResponseData.Write(720);
            Context.ResponseData.Write(60.0f);
            Context.ResponseData.Write(0);

            return 0;
        }
    }
}