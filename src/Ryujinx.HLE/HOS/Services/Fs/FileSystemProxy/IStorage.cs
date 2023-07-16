using LibHac;
using LibHac.Common;
using LibHac.Sf;

namespace Ryujinx.HLE.HOS.Services.Fs.FileSystemProxy
{
    class IStorage : DisposableIpcService
    {
        private SharedRef<LibHac.FsSrv.Sf.IStorage> _baseStorage;

        public IStorage(ref SharedRef<LibHac.FsSrv.Sf.IStorage> baseStorage)
        {
            _baseStorage = SharedRef<LibHac.FsSrv.Sf.IStorage>.CreateMove(ref baseStorage);
        }

        [CommandCmif(0)]
        // Read(u64 offset, u64 length) -> buffer<u8, 0x46, 0> buffer
        public ResultCode Read(ServiceCtx context)
        {
            ulong offset = context.RequestData.ReadUInt64();
            ulong size = context.RequestData.ReadUInt64();

            if (context.Request.ReceiveBuff.Count > 0)
            {
                ulong bufferAddress = context.Request.ReceiveBuff[0].Position;
                ulong bufferLen = context.Request.ReceiveBuff[0].Size;

                // Use smaller length to avoid overflows.
                if (size > bufferLen)
                {
                    size = bufferLen;
                }

                using var region = context.Memory.GetWritableRegion(bufferAddress, (int)bufferLen, true);
                Result result = _baseStorage.Get.Read((long)offset, new OutBuffer(region.Memory.Span), (long)size);

                return (ResultCode)result.Value;
            }

            return ResultCode.Success;
        }

        [CommandCmif(4)]
        // GetSize() -> u64 size
        public ResultCode GetSize(ServiceCtx context)
        {
            Result result = _baseStorage.Get.GetSize(out long size);

            context.ResponseData.Write(size);

            return (ResultCode)result.Value;
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                _baseStorage.Destroy();
            }
        }
    }
}
