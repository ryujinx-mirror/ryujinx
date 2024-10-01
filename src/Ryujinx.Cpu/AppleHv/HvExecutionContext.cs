using ARMeilleure.State;
using Ryujinx.Cpu.AppleHv.Arm;
using Ryujinx.Memory.Tracking;
using System;
using System.Runtime.Versioning;
using System.Threading;

namespace Ryujinx.Cpu.AppleHv
{
    [SupportedOSPlatform("macos")]
    class HvExecutionContext : IExecutionContext
    {
        /// <inheritdoc/>
        public ulong Pc => _impl.ElrEl1;

        /// <inheritdoc/>
        public long TpidrEl0
        {
            get => _impl.TpidrEl0;
            set => _impl.TpidrEl0 = value;
        }

        /// <inheritdoc/>
        public long TpidrroEl0
        {
            get => _impl.TpidrroEl0;
            set => _impl.TpidrroEl0 = value;
        }

        /// <inheritdoc/>
        public uint Pstate
        {
            get => _impl.Pstate;
            set => _impl.Pstate = value;
        }

        /// <inheritdoc/>
        public uint Fpcr
        {
            get => _impl.Fpcr;
            set => _impl.Fpcr = value;
        }

        /// <inheritdoc/>
        public uint Fpsr
        {
            get => _impl.Fpsr;
            set => _impl.Fpsr = value;
        }

        /// <inheritdoc/>
        public bool IsAarch32
        {
            get => false;
            set
            {
                if (value)
                {
                    throw new NotSupportedException();
                }
            }
        }

        /// <inheritdoc/>
        public bool Running { get; private set; }

        private readonly ICounter _counter;
        private readonly IHvExecutionContext _shadowContext;
        private IHvExecutionContext _impl;

        private readonly ExceptionCallbacks _exceptionCallbacks;

        private int _interruptRequested;

        public HvExecutionContext(ICounter counter, ExceptionCallbacks exceptionCallbacks)
        {
            _counter = counter;
            _shadowContext = new HvExecutionContextShadow();
            _impl = _shadowContext;
            _exceptionCallbacks = exceptionCallbacks;
            Running = true;
        }

        /// <inheritdoc/>
        public ulong GetX(int index) => _impl.GetX(index);

        /// <inheritdoc/>
        public void SetX(int index, ulong value) => _impl.SetX(index, value);

        /// <inheritdoc/>
        public V128 GetV(int index) => _impl.GetV(index);

        /// <inheritdoc/>
        public void SetV(int index, V128 value) => _impl.SetV(index, value);

        private void InterruptHandler()
        {
            _exceptionCallbacks.InterruptCallback?.Invoke(this);
        }

        private void BreakHandler(ulong address, int imm)
        {
            _exceptionCallbacks.BreakCallback?.Invoke(this, address, imm);
        }

        private void SupervisorCallHandler(ulong address, int imm)
        {
            _exceptionCallbacks.SupervisorCallback?.Invoke(this, address, imm);
        }

        private void UndefinedHandler(ulong address, int opCode)
        {
            _exceptionCallbacks.UndefinedCallback?.Invoke(this, address, opCode);
        }

        /// <inheritdoc/>
        public void RequestInterrupt()
        {
            if (Interlocked.Exchange(ref _interruptRequested, 1) == 0 && _impl is HvExecutionContextVcpu impl)
            {
                impl.RequestInterrupt();
            }
        }

        private bool GetAndClearInterruptRequested()
        {
            return Interlocked.Exchange(ref _interruptRequested, 0) != 0;
        }

        /// <inheritdoc/>
        public void StopRunning()
        {
            Running = false;
            RequestInterrupt();
        }

        public unsafe void Execute(HvMemoryManager memoryManager, ulong address)
        {
            HvVcpu vcpu = HvVcpuPool.Instance.Create(memoryManager.AddressSpace, _shadowContext, SwapContext);

            HvApi.hv_vcpu_set_reg(vcpu.Handle, HvReg.PC, address).ThrowOnError();

            while (Running)
            {
                HvApi.hv_vcpu_run(vcpu.Handle).ThrowOnError();

                HvExitReason reason = vcpu.ExitInfo->Reason;

                if (reason == HvExitReason.Exception)
                {
                    uint hvEsr = (uint)vcpu.ExitInfo->Exception.Syndrome;
                    ExceptionClass hvEc = (ExceptionClass)(hvEsr >> 26);

                    if (hvEc != ExceptionClass.HvcAarch64)
                    {
                        throw new Exception($"Unhandled exception from guest kernel with ESR 0x{hvEsr:X} ({hvEc}).");
                    }

                    address = SynchronousException(memoryManager, ref vcpu);
                    HvApi.hv_vcpu_set_reg(vcpu.Handle, HvReg.PC, address).ThrowOnError();
                }
                else if (reason == HvExitReason.Canceled || reason == HvExitReason.VTimerActivated)
                {
                    if (GetAndClearInterruptRequested())
                    {
                        ReturnToPool(vcpu);
                        InterruptHandler();
                        vcpu = RentFromPool(memoryManager.AddressSpace, vcpu);
                    }

                    if (reason == HvExitReason.VTimerActivated)
                    {
                        vcpu.EnableAndUpdateVTimer();

                        // Unmask VTimer interrupts.
                        HvApi.hv_vcpu_set_vtimer_mask(vcpu.Handle, false).ThrowOnError();
                    }
                }
                else
                {
                    throw new Exception($"Unhandled exit reason {reason}.");
                }
            }

            HvVcpuPool.Instance.Destroy(vcpu, SwapContext);
        }

