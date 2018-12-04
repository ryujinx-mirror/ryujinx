using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Psm
{
    class IPsmServer : IpcService
    {
        enum ChargerType
        {
            None,
            ChargerOrDock,
            UsbC
        }

        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        public IPsmServer()
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 0, GetBatteryChargePercentage },
                { 1, GetChargerType             },
                { 7, OpenSession                }
            };
        }

        // GetBatteryChargePercentage() -> u32
        public static long GetBatteryChargePercentage(ServiceCtx context)
        {
            int chargePercentage = 100;

            context.ResponseData.Write(chargePercentage);

            Logger.PrintStub(LogClass.ServicePsm, $"Stubbed. ChargePercentage: {chargePercentage}");

            return 0;
        }

        // GetChargerType() -> u32
        public static long GetChargerType(ServiceCtx context)
        {
            context.ResponseData.Write((int)ChargerType.ChargerOrDock);

            Logger.PrintStub(LogClass.ServicePsm, $"Stubbed. ChargerType: {ChargerType.ChargerOrDock}");

            return 0;
        }

        // OpenSession() -> IPsmSession
        public long OpenSession(ServiceCtx context)
        {
            MakeObject(context, new IPsmSession(context.Device.System));

            return 0;
        }
    }
}