using ARMeilleure.State;
using Ryujinx.Memory;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Ryujinx.Cpu.AppleHv
{
    class HvExecutionContextVcpu : IHvExecutionContext
    {
        private static MemoryBlock _setSimdFpRegFuncMem;
        private delegate hv_result_t SetSimdFpReg(ulong vcpu, hv_simd_fp_reg_t reg, in V128 value, IntPtr funcPtr);
        private static SetSimdFpReg _setSimdFpReg;
        private static IntPtr _setSimdFpRegNativePtr;

        static HvExecutionContextVcpu()
        {
            // .NET does not support passing vectors by value, so we need to pass a pointer and use a native
            // function to load the value into a vector register.
            _setSimdFpRegFuncMem = new MemoryBlock(MemoryBlock.GetPageSize());
            _setSimdFpRegFuncMem.Write(0, 0x3DC00040u); // LDR Q0, [X2]
            _setSimdFpRegFuncMem.Write(4, 0xD61F0060u); // BR X3
            _setSimdFpRegFuncMem.Reprotect(0, _setSimdFpRegFuncMem.Size, MemoryPermission.ReadAndExecute);

            _setSimdFpReg = Marshal.GetDelegateForFunctionPointer<SetSimdFpReg>(_setSimdFpRegFuncMem.Pointer);

            if (NativeLibrary.TryLoad(HvApi.LibraryName, out IntPtr hvLibHandle))
            {
                _setSimdFpRegNativePtr = NativeLibrary.GetExport(hvLibHandle, nameof(HvApi.hv_vcpu_set_simd_fp_reg));
            }
        }

        public ulong Pc
        {
            get
            {
                HvApi.hv_vcpu_get_reg(_vcpu, hv_reg_t.HV_REG_PC, out ulong pc).ThrowOnError();
                return pc;
            }
            set
            {
                HvApi.hv_vcpu_set_reg(_vcpu, hv_reg_t.HV_REG_PC, value).ThrowOnError();
            }
        }

        public ulong ElrEl1
        {
            get
            {
                HvApi.hv_vcpu_get_sys_reg(_vcpu, hv_sys_reg_t.HV_SYS_REG_ELR_EL1, out ulong elr).ThrowOnError();
                return elr;
            }
            set
            {
                HvApi.hv_vcpu_set_sys_reg(_vcpu, hv_sys_reg_t.HV_SYS_REG_ELR_EL1, value).ThrowOnError();
            }
        }

        public ulong EsrEl1
        {
            get
            {
                HvApi.hv_vcpu_get_sys_reg(_vcpu, hv_sys_reg_t.HV_SYS_REG_ESR_EL1, out ulong esr).ThrowOnError();
                return esr;
            }
            set
            {
                HvApi.hv_vcpu_set_sys_reg(_vcpu, hv_sys_reg_t.HV_SYS_REG_ESR_EL1, value).ThrowOnError();
            }
        }

        public long TpidrEl0
        {
            get
            {
                HvApi.hv_vcpu_get_sys_reg(_vcpu, hv_sys_reg_t.HV_SYS_REG_TPIDR_EL0, out ulong tpidrEl0).ThrowOnError();
                return (long)tpidrEl0;
            }
            set
            {
                HvApi.hv_vcpu_set_sys_reg(_vcpu, hv_sys_reg_t.HV_SYS_REG_TPIDR_EL0, (ulong)value).ThrowOnError();
            }
        }

        public long TpidrroEl0
        {
            get
            {
                HvApi.hv_vcpu_get_sys_reg(_vcpu, hv_sys_reg_t.HV_SYS_REG_TPIDRRO_EL0, out ulong tpidrroEl0).ThrowOnError();
                return (long)tpidrroEl0;
            }
            set
            {
                HvApi.hv_vcpu_set_sys_reg(_vcpu, hv_sys_reg_t.HV_SYS_REG_TPIDRRO_EL0, (ulong)value).ThrowOnError();
            }
        }

        public uint Pstate
        {
            get
            {
                HvApi.hv_vcpu_get_reg(_vcpu, hv_reg_t.HV_REG_CPSR, out ulong cpsr).ThrowOnError();
                return (uint)cpsr;
            }
            set
            {
                HvApi.hv_vcpu_set_reg(_vcpu, hv_reg_t.HV_REG_CPSR, (ulong)value).ThrowOnError();
            }
        }

        public uint Fpcr
        {
            get
            {
                HvApi.hv_vcpu_get_reg(_vcpu, hv_reg_t.HV_REG_FPCR, out ulong fpcr).ThrowOnError();
                return (uint)fpcr;
            }
            set
            {
                HvApi.hv_vcpu_set_reg(_vcpu, hv_reg_t.HV_REG_FPCR, (ulong)value).ThrowOnError();
            }
        }

        public uint Fpsr
        {
            get
            {
                HvApi.hv_vcpu_get_reg(_vcpu, hv_reg_t.HV_REG_FPSR, out ulong fpsr).ThrowOnError();
                return (uint)fpsr;
            }
            set
            {
                HvApi.hv_vcpu_set_reg(_vcpu, hv_reg_t.HV_REG_FPSR, (ulong)value).ThrowOnError();
            }
        }

        private ulong _vcpu;
        private int _interruptRequested;

        public HvExecutionContextVcpu(ulong vcpu)
        {
            _vcpu = vcpu;
        }

        public ulong GetX(int index)
        {
            if (index == 31)
            {
                HvApi.hv_vcpu_get_sys_reg(_vcpu, hv_sys_reg_t.HV_SYS_REG_SP_EL0, out ulong value).ThrowOnError();
                return value;
            }
            else
            {
                HvApi.hv_vcpu_get_reg(_vcpu, hv_reg_t.HV_REG_X0 + (uint)index, out ulong value).ThrowOnError();
                return value;
            }
        }

        public void SetX(int index, ulong value)
        {
            if (index == 31)
            {
                HvApi.hv_vcpu_set_sys_reg(_vcpu, hv_sys_reg_t.HV_SYS_REG_SP_EL0, value).ThrowOnError();
            }
            else
            {
                HvApi.hv_vcpu_set_reg(_vcpu, hv_reg_t.HV_REG_X0 + (uint)index, value).ThrowOnError();
            }
        }

        public V128 GetV(int index)
        {
            HvApi.hv_vcpu_get_simd_fp_reg(_vcpu, hv_simd_fp_reg_t.HV_SIMD_FP_REG_Q0 + (uint)index, out hv_simd_fp_uchar16_t value).ThrowOnError();
            return new V128(value.Low, value.High);
        }

        public void SetV(int index, V128 value)
        {
            _setSimdFpReg(_vcpu, hv_simd_fp_reg_t.HV_SIMD_FP_REG_Q0 + (uint)index, value, _setSimdFpRegNativePtr).ThrowOnError();
        }

        public void RequestInterrupt()
        {
            if (Interlocked.Exchange(ref _interruptRequested, 1) == 0)
            {
                ulong vcpu = _vcpu;
                HvApi.hv_vcpus_exit(ref vcpu, 1);
            }
        }

        public bool GetAndClearInterruptRequested()
        {
            return Interlocked.Exchange(ref _interruptRequested, 0) != 0;
        }
    }
}