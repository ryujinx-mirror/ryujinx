using LibHac;
using LibHac.Bcat;
using Ryujinx.Common;

namespace Ryujinx.HLE.HOS.Services.Bcat.ServiceCreator
{
    class IDeliveryCacheFileService : DisposableIpcService
    {
        private LibHac.Bcat.Impl.Ipc.IDeliveryCacheFileService _base;

        public IDeliveryCacheFileService(LibHac.Bcat.Impl.Ipc.IDeliveryCacheFileService baseService)
        {
            _base = baseService;
        }

        [CommandHipc(0)]
        // Open(nn::bcat::DirectoryName, nn::bcat::FileName)
        public ResultCode Open(ServiceCtx context)
        {
            DirectoryName directoryName = context.RequestData.ReadStruct<DirectoryName>();
            FileName fileName = context.RequestData.ReadStruct<FileName>();

            Result result = _base.Open(ref directoryName, ref fileName);

            return (ResultCode)result.Value;
        }

        [CommandHipc(1)]
        // Read(u64) -> (u64, buffer<bytes, 6>)
        public ResultCode Read(ServiceCtx context)
        {
            ulong position = context.Request.ReceiveBuff[0].Position;
            ulong size = context.Request.ReceiveBuff[0].Size;

            long offset = context.RequestData.ReadInt64();

            byte[] data = new byte[size];

            Result result = _base.Read(out long bytesRead, offset, data);

            context.Memory.Write(position, data);

            context.ResponseData.Write(bytesRead);

            return (ResultCode)result.Value;
        }

        [CommandHipc(2)]
        // GetSize() -> u64
        public ResultCode GetSize(ServiceCtx context)
        {
            Result result = _base.GetSize(out long size);

            context.ResponseData.Write(size);

            return (ResultCode)result.Value;
        }

        [CommandHipc(3)]
        // GetDigest() -> nn::bcat::Digest
        public ResultCode GetDigest(ServiceCtx context)
        {
            Result result = _base.GetDigest(out Digest digest);

            context.ResponseData.WriteStruct(digest);

            return (ResultCode)result.Value;
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                _base?.Dispose();
            }
        }
    }
}
