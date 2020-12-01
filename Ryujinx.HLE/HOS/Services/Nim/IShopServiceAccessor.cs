using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.HOS.Services.Nim.ShopServiceAccessServerInterface.ShopServiceAccessServer.ShopServiceAccessor;
using System;

namespace Ryujinx.HLE.HOS.Services.Nim.ShopServiceAccessServerInterface.ShopServiceAccessServer
{
    class IShopServiceAccessor : IpcService
    {
        private readonly KEvent _event;

        private int _eventHandle;

        public IShopServiceAccessor(Horizon system)
        {
            _event = new KEvent(system.KernelContext);
        }

        [Command(0)]
        // CreateAsyncInterface(u64) -> (handle<copy>, object<nn::ec::IShopServiceAsync>)
        public ResultCode CreateAsyncInterface(ServiceCtx context)
        {
            MakeObject(context, new IShopServiceAsync());

            if (_eventHandle == 0)
            {
                if (context.Process.HandleTable.GenerateHandle(_event.ReadableEvent, out _eventHandle) != KernelResult.Success)
                {
                    throw new InvalidOperationException("Out of handles!");
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_eventHandle);

            Logger.Stub?.PrintStub(LogClass.ServiceNim);

            return ResultCode.Success;
        }
    }
}