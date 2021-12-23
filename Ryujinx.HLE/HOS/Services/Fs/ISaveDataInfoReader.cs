using LibHac;
using LibHac.Common;
using LibHac.Sf;

namespace Ryujinx.HLE.HOS.Services.Fs
{
    class ISaveDataInfoReader : DisposableIpcService
    {
        private SharedRef<LibHac.FsSrv.Sf.ISaveDataInfoReader> _baseReader;

        public ISaveDataInfoReader(ref SharedRef<LibHac.FsSrv.Sf.ISaveDataInfoReader> baseReader)
        {
            _baseReader = SharedRef<LibHac.FsSrv.Sf.ISaveDataInfoReader>.CreateMove(ref baseReader);
        }

        [CommandHipc(0)]
        // ReadSaveDataInfo() -> (u64, buffer<unknown, 6>)
        public ResultCode ReadSaveDataInfo(ServiceCtx context)
        {
            ulong bufferPosition = context.Request.ReceiveBuff[0].Position;
            ulong bufferLen = context.Request.ReceiveBuff[0].Size;

            byte[] infoBuffer = new byte[bufferLen];

            Result result = _baseReader.Get.Read(out long readCount, new OutBuffer(infoBuffer));

            context.Memory.Write(bufferPosition, infoBuffer);
            context.ResponseData.Write(readCount);

            return (ResultCode)result.Value;
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                _baseReader.Destroy();
            }
        }
    }
}
