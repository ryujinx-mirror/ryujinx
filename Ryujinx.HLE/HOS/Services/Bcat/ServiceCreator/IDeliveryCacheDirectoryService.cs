using LibHac;
using LibHac.Bcat;
using Ryujinx.Common;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Bcat.ServiceCreator
{
    class IDeliveryCacheDirectoryService : IpcService, IDisposable
    {
        private LibHac.Bcat.Detail.Ipc.IDeliveryCacheDirectoryService _base;

        public IDeliveryCacheDirectoryService(LibHac.Bcat.Detail.Ipc.IDeliveryCacheDirectoryService baseService)
        {
            _base = baseService;
        }

        [Command(0)]
        // Open(nn::bcat::DirectoryName)
        public ResultCode Open(ServiceCtx context)
        {
            DirectoryName directoryName = context.RequestData.ReadStruct<DirectoryName>();

            Result result = _base.Open(ref directoryName);

            return (ResultCode)result.Value;
        }

        [Command(1)]
        // Read() -> (u32, buffer<nn::bcat::DeliveryCacheDirectoryEntry, 6>)
        public ResultCode Read(ServiceCtx context)
        {
            long position = context.Request.ReceiveBuff[0].Position;
            long size = context.Request.ReceiveBuff[0].Size;

            byte[] data = new byte[size];

            Result result = _base.Read(out int entriesRead, MemoryMarshal.Cast<byte, DeliveryCacheDirectoryEntry>(data));

            context.Memory.Write((ulong)position, data);

            context.ResponseData.Write(entriesRead);

            return (ResultCode)result.Value;
        }

        [Command(2)]
        // GetCount() -> u32
        public ResultCode GetCount(ServiceCtx context)
        {
            Result result = _base.GetCount(out int count);

            context.ResponseData.Write(count);

            return (ResultCode)result.Value;
        }

        public void Dispose()
        {
            _base?.Dispose();
        }
    }
}
