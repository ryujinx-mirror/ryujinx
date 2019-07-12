namespace Ryujinx.HLE.HOS.Services.Hid
{
    class IActiveApplicationDeviceList : IpcService
    {
        public IActiveApplicationDeviceList() { }

        [Command(0)]
        // ActivateVibrationDevice(nn::hid::VibrationDeviceHandle)
        public long ActivateVibrationDevice(ServiceCtx context)
        {
            int vibrationDeviceHandle = context.RequestData.ReadInt32();

            return 0;
        }
    }
}