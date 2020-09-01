using System;
using LibHac;

namespace Ryujinx.HLE.HOS.Services.Fs
{
    class ISaveDataInfoReader : IpcService, IDisposable
    {
        private ReferenceCountedDisposable<LibHac.FsSrv.ISaveDataInfoReader> _baseReader;

        public ISaveDataInfoReader(ReferenceCountedDisposable<LibHac.FsSrv.ISaveDataInfoReader> baseReader)
        {
            _baseReader = baseReader;
        }

        [Command(0)]
        // ReadSaveDataInfo() -> (u64, buffer<unknown, 6>)
        public ResultCode ReadSaveDataInfo(ServiceCtx context)
        {
            long bufferPosition = context.Request.ReceiveBuff[0].Position;
            long bufferLen      = context.Request.ReceiveBuff[0].Size;

            byte[] infoBuffer = new byte[bufferLen];

            Result result = _baseReader.Target.Read(out long readCount, infoBuffer);

            context.Memory.Write((ulong)bufferPosition, infoBuffer);
            context.ResponseData.Write(readCount);

            return (ResultCode)result.Value;
        }

        public void Dispose()
        {
            _baseReader.Dispose();
        }
    }
}
