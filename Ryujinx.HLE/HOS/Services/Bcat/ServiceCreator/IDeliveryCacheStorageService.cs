using LibHac;
using LibHac.Bcat;
using LibHac.Common;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Bcat.ServiceCreator
{
    class IDeliveryCacheStorageService : DisposableIpcService
    {
        private SharedRef<LibHac.Bcat.Impl.Ipc.IDeliveryCacheStorageService> _base;

        public IDeliveryCacheStorageService(ServiceCtx context, ref SharedRef<LibHac.Bcat.Impl.Ipc.IDeliveryCacheStorageService> baseService)
        {
            _base = SharedRef<LibHac.Bcat.Impl.Ipc.IDeliveryCacheStorageService>.CreateMove(ref baseService);
        }

        [CommandHipc(0)]
        // CreateFileService() -> object<nn::bcat::detail::ipc::IDeliveryCacheFileService>
        public ResultCode CreateFileService(ServiceCtx context)
        {
            using var service = new SharedRef<LibHac.Bcat.Impl.Ipc.IDeliveryCacheFileService>();

            Result result = _base.Get.CreateFileService(ref service.Ref());

            if (result.IsSuccess())
            {
                MakeObject(context, new IDeliveryCacheFileService(ref service.Ref()));
            }

            return (ResultCode)result.Value;
        }

        [CommandHipc(1)]
        // CreateDirectoryService() -> object<nn::bcat::detail::ipc::IDeliveryCacheDirectoryService>
        public ResultCode CreateDirectoryService(ServiceCtx context)
        {
            using var service = new SharedRef<LibHac.Bcat.Impl.Ipc.IDeliveryCacheDirectoryService>();

            Result result = _base.Get.CreateDirectoryService(ref service.Ref());

            if (result.IsSuccess())
            {
                MakeObject(context, new IDeliveryCacheDirectoryService(ref service.Ref()));
            }

            return (ResultCode)result.Value;
        }

        [CommandHipc(10)]
        // EnumerateDeliveryCacheDirectory() -> (u32, buffer<nn::bcat::DirectoryName, 6>)
        public ResultCode EnumerateDeliveryCacheDirectory(ServiceCtx context)
        {
            ulong bufferAddress = context.Request.ReceiveBuff[0].Position;
            ulong bufferLen = context.Request.ReceiveBuff[0].Size;

            using (var region = context.Memory.GetWritableRegion(bufferAddress, (int)bufferLen, true))
            {
                Result result = _base.Get.EnumerateDeliveryCacheDirectory(out int count, MemoryMarshal.Cast<byte, DirectoryName>(region.Memory.Span));

                context.ResponseData.Write(count);

                return (ResultCode)result.Value;
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                _base.Destroy();
            }
        }
    }
}
