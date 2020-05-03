using LibHac;
using LibHac.Bcat;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Bcat.ServiceCreator
{
    class IDeliveryCacheStorageService : IpcService, IDisposable
    {
        private LibHac.Bcat.Detail.Ipc.IDeliveryCacheStorageService _base;

        public IDeliveryCacheStorageService(ServiceCtx context, LibHac.Bcat.Detail.Ipc.IDeliveryCacheStorageService baseService)
        {
            _base = baseService;
        }

        [Command(0)]
        // CreateFileService() -> object<nn::bcat::detail::ipc::IDeliveryCacheFileService>
        public ResultCode CreateFileService(ServiceCtx context)
        {
            Result result = _base.CreateFileService(out LibHac.Bcat.Detail.Ipc.IDeliveryCacheFileService service);

            if (result.IsSuccess())
            {
                MakeObject(context, new IDeliveryCacheFileService(service));
            }

            return (ResultCode)result.Value;
        }

        [Command(1)]
        // CreateDirectoryService() -> object<nn::bcat::detail::ipc::IDeliveryCacheDirectoryService>
        public ResultCode CreateDirectoryService(ServiceCtx context)
        {
            Result result = _base.CreateDirectoryService(out LibHac.Bcat.Detail.Ipc.IDeliveryCacheDirectoryService service);

            if (result.IsSuccess())
            {
                MakeObject(context, new IDeliveryCacheDirectoryService(service));
            }

            return (ResultCode)result.Value;
        }

        [Command(10)]
        // EnumerateDeliveryCacheDirectory() -> (u32, buffer<nn::bcat::DirectoryName, 6>)
        public ResultCode EnumerateDeliveryCacheDirectory(ServiceCtx context)
        {
            long position = context.Request.ReceiveBuff[0].Position;
            long size = context.Request.ReceiveBuff[0].Size;

            byte[] data = new byte[size];

            Result result = _base.EnumerateDeliveryCacheDirectory(out int count, MemoryMarshal.Cast<byte, DirectoryName>(data));

            context.Memory.Write((ulong)position, data);

            context.ResponseData.Write(count);

            return (ResultCode)result.Value;
        }

        public void Dispose()
        {
            _base?.Dispose();
        }
    }
}
