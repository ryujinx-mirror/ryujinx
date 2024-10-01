using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.Horizon.Common;
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

        [CommandCmif(0)]
        // SetDefaultDeliveryTarget(pid, buffer<bytes, 5> unknown)
        public ResultCode SetDefaultDeliveryTarget(ServiceCtx context)
        {
            ulong inBufferPosition = context.Request.SendBuff[0].Position;
            ulong inBufferSize = context.Request.SendBuff[0].Size;
            byte[] buffer = new byte[inBufferSize];

            context.Memory.Read(inBufferPosition, buffer);

            // NOTE: Service uses the pid to call arp:r GetApplicationLaunchProperty and store it in internal field.
            //       Then it seems to use the buffer content and compare it with a stored linked instrusive list.
            //       Since we don't support purchase from eShop, we can stub it.

            Logger.Stub?.PrintStub(LogClass.ServiceNs);

            return ResultCode.Success;
        }

        [CommandCmif(2)]
        // GetPurchasedEventReadableHandle() -> handle<copy, event>
        public ResultCode GetPurchasedEventReadableHandle(ServiceCtx context)
        {
            if (context.Process.HandleTable.GenerateHandle(_purchasedEvent.ReadableEvent, out int purchasedEventReadableHandle) != Result.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(purchasedEventReadableHandle);

            return ResultCode.Success;
        }

        [CommandCmif(3)]
        // PopPurchasedProductInfo(nn::ec::detail::PurchasedProductInfo)
        public ResultCode PopPurchasedProductInfo(ServiceCtx context)
        {
            byte[] purchasedProductInfo = new byte[0x80];

            context.ResponseData.Write(purchasedProductInfo);

            // NOTE: Service finds info using internal array then convert it into nn::ec::detail::PurchasedProductInfo.
            //       Returns 0x320A4 if the internal array size is null.
            //       Since we don't support purchase from eShop, we can stub it.

            Logger.Debug?.PrintStub(LogClass.ServiceNs); // NOTE: Uses Debug to avoid spamming.

            return ResultCode.Success;
        }
    }
}
