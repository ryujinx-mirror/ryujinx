using Ryujinx.Common;
using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.HOS.Services.Bcat.ServiceCreator.Types;
using System;
using System.IO;

namespace Ryujinx.HLE.HOS.Services.Bcat.ServiceCreator
{
    class IDeliveryCacheProgressService : IpcService
    {
        private KEvent _event;
        private int    _eventHandle;

        public IDeliveryCacheProgressService(ServiceCtx context)
        {
            _event = new KEvent(context.Device.System.KernelContext);
        }

        [Command(0)]
        // GetEvent() -> handle<copy>
        public ResultCode GetEvent(ServiceCtx context)
        {
            if (_eventHandle == 0)
            {
                if (context.Process.HandleTable.GenerateHandle(_event.ReadableEvent, out _eventHandle) != KernelResult.Success)
                {
                    throw new InvalidOperationException("Out of handles!");
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_eventHandle);

            Logger.Stub?.PrintStub(LogClass.ServiceBcat);

            return ResultCode.Success;
        }

        [Command(1)]
        // GetImpl() -> buffer<nn::bcat::detail::DeliveryCacheProgressImpl, 0x1a>
        public ResultCode GetImpl(ServiceCtx context)
        {
            DeliveryCacheProgressImpl deliveryCacheProgress = new DeliveryCacheProgressImpl
            {
                State  = DeliveryCacheProgressImpl.Status.Done,
                Result = 0
            };

            WriteDeliveryCacheProgressImpl(context, context.Request.RecvListBuff[0], deliveryCacheProgress);

            Logger.Stub?.PrintStub(LogClass.ServiceBcat);

            return ResultCode.Success;
        }

        private void WriteDeliveryCacheProgressImpl(ServiceCtx context, IpcRecvListBuffDesc ipcDesc, DeliveryCacheProgressImpl deliveryCacheProgress)
        {
            using (MemoryStream memory = new MemoryStream((int)ipcDesc.Size))
            using (BinaryWriter bufferWriter = new BinaryWriter(memory))
            {
                bufferWriter.WriteStruct(deliveryCacheProgress);
                context.Memory.Write((ulong)ipcDesc.Position, memory.ToArray());
            }
        }
    }
}