using System;
using LibHac;

namespace Ryujinx.HLE.HOS.Services.Fs
{
    class ISaveDataInfoReader : DisposableIpcService
    {
        private ReferenceCountedDisposable<LibHac.FsSrv.ISaveDataInfoReader> _baseReader;

        public ISaveDataInfoReader(ReferenceCountedDisposable<LibHac.FsSrv.ISaveDataInfoReader> baseReader)
        {
            _baseReader = baseReader;
        }

        [CommandHipc(0)]
        // ReadSaveDataInfo() -> (u64, buffer<unknown, 6>)
        public ResultCode ReadSaveDataInfo(ServiceCtx context)
        {
            ulong bufferPosition = context.Request.ReceiveBuff[0].Position;
            ulong bufferLen      = context.Request.ReceiveBuff[0].Size;

            byte[] infoBuffer = new byte[bufferLen];

            Result result = _baseReader.Target.Read(out long readCount, infoBuffer);

            context.Memory.Write(bufferPosition, infoBuffer);
            context.ResponseData.Write(readCount);

            return (ResultCode)result.Value;
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                _baseReader?.Dispose();
            }
        }
    }
}
