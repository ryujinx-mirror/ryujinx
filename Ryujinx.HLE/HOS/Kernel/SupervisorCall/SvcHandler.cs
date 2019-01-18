using ChocolArm64.Events;
using ChocolArm64.Memory;
using ChocolArm64.State;
using Ryujinx.HLE.HOS.Kernel.Process;
using System;

namespace Ryujinx.HLE.HOS.Kernel.SupervisorCall
{
    partial class SvcHandler
    {
        private Switch        _device;
        private KProcess      _process;
        private Horizon       _system;
        private MemoryManager _memory;

        public SvcHandler(Switch device, KProcess process)
        {
            _device  = device;
            _process = process;
            _system  = device.System;
            _memory  = process.CpuMemory;
        }

        public void SvcCall(object sender, InstExceptionEventArgs e)
        {
            Action<SvcHandler, CpuThreadState> svcFunc = SvcTable.GetSvcFunc(e.Id);

            if (svcFunc == null)
            {
                throw new NotImplementedException($"SVC 0x{e.Id:X4} is not implemented.");
            }

            CpuThreadState threadState = (CpuThreadState)sender;

            svcFunc(this, threadState);
        }
    }
}