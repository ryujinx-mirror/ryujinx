using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Psm
{
    class IPsmServer : IpcService
    {
        enum ChargerType : int
        {
            None,
            ChargerOrDock,
            UsbC
        }

        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public IPsmServer()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0, GetBatteryChargePercentage },
                { 1, GetChargerType             },
                { 7, OpenSession                }
            };
        }

        // GetBatteryChargePercentage() -> u32
        public static long GetBatteryChargePercentage(ServiceCtx Context)
        {
            int ChargePercentage = 100;

            Context.ResponseData.Write(ChargePercentage);

            Logger.PrintStub(LogClass.ServicePsm, $"Stubbed. ChargePercentage: {ChargePercentage}");

            return 0;
        }

        // GetChargerType() -> u32
        public static long GetChargerType(ServiceCtx Context)
        {
            Context.ResponseData.Write((int)ChargerType.ChargerOrDock);

            Logger.PrintStub(LogClass.ServicePsm, $"Stubbed. ChargerType: {ChargerType.ChargerOrDock}");

            return 0;
        }

        // OpenSession() -> IPsmSession
        public long OpenSession(ServiceCtx Context)
        {
            MakeObject(Context, new IPsmSession(Context.Device.System));

            return 0;
        }
    }
}