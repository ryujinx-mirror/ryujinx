namespace Ryujinx.HLE.HOS.Services.Hid.HidServer
{
    class IActiveApplicationDeviceList : IpcService
    {
        public IActiveApplicationDeviceList() { }

        [CommandCmif(0)]
        // ActivateVibrationDevice(nn::hid::VibrationDeviceHandle)
        public ResultCode ActivateVibrationDevice(ServiceCtx context)
        {
#pragma warning disable IDE0059 // Remove unnecessary value assignment
            int vibrationDeviceHandle = context.RequestData.ReadInt32();
#pragma warning restore IDE0059

            return ResultCode.Success;
        }
    }
}
