using Ryujinx.HLE.Logging;
using Ryujinx.HLE.OsHle.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.OsHle.Services.Vi
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
            Context.Ns.Log.PrintStub(LogClass.ServiceVi, "Stubbed.");
            return 0;
        }

        public static long SetLayerVisibility(ServiceCtx Context)
        {
            Context.Ns.Log.PrintStub(LogClass.ServiceVi, "Stubbed.");
            return 0;
        }

        public static long GetDisplayMode(ServiceCtx Context)
        {
            Context.ResponseData.Write(1280);
            Context.ResponseData.Write(720);
            Context.ResponseData.Write(60.0f);
            Context.ResponseData.Write(0);
            return 0;
        }
    }
}