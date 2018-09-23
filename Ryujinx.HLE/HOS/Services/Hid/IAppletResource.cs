using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel;
using System;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Hid
{
    class IAppletResource : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        private KSharedMemory HidSharedMem;

        public IAppletResource(KSharedMemory HidSharedMem)
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0, GetSharedMemoryHandle }
            };

            this.HidSharedMem = HidSharedMem;
        }

        public long GetSharedMemoryHandle(ServiceCtx Context)
        {
            if (Context.Process.HandleTable.GenerateHandle(HidSharedMem, out int Handle) != KernelResult.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            Context.Response.HandleDesc = IpcHandleDesc.MakeCopy(Handle);

            return 0;
        }
    }
}