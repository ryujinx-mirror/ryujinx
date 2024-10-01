using Ryujinx.Common.Logging;

namespace Ryujinx.HLE.HOS.Services.Mii
{
    [Service("miiimg")] // 5.0.0+
    class IImageDatabaseService : IpcService
    {
        private uint _imageCount;
        private bool _isDirty;

        public IImageDatabaseService(ServiceCtx context) { }

        [CommandCmif(0)]
        // Initialize(b8) -> b8
        public ResultCode Initialize(ServiceCtx context)
        {
            // TODO: Service uses MiiImage:/database.dat if true, seems to use hardcoded data if false.
            bool useHardcodedData = context.RequestData.ReadBoolean();

            _imageCount = 0;
            _isDirty = false;

            context.ResponseData.Write(_isDirty);

            Logger.Stub?.PrintStub(LogClass.ServiceMii, new { useHardcodedData });

            return ResultCode.Success;
        }

        [CommandCmif(11)]
        // GetCount() -> u32
        public ResultCode GetCount(ServiceCtx context)
        {
            context.ResponseData.Write(_imageCount);

            Logger.Stub?.PrintStub(LogClass.ServiceMii);

            return ResultCode.Success;
        }
    }
}
