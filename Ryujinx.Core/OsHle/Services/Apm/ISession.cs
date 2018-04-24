using Ryujinx.Core.Logging;
using Ryujinx.Core.OsHle.Ipc;
using System.Collections.Generic;

namespace Ryujinx.Core.OsHle.Services.Apm
{
    class ISession : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public ISession()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0, SetPerformanceConfiguration },
                { 1, GetPerformanceConfiguration }
            };
        }

        public long SetPerformanceConfiguration(ServiceCtx Context)
        {
            PerformanceMode          PerfMode   = (PerformanceMode)Context.RequestData.ReadInt32();
            PerformanceConfiguration PerfConfig = (PerformanceConfiguration)Context.RequestData.ReadInt32();

            return 0;
        }

        public long GetPerformanceConfiguration(ServiceCtx Context)
        {
            PerformanceMode PerfMode = (PerformanceMode)Context.RequestData.ReadInt32();

            Context.ResponseData.Write((uint)PerformanceConfiguration.PerformanceConfiguration1);

            Context.Ns.Log.PrintStub(LogClass.ServiceApm, "Stubbed.");

            return 0;
        }
    }
}