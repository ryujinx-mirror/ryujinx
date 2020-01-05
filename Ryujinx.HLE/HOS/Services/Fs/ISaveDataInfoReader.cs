using LibHac;

namespace Ryujinx.HLE.HOS.Services.Fs
{
    class ISaveDataInfoReader : IpcService
    {
        private LibHac.FsService.ISaveDataInfoReader _baseReader;

        public ISaveDataInfoReader(LibHac.FsService.ISaveDataInfoReader baseReader)
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

            Result result = _baseReader.ReadSaveDataInfo(out long readCount, infoBuffer);

            context.Memory.WriteBytes(bufferPosition, infoBuffer);
            context.ResponseData.Write(readCount);

            return (ResultCode)result.Value;
        }
    }
}
