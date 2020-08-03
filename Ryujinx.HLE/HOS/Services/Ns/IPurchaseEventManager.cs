using Ryujinx.Common;
using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Threading;
using System;

namespace Ryujinx.HLE.HOS.Services.Ns
{
    class IPurchaseEventManager : IpcService
    {
        private readonly KEvent _purchasedEvent;

        public IPurchaseEventManager(Horizon system)
        {
            _purchasedEvent = new KEvent(system.KernelContext);
        }

        [Command(0)]
        // SetDefaultDeliveryTarget(pid, buffer<bytes, 5> unknown)
        public ResultCode SetDefaultDeliveryTarget(ServiceCtx context)
        {
            long   inBufferPosition = context.Request.SendBuff[0].Position;
            long   inBufferSize     = context.Request.SendBuff[0].Size;
            byte[] buffer           = new byte[inBufferSize];

            context.Memory.Read((ulong)inBufferPosition, buffer);

            // NOTE: Service use the pid to call arp:r GetApplicationLaunchProperty and store it in internal field.
            //       Then it seems to use the buffer content and compare it with a stored linked instrusive list.
            //       Since we don't support purchase from eShop, we can stub it.

            Logger.Stub?.PrintStub(LogClass.ServiceNs);

            return ResultCode.Success;
        }

        [Command(2)]
        // GetPurchasedEventReadableHandle() -> handle<copy, event>
        public ResultCode GetPurchasedEventReadableHandle(ServiceCtx context)
        {
            if (context.Process.HandleTable.GenerateHandle(_purchasedEvent.ReadableEvent, out int handle) != KernelResult.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(handle);

            return ResultCode.Success;
        }
    }
}