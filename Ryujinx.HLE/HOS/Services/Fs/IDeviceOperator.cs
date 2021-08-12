using LibHac;
using LibHac.FsSrv;

namespace Ryujinx.HLE.HOS.Services.Fs
{
    class IDeviceOperator : DisposableIpcService
    {
        private ReferenceCountedDisposable<LibHac.FsSrv.Sf.IDeviceOperator> _baseOperator;

        public IDeviceOperator(ReferenceCountedDisposable<LibHac.FsSrv.Sf.IDeviceOperator> baseOperator)
        {
            _baseOperator = baseOperator;
        }

        [CommandHipc(0)]
        // IsSdCardInserted() -> b8 is_inserted
        public ResultCode IsSdCardInserted(ServiceCtx context)
        {
            Result result = _baseOperator.Target.IsSdCardInserted(out bool isInserted);

            context.ResponseData.Write(isInserted);

            return (ResultCode)result.Value;
        }

        [CommandHipc(200)]
        // IsGameCardInserted() -> b8 is_inserted
        public ResultCode IsGameCardInserted(ServiceCtx context)
        {
            Result result = _baseOperator.Target.IsGameCardInserted(out bool isInserted);

            context.ResponseData.Write(isInserted);

            return (ResultCode)result.Value;
        }

        [CommandHipc(202)]
        // GetGameCardHandle() -> u32 gamecard_handle
        public ResultCode GetGameCardHandle(ServiceCtx context)
        {
            Result result = _baseOperator.Target.GetGameCardHandle(out GameCardHandle handle);

            context.ResponseData.Write(handle.Value);

            return (ResultCode)result.Value;
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                _baseOperator?.Dispose();
            }
        }
    }
}
