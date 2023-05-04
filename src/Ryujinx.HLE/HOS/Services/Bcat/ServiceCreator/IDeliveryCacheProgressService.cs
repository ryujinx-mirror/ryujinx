using Ryujinx.Common.Logging;
using Ryujinx.Cpu;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.HOS.Services.Bcat.ServiceCreator.Types;
using Ryujinx.Horizon.Common;
using System;

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

        [CommandCmif(0)]
        // GetEvent() -> handle<copy>
        public ResultCode GetEvent(ServiceCtx context)
        {
            if (_eventHandle == 0)
            {
                if (context.Process.HandleTable.GenerateHandle(_event.ReadableEvent, out _eventHandle) != Result.Success)
                {
                    throw new InvalidOperationException("Out of handles!");
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_eventHandle);

            Logger.Stub?.PrintStub(LogClass.ServiceBcat);

            return ResultCode.Success;
        }

        [CommandCmif(1)]
        // GetImpl() -> buffer<nn::bcat::detail::DeliveryCacheProgressImpl, 0x1a>
        public ResultCode GetImpl(ServiceCtx context)
        {
            DeliveryCacheProgressImpl deliveryCacheProgress = new DeliveryCacheProgressImpl
            {
                State  = DeliveryCacheProgressImpl.Status.Done,
                Result = 0
            };

            ulong dcpSize = WriteDeliveryCacheProgressImpl(context, context.Request.RecvListBuff[0], deliveryCacheProgress);
            context.Response.PtrBuff[0] = context.Response.PtrBuff[0].WithSize(dcpSize);

            Logger.Stub?.PrintStub(LogClass.ServiceBcat);

            return ResultCode.Success;
        }

        private ulong WriteDeliveryCacheProgressImpl(ServiceCtx context, IpcRecvListBuffDesc ipcDesc, DeliveryCacheProgressImpl deliveryCacheProgress)
        {
            return MemoryHelper.Write(context.Memory, ipcDesc.Position, deliveryCacheProgress);
        }
    }
}