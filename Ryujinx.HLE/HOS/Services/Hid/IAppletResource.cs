using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Memory;
using System;

namespace Ryujinx.HLE.HOS.Services.Hid
{
    class IAppletResource : IpcService
    {
        private KSharedMemory _hidSharedMem;

        public IAppletResource(KSharedMemory hidSharedMem)
        {
            _hidSharedMem = hidSharedMem;
        }

        [Command(0)]
        // GetSharedMemoryHandle() -> handle<copy>
        public long GetSharedMemoryHandle(ServiceCtx context)
        {
            if (context.Process.HandleTable.GenerateHandle(_hidSharedMem, out int handle) != KernelResult.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(handle);

            return 0;
        }
    }
}