using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Threading;
using System;

namespace Ryujinx.HLE.HOS.Services.Ns.Aoc
{
    class IPurchaseEventManager : IpcService
    {
        private readonly KEvent _purchasedEvent;

        public IPurchaseEventManager(Horizon system)
        {
            _purchasedEvent = new KEvent(system.KernelContext);
        }

        [CommandHipc(0)]
        // SetDefaultDeliveryTarget(pid, buffer<bytes, 5> unknown)
        public ResultCode SetDefaultDeliveryTarget(ServiceCtx context)
        {
            ulong  inBufferPosition = context.Request.SendBuff[0].Position;
            ulong  inBufferSize     = context.Request.SendBuff[0].Size;
            byte[] buffer           = new byte[inBufferSize];

            context.Memory.Read(inBufferPosition, buffer);

            // NOTE: Service use the pid to call arp:r GetApplicationLaunchProperty and store it in internal field.
            //       Then it seems to use the buffer content and compare it with a stored linked instrusive list.
            //       Since we don't support purchase from eShop, we can stub it.

            Logger.Stub?.PrintStub(LogClass.ServiceNs);

            return ResultCode.Success;
        }

        [CommandHipc(2)]
        // GetPurchasedEventReadableHandle() -> handle<copy, event>
        public ResultCode GetPurchasedEventReadableHandle(ServiceCtx context)
        {
            if (context.Process.HandleTable.GenerateHandle(_purchasedEvent.ReadableEvent, out int purchasedEventReadableHandle) != KernelResult.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(purchasedEventReadableHandle);

            return ResultCode.Success;
        }
    }
}