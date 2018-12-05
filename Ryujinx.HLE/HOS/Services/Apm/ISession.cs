using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Apm
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

            Logger.PrintStub(LogClass.ServiceApm, "Stubbed.");

            return 0;
        }
    }
}