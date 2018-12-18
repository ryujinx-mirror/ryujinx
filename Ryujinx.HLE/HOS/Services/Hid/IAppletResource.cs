using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Memory;
using System;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Hid
{
    class IAppletResource : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        private KSharedMemory _hidSharedMem;

        public IAppletResource(KSharedMemory hidSharedMem)
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 0, GetSharedMemoryHandle }
            };

            _hidSharedMem = hidSharedMem;
        }

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