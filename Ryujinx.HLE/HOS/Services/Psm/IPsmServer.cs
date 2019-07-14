using Ryujinx.Common.Logging;

namespace Ryujinx.HLE.HOS.Services.Psm
{
    [Service("psm")]
    class IPsmServer : IpcService
    {
        enum ChargerType
        {
            None,
            ChargerOrDock,
            UsbC
        }

        public IPsmServer(ServiceCtx context) { }

        [Command(0)]
        // GetBatteryChargePercentage() -> u32
        public static ResultCode GetBatteryChargePercentage(ServiceCtx context)
        {
            int chargePercentage = 100;

            context.ResponseData.Write(chargePercentage);

            Logger.PrintStub(LogClass.ServicePsm, new { chargePercentage });

            return ResultCode.Success;
        }

        [Command(1)]
        // GetChargerType() -> u32
        public static ResultCode GetChargerType(ServiceCtx context)
        {
            ChargerType chargerType = ChargerType.ChargerOrDock;

            context.ResponseData.Write((int)chargerType);

            Logger.PrintStub(LogClass.ServicePsm, new { chargerType });

            return ResultCode.Success;
        }

        [Command(7)]
        // OpenSession() -> IPsmSession
        public ResultCode OpenSession(ServiceCtx context)
        {
            MakeObject(context, new IPsmSession(context.Device.System));

            return ResultCode.Success;
        }
    }
}