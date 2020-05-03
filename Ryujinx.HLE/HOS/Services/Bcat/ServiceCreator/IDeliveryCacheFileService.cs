using LibHac;
using LibHac.Bcat;
using Ryujinx.Common;
using System;

namespace Ryujinx.HLE.HOS.Services.Bcat.ServiceCreator
{
    class IDeliveryCacheFileService : IpcService, IDisposable
    {
        private LibHac.Bcat.Detail.Ipc.IDeliveryCacheFileService _base;

        public IDeliveryCacheFileService(LibHac.Bcat.Detail.Ipc.IDeliveryCacheFileService baseService)
        {
            _base = baseService;
        }

        [Command(0)]
        // Open(nn::bcat::DirectoryName, nn::bcat::FileName)
        public ResultCode Open(ServiceCtx context)
        {
            DirectoryName directoryName = context.RequestData.ReadStruct<DirectoryName>();
            FileName fileName = context.RequestData.ReadStruct<FileName>();

            Result result = _base.Open(ref directoryName, ref fileName);

            return (ResultCode)result.Value;
        }

        [Command(1)]
        // Read(u64) -> (u64, buffer<bytes, 6>)
        public ResultCode Read(ServiceCtx context)
        {
            long position = context.Request.ReceiveBuff[0].Position;
            long size = context.Request.ReceiveBuff[0].Size;

            long offset = context.RequestData.ReadInt64();

            byte[] data = new byte[size];

            Result result = _base.Read(out long bytesRead, offset, data);

            context.Memory.Write((ulong)position, data);

            context.ResponseData.Write(bytesRead);

            return (ResultCode)result.Value;
        }

        [Command(2)]
        // GetSize() -> u64
        public ResultCode GetSize(ServiceCtx context)
        {
            Result result = _base.GetSize(out long size);

            context.ResponseData.Write(size);

            return (ResultCode)result.Value;
        }

        [Command(3)]
        // GetDigest() -> nn::bcat::Digest
        public ResultCode GetDigest(ServiceCtx context)
        {
            Result result = _base.GetDigest(out Digest digest);

            context.ResponseData.WriteStruct(digest);

            return (ResultCode)result.Value;
        }

        public void Dispose()
        {
            _base?.Dispose();
        }
    }
}
