namespace Ryujinx.HLE.HOS.Services.Time.Clock
{
    class LocalSystemClockContextWriter : SystemClockContextUpdateCallback
    {
        private TimeSharedMemory _sharedMemory;

        public LocalSystemClockContextWriter(TimeSharedMemory sharedMemory)
        {
            _sharedMemory = sharedMemory;
        }

        protected override ResultCode Update()
        {
            _sharedMemory.UpdateLocalSystemClockContext(_context);

            return ResultCode.Success;
        }
    }
}
