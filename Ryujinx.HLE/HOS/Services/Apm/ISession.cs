using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Apm
{
    class ISession : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        public ISession()
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 0, SetPerformanceConfiguration },
                { 1, GetPerformanceConfiguration }
            };
        }

        public long SetPerformanceConfiguration(ServiceCtx context)
        {
            PerformanceMode          perfMode   = (PerformanceMode)context.RequestData.ReadInt32();
            PerformanceConfiguration perfConfig = (PerformanceConfiguration)context.RequestData.ReadInt32();

            return 0;
        }

        public long GetPerformanceConfiguration(ServiceCtx context)
        {
            PerformanceMode perfMode = (PerformanceMode)context.RequestData.ReadInt32();

            context.ResponseData.Write((uint)PerformanceConfiguration.PerformanceConfiguration1);

            Logger.PrintStub(LogClass.ServiceApm, "Stubbed.");

            return 0;
        }
    }
}