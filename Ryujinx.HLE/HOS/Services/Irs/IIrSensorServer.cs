using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Irs
{
    class IIrSensorServer : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        private bool _activated;

        public IIrSensorServer()
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 302, ActivateIrsensor   },
                { 303, DeactivateIrsensor }
            };
        }

        // ActivateIrsensor(nn::applet::AppletResourceUserId, pid)
        public long ActivateIrsensor(ServiceCtx context)
        {
            long appletResourceUserId = context.RequestData.ReadInt64();

            Logger.PrintStub(LogClass.ServiceIrs, new { appletResourceUserId });

            return 0;
        }

        // DeactivateIrsensor(nn::applet::AppletResourceUserId, pid)
        public long DeactivateIrsensor(ServiceCtx context)
        {
            long appletResourceUserId = context.RequestData.ReadInt64();

            Logger.PrintStub(LogClass.ServiceIrs, new { appletResourceUserId });

            return 0;
        }
    }
}