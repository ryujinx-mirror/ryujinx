using LibHac;
using LibHac.FsSrv;

namespace Ryujinx.HLE.HOS.Services.Fs
{
    class IDeviceOperator : IpcService
    {
        private LibHac.FsSrv.IDeviceOperator _baseOperator;

        public IDeviceOperator(LibHac.FsSrv.IDeviceOperator baseOperator)
        {
            _baseOperator = baseOperator;
        }

        [Command(0)]
        // IsSdCardInserted() -> b8 is_inserted
        public ResultCode IsSdCardInserted(ServiceCtx context)
        {
            Result result = _baseOperator.IsSdCardInserted(out bool isInserted);

            context.ResponseData.Write(isInserted);

            return (ResultCode)result.Value;
        }

        [Command(200)]
        // IsGameCardInserted() -> b8 is_inserted
        public ResultCode IsGameCardInserted(ServiceCtx context)
        {
            Result result = _baseOperator.IsGameCardInserted(out bool isInserted);

            context.ResponseData.Write(isInserted);

            return (ResultCode)result.Value;
        }

        [Command(202)]
        // GetGameCardHandle() -> u32 gamecard_handle
        public ResultCode GetGameCardHandle(ServiceCtx context)
        {
            Result result = _baseOperator.GetGameCardHandle(out GameCardHandle handle);

            context.ResponseData.Write(handle.Value);

            return (ResultCode)result.Value;
        }
    }
}
