using LibHac;
using LibHac.Common;

using GameCardHandle = System.UInt32;

namespace Ryujinx.HLE.HOS.Services.Fs
{
    class IDeviceOperator : DisposableIpcService
    {
        private SharedRef<LibHac.FsSrv.Sf.IDeviceOperator> _baseOperator;

        public IDeviceOperator(ref SharedRef<LibHac.FsSrv.Sf.IDeviceOperator> baseOperator)
        {
            _baseOperator = SharedRef<LibHac.FsSrv.Sf.IDeviceOperator>.CreateMove(ref baseOperator);
        }

        [CommandCmif(0)]
        // IsSdCardInserted() -> b8 is_inserted
        public ResultCode IsSdCardInserted(ServiceCtx context)
        {
            Result result = _baseOperator.Get.IsSdCardInserted(out bool isInserted);

            context.ResponseData.Write(isInserted);

            return (ResultCode)result.Value;
        }

        [CommandCmif(200)]
        // IsGameCardInserted() -> b8 is_inserted
        public ResultCode IsGameCardInserted(ServiceCtx context)
        {
            Result result = _baseOperator.Get.IsGameCardInserted(out bool isInserted);

            context.ResponseData.Write(isInserted);

            return (ResultCode)result.Value;
        }

        [CommandCmif(202)]
        // GetGameCardHandle() -> u32 gamecard_handle
        public ResultCode GetGameCardHandle(ServiceCtx context)
        {
            Result result = _baseOperator.Get.GetGameCardHandle(out GameCardHandle handle);

            context.ResponseData.Write(handle);

            return (ResultCode)result.Value;
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                _baseOperator.Destroy();
            }
        }
    }
}
