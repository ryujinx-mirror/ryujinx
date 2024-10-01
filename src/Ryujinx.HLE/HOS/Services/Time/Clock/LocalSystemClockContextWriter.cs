namespace Ryujinx.HLE.HOS.Services.Time.Clock
{
    class LocalSystemClockContextWriter : SystemClockContextUpdateCallback
    {
        private readonly TimeSharedMemory _sharedMemory;

        public LocalSystemClockContextWriter(TimeSharedMemory sharedMemory)
        {
            _sharedMemory = sharedMemory;
        }

        protected override ResultCode Update()
        {
            _sharedMemory.UpdateLocalSystemClockContext(Context);

            return ResultCode.Success;
        }
    }
}
