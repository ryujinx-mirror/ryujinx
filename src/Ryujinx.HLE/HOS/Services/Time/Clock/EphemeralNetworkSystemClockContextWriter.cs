namespace Ryujinx.HLE.HOS.Services.Time.Clock
{
    class EphemeralNetworkSystemClockContextWriter : SystemClockContextUpdateCallback
    {
        protected override ResultCode Update()
        {
            return ResultCode.Success;
        }
    }
}
