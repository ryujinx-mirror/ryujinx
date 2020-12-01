using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Memory;
using System;

namespace Ryujinx.HLE.HOS.Services.Hid.HidServer
{
    class IAppletResource : IpcService
    {
        private KSharedMemory _hidSharedMem;
        private int           _hidSharedMemHandle;

        public IAppletResource(KSharedMemory hidSharedMem)
        {
            _hidSharedMem = hidSharedMem;
        }

        [Command(0)]
        // GetSharedMemoryHandle() -> handle<copy>
        public ResultCode GetSharedMemoryHandle(ServiceCtx context)
        {
            if (_hidSharedMemHandle == 0)
            {
                if (context.Process.HandleTable.GenerateHandle(_hidSharedMem, out _hidSharedMemHandle) != KernelResult.Success)
                {
                    throw new InvalidOperationException("Out of handles!");
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_hidSharedMemHandle);

            return ResultCode.Success;
        }
    }
}