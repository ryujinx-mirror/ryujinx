namespace Ryujinx.HLE.HOS.Services.Hid.HidServer
{
    class IActiveApplicationDeviceList : IpcService
    {
        public IActiveApplicationDeviceList() { }

        [CommandHipc(0)]
        // ActivateVibrationDevice(nn::hid::VibrationDeviceHandle)
        public ResultCode ActivateVibrationDevice(ServiceCtx context)
        {
            int vibrationDeviceHandle = context.RequestData.ReadInt32();

            return ResultCode.Success;
        }
    }
}