        private ulong SynchronousException(HvMemoryManager memoryManager, ref HvVcpu vcpu)
        {
            ulong vcpuHandle = vcpu.Handle;

            HvApi.hv_vcpu_get_sys_reg(vcpuHandle, HvSysReg.ELR_EL1, out ulong elr).ThrowOnError();
            HvApi.hv_vcpu_get_sys_reg(vcpuHandle, HvSysReg.ESR_EL1, out ulong esr).ThrowOnError();

            ExceptionClass ec = (ExceptionClass)((uint)esr >> 26);

            switch (ec)
            {
                case ExceptionClass.DataAbortLowerEl:
                    DataAbort(memoryManager.Tracking, vcpuHandle, (uint)esr);
                    break;
                case ExceptionClass.TrappedMsrMrsSystem:
                    InstructionTrap((uint)esr);
                    HvApi.hv_vcpu_set_sys_reg(vcpuHandle, HvSysReg.ELR_EL1, elr + 4UL).ThrowOnError();
                    break;
                case ExceptionClass.SvcAarch64:
                    ReturnToPool(vcpu);
                    ushort id = (ushort)esr;
                    SupervisorCallHandler(elr - 4UL, id);
                    vcpu = RentFromPool(memoryManager.AddressSpace, vcpu);
                    break;
                default:
                    throw new Exception($"Unhandled guest exception {ec}.");
            }

            // Make sure we will continue running at EL0.
            if (memoryManager.AddressSpace.GetAndClearUserTlbInvalidationPending())
            {
                // TODO: Invalidate only the range that was modified?
                return HvAddressSpace.KernelRegionTlbiEretAddress;
            }
            else
            {
                return HvAddressSpace.KernelRegionEretAddress;
            }
        }

        private static void DataAbort(MemoryTracking tracking, ulong vcpu, uint esr)
        {
            bool write = (esr & (1u << 6)) != 0;
            bool farValid = (esr & (1u << 10)) == 0;
            int accessSizeLog2 = (int)((esr >> 22) & 3);

            if (farValid)
            {
                HvApi.hv_vcpu_get_sys_reg(vcpu, HvSysReg.FAR_EL1, out ulong far).ThrowOnError();

                ulong size = 1UL << accessSizeLog2;

                if (!tracking.VirtualMemoryEvent(far, size, write))
                {
                    string rw = write ? "write" : "read";
                    throw new Exception($"Unhandled invalid memory access at VA 0x{far:X} with size 0x{size:X} ({rw}).");
                }
            }
            else
            {
                throw new Exception($"Unhandled invalid memory access at unknown VA with ESR 0x{esr:X}.");
            }
        }

        private void InstructionTrap(uint esr)
        {
            bool read = (esr & 1) != 0;
            uint rt = (esr >> 5) & 0x1f;

            if (read)
            {
                // Op0 Op2 Op1 CRn 00000 CRm
                switch ((esr >> 1) & 0x1ffe0f)
                {
                    case 0b11_000_011_1110_00000_0000: // CNTFRQ_EL0
                        WriteRt(rt, _counter.Frequency);
                        break;
                    case 0b11_001_011_1110_00000_0000: // CNTPCT_EL0
                        WriteRt(rt, _counter.Counter);
                        break;
                    default:
                        throw new Exception($"Unhandled system register read with ESR 0x{esr:X}");
                }
            }
            else
            {
                throw new Exception($"Unhandled system register write with ESR 0x{esr:X}");
            }
        }

        private void WriteRt(uint rt, ulong value)
        {
            if (rt < 31)
            {
                SetX((int)rt, value);
            }
        }

        private void ReturnToPool(HvVcpu vcpu)
        {
            HvVcpuPool.Instance.Return(vcpu, SwapContext);
        }

        private HvVcpu RentFromPool(HvAddressSpace addressSpace, HvVcpu vcpu)
        {
            return HvVcpuPool.Instance.Rent(addressSpace, _shadowContext, vcpu, SwapContext);
        }

        private void SwapContext(IHvExecutionContext newContext)
        {
            _impl = newContext;
        }

        public void Dispose()
        {
        }
    }
}
