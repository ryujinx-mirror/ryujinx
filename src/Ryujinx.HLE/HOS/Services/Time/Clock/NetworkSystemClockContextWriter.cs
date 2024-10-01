namespace Ryujinx.HLE.HOS.Services.Time.Clock
{
    class NetworkSystemClockContextWriter : SystemClockContextUpdateCallback
    {
        private readonly TimeSharedMemory _sharedMemory;

        public NetworkSystemClockContextWriter(TimeSharedMemory sharedMemory)
        {
            _sharedMemory = sharedMemory;
        }

        protected override ResultCode Update()
        {
            _sharedMemory.UpdateNetworkSystemClockContext(Context);

            return ResultCode.Success;
        }
    }
}
