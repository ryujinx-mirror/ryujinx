using LibHac;
using LibHac.Bcat;
using LibHac.Common;
using Ryujinx.Common;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Bcat.ServiceCreator
{
    class IDeliveryCacheDirectoryService : DisposableIpcService
    {
        private SharedRef<LibHac.Bcat.Impl.Ipc.IDeliveryCacheDirectoryService> _base;

        public IDeliveryCacheDirectoryService(ref SharedRef<LibHac.Bcat.Impl.Ipc.IDeliveryCacheDirectoryService> baseService)
        {
            _base = SharedRef<LibHac.Bcat.Impl.Ipc.IDeliveryCacheDirectoryService>.CreateMove(ref baseService);
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                _base.Destroy();
            }
        }

        [CommandHipc(0)]
        // Open(nn::bcat::DirectoryName)
        public ResultCode Open(ServiceCtx context)
        {
            DirectoryName directoryName = context.RequestData.ReadStruct<DirectoryName>();

            Result result = _base.Get.Open(ref directoryName);

            return (ResultCode)result.Value;
        }

        [CommandHipc(1)]
        // Read() -> (u32, buffer<nn::bcat::DeliveryCacheDirectoryEntry, 6>)
        public ResultCode Read(ServiceCtx context)
        {
            ulong position = context.Request.ReceiveBuff[0].Position;
            ulong size = context.Request.ReceiveBuff[0].Size;

            byte[] data = new byte[size];

            Result result = _base.Get.Read(out int entriesRead, MemoryMarshal.Cast<byte, DeliveryCacheDirectoryEntry>(data));

            context.Memory.Write(position, data);

            context.ResponseData.Write(entriesRead);

            return (ResultCode)result.Value;
        }

        [CommandHipc(2)]
        // GetCount() -> u32
        public ResultCode GetCount(ServiceCtx context)
        {
            Result result = _base.Get.GetCount(out int count);

            context.ResponseData.Write(count);

            return (ResultCode)result.Value;
        }
    }
}
