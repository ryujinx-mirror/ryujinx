using LibHac;
using Ryujinx.HLE.HOS.Ipc;

namespace Ryujinx.HLE.HOS.Services.Fs.FileSystemProxy
{
    class IStorage : DisposableIpcService
    {
        private LibHac.Fs.IStorage _baseStorage;

        public IStorage(LibHac.Fs.IStorage baseStorage)
        {
            _baseStorage = baseStorage;
        }

        [CommandHipc(0)]
        // Read(u64 offset, u64 length) -> buffer<u8, 0x46, 0> buffer
        public ResultCode Read(ServiceCtx context)
        {
            ulong offset = context.RequestData.ReadUInt64();
            ulong size   = context.RequestData.ReadUInt64();

            if (context.Request.ReceiveBuff.Count > 0)
            {
                IpcBuffDesc buffDesc = context.Request.ReceiveBuff[0];

                // Use smaller length to avoid overflows.
                if (size > buffDesc.Size)
                {
                    size = buffDesc.Size;
                }

                byte[] data = new byte[size];

                Result result = _baseStorage.Read((long)offset, data);

                context.Memory.Write(buffDesc.Position, data);

                return (ResultCode)result.Value;
            }

            return ResultCode.Success;
        }

        [CommandHipc(4)]
        // GetSize() -> u64 size
        public ResultCode GetSize(ServiceCtx context)
        {
            Result result = _baseStorage.GetSize(out long size);

            context.ResponseData.Write(size);

            return (ResultCode)result.Value;
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                _baseStorage?.Dispose();
            }
        }
    }
}