using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Threading;
using System;

namespace Ryujinx.HLE.HOS.Services.Am.AppletAE.AllSystemAppletProxiesService.SystemAppletProxy
{
    class IHomeMenuFunctions : IpcService
    {
        private KEvent _channelEvent;
        private int    _channelEventHandle;

        public IHomeMenuFunctions(Horizon system)
        {
            // TODO: Signal this Event somewhere in future.
            _channelEvent = new KEvent(system.KernelContext);
        }

        [Command(10)]
        // RequestToGetForeground()
        public ResultCode RequestToGetForeground(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAm);

            return ResultCode.Success;
        }

        [Command(21)]
        // GetPopFromGeneralChannelEvent() -> handle<copy>
        public ResultCode GetPopFromGeneralChannelEvent(ServiceCtx context)
        {
            if (_channelEventHandle == 0)
            {
                if (context.Process.HandleTable.GenerateHandle(_channelEvent.ReadableEvent, out _channelEventHandle) != KernelResult.Success)
                {
                    throw new InvalidOperationException("Out of handles!");
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_channelEventHandle);

            Logger.Stub?.PrintStub(LogClass.ServiceAm);

            return ResultCode.Success;
        }
    }
}