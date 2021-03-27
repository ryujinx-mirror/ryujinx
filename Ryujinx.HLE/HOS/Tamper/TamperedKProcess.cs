using Ryujinx.Common.Logging;
using Ryujinx.HLE.Exceptions;
using Ryujinx.HLE.HOS.Kernel.Process;
using System.Runtime.CompilerServices;

namespace Ryujinx.HLE.HOS.Tamper
{
    class TamperedKProcess : ITamperedProcess
    {
        private KProcess _process;

        public ProcessState State => _process.State;

        public TamperedKProcess(KProcess process)
        {
            this._process = process;
        }

        private void AssertMemoryRegion<T>(ulong va, bool isWrite) where T : unmanaged
        {
            ulong size = (ulong)Unsafe.SizeOf<T>();

            // TODO (Caian): This double check is workaround because CpuMemory.IsRangeMapped reports
            // some addresses as mapped even though they are not, i. e. 4 bytes from 0xffffffffffffff70.
            if (!_process.CpuMemory.IsMapped(va) || !_process.CpuMemory.IsRangeMapped(va, size))
            {
                throw new TamperExecutionException($"Unmapped memory access of {size} bytes at 0x{va:X16}");
            }

            if (!isWrite)
            {
                return;
            }

            // TODO (Caian): It is unknown how PPTC behaves if the tamper modifies memory regions
            // belonging to code. So for now just prevent code tampering.
            if ((va >= _process.MemoryManager.CodeRegionStart) && (va + size <= _process.MemoryManager.CodeRegionEnd))
            {
                throw new CodeRegionTamperedException($"Writing {size} bytes to address 0x{va:X16} alters code");
            }
        }

        public T ReadMemory<T>(ulong va) where T : unmanaged
        {
            AssertMemoryRegion<T>(va, false);

            return _process.CpuMemory.Read<T>(va);
        }

        public void WriteMemory<T>(ulong va, T value) where T : unmanaged
        {
            AssertMemoryRegion<T>(va, true);
            _process.CpuMemory.Write(va, value);
        }

        public void PauseProcess()
        {
            Logger.Warning?.Print(LogClass.TamperMachine, "Process pausing is not supported!");
        }

        public void ResumeProcess()
        {
            Logger.Warning?.Print(LogClass.TamperMachine, "Process resuming is not supported!");
        }
    }
}