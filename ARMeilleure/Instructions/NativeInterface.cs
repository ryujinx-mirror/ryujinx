using ARMeilleure.Memory;
using ARMeilleure.State;
using System;

namespace ARMeilleure.Instructions
{
    static class NativeInterface
    {
        private const int ErgSizeLog2 = 4;

        private class ThreadContext
        {
            public ExecutionContext Context { get; }
            public MemoryManager    Memory  { get; }

            public ulong ExclusiveAddress   { get; set; }
            public ulong ExclusiveValueLow  { get; set; }
            public ulong ExclusiveValueHigh { get; set; }

            public ThreadContext(ExecutionContext context, MemoryManager memory)
            {
                Context = context;
                Memory  = memory;

                ExclusiveAddress = ulong.MaxValue;
            }
        }

        [ThreadStatic]
        private static ThreadContext _context;

        public static void RegisterThread(ExecutionContext context, MemoryManager memory)
        {
            _context = new ThreadContext(context, memory);
        }

        public static void UnregisterThread()
        {
            _context = null;
        }

        public static void Break(ulong address, int imm)
        {
            Statistics.PauseTimer();

            GetContext().OnBreak(address, imm);

            Statistics.ResumeTimer();
        }

        public static void SupervisorCall(ulong address, int imm)
        {
            Statistics.PauseTimer();

            GetContext().OnSupervisorCall(address, imm);

            Statistics.ResumeTimer();
        }

        public static void Undefined(ulong address, int opCode)
        {
            Statistics.PauseTimer();

            GetContext().OnUndefined(address, opCode);

            Statistics.ResumeTimer();
        }

#region "System registers"
        public static ulong GetCtrEl0()
        {
            return (ulong)GetContext().CtrEl0;
        }

        public static ulong GetDczidEl0()
        {
            return (ulong)GetContext().DczidEl0;
        }

        public static ulong GetFpcr()
        {
            return (ulong)GetContext().Fpcr;
        }

        public static ulong GetFpsr()
        {
            return (ulong)GetContext().Fpsr;
        }

        public static uint GetFpscr()
        {
            ExecutionContext context = GetContext();
            uint result = (uint)(context.Fpsr & FPSR.A32Mask) | (uint)(context.Fpcr & FPCR.A32Mask);

            result |= context.GetFPstateFlag(FPState.NFlag) ? (1u << 31) : 0;
            result |= context.GetFPstateFlag(FPState.ZFlag) ? (1u << 30) : 0;
            result |= context.GetFPstateFlag(FPState.CFlag) ? (1u << 29) : 0;
            result |= context.GetFPstateFlag(FPState.VFlag) ? (1u << 28) : 0;

            return result;
        }

        public static ulong GetTpidrEl0()
        {
            return (ulong)GetContext().TpidrEl0;
        }

        public static uint GetTpidrEl032()
        {
            return (uint)GetContext().TpidrEl0;
        }

        public static ulong GetTpidr()
        {
            return (ulong)GetContext().Tpidr;
        }

        public static uint GetTpidr32()
        {
            return (uint)GetContext().Tpidr;
        }

        public static ulong GetCntfrqEl0()
        {
            return GetContext().CntfrqEl0;
        }

        public static ulong GetCntpctEl0()
        {
            return GetContext().CntpctEl0;
        }

        public static void SetFpcr(ulong value)
        {
            GetContext().Fpcr = (FPCR)value;
        }

        public static void SetFpsr(ulong value)
        {
            GetContext().Fpsr = (FPSR)value;
        }

        public static void SetFpscr(uint value)
        {
            ExecutionContext context = GetContext();

            context.SetFPstateFlag(FPState.NFlag, (value & (1u << 31)) != 0);
            context.SetFPstateFlag(FPState.ZFlag, (value & (1u << 30)) != 0);
            context.SetFPstateFlag(FPState.CFlag, (value & (1u << 29)) != 0);
            context.SetFPstateFlag(FPState.VFlag, (value & (1u << 28)) != 0);

            context.Fpsr = FPSR.A32Mask & (FPSR)value;
            context.Fpcr = FPCR.A32Mask & (FPCR)value;
        }

        public static void SetTpidrEl0(ulong value)
        {
            GetContext().TpidrEl0 = (long)value;
        }

        public static void SetTpidrEl032(uint value)
        {
            GetContext().TpidrEl0 = (long)value;
        }
        #endregion

        #region "Read"
        public static byte ReadByte(ulong address)
        {
            return GetMemoryManager().ReadByte((long)address);
        }

        public static ushort ReadUInt16(ulong address)
        {
            return GetMemoryManager().ReadUInt16((long)address);
        }

        public static uint ReadUInt32(ulong address)
        {
            return GetMemoryManager().ReadUInt32((long)address);
        }

        public static ulong ReadUInt64(ulong address)
        {
            return GetMemoryManager().ReadUInt64((long)address);
        }

        public static V128 ReadVector128(ulong address)
        {
            return GetMemoryManager().ReadVector128((long)address);
        }
#endregion

#region "Read exclusive"
        public static byte ReadByteExclusive(ulong address)
        {
            byte value = _context.Memory.ReadByte((long)address);

            _context.ExclusiveAddress   = GetMaskedExclusiveAddress(address);
            _context.ExclusiveValueLow  = value;
            _context.ExclusiveValueHigh = 0;

            return value;
        }

        public static ushort ReadUInt16Exclusive(ulong address)
        {
            ushort value = _context.Memory.ReadUInt16((long)address);

            _context.ExclusiveAddress   = GetMaskedExclusiveAddress(address);
            _context.ExclusiveValueLow  = value;
            _context.ExclusiveValueHigh = 0;

            return value;
        }

        public static uint ReadUInt32Exclusive(ulong address)
        {
            uint value = _context.Memory.ReadUInt32((long)address);

            _context.ExclusiveAddress   = GetMaskedExclusiveAddress(address);
            _context.ExclusiveValueLow  = value;
            _context.ExclusiveValueHigh = 0;

            return value;
        }

        public static ulong ReadUInt64Exclusive(ulong address)
        {
            ulong value = _context.Memory.ReadUInt64((long)address);

            _context.ExclusiveAddress   = GetMaskedExclusiveAddress(address);
            _context.ExclusiveValueLow  = value;
            _context.ExclusiveValueHigh = 0;

            return value;
        }

        public static V128 ReadVector128Exclusive(ulong address)
        {
            V128 value = _context.Memory.AtomicLoadInt128((long)address);

            _context.ExclusiveAddress   = GetMaskedExclusiveAddress(address);
            _context.ExclusiveValueLow  = value.GetUInt64(0);
            _context.ExclusiveValueHigh = value.GetUInt64(1);

            return value;
        }
#endregion

#region "Write"
        public static void WriteByte(ulong address, byte value)
        {
            GetMemoryManager().WriteByte((long)address, value);
        }

        public static void WriteUInt16(ulong address, ushort value)
        {
            GetMemoryManager().WriteUInt16((long)address, value);
        }

        public static void WriteUInt32(ulong address, uint value)
        {
            GetMemoryManager().WriteUInt32((long)address, value);
        }

        public static void WriteUInt64(ulong address, ulong value)
        {
            GetMemoryManager().WriteUInt64((long)address, value);
        }

        public static void WriteVector128(ulong address, V128 value)
        {
            GetMemoryManager().WriteVector128((long)address, value);
        }
#endregion

#region "Write exclusive"
        public static int WriteByteExclusive(ulong address, byte value)
        {
            bool success = _context.ExclusiveAddress == GetMaskedExclusiveAddress(address);

            if (success)
            {
                success = _context.Memory.AtomicCompareExchangeByte(
                    (long)address,
                    (byte)_context.ExclusiveValueLow,
                    (byte)value);

                if (success)
                {
                    ClearExclusive();
                }
            }

            return success ? 0 : 1;
        }

        public static int WriteUInt16Exclusive(ulong address, ushort value)
        {
            bool success = _context.ExclusiveAddress == GetMaskedExclusiveAddress(address);

            if (success)
            {
                success = _context.Memory.AtomicCompareExchangeInt16(
                    (long)address,
                    (short)_context.ExclusiveValueLow,
                    (short)value);

                if (success)
                {
                    ClearExclusive();
                }
            }

            return success ? 0 : 1;
        }

        public static int WriteUInt32Exclusive(ulong address, uint value)
        {
            bool success = _context.ExclusiveAddress == GetMaskedExclusiveAddress(address);

            if (success)
            {
                success = _context.Memory.AtomicCompareExchangeInt32(
                    (long)address,
                    (int)_context.ExclusiveValueLow,
                    (int)value);

                if (success)
                {
                    ClearExclusive();
                }
            }

            return success ? 0 : 1;
        }

        public static int WriteUInt64Exclusive(ulong address, ulong value)
        {
            bool success = _context.ExclusiveAddress == GetMaskedExclusiveAddress(address);

            if (success)
            {
                success = _context.Memory.AtomicCompareExchangeInt64(
                    (long)address,
                    (long)_context.ExclusiveValueLow,
                    (long)value);

                if (success)
                {
                    ClearExclusive();
                }
            }

            return success ? 0 : 1;
        }

        public static int WriteVector128Exclusive(ulong address, V128 value)
        {
            bool success = _context.ExclusiveAddress == GetMaskedExclusiveAddress(address);

            if (success)
            {
                V128 expected = new V128(_context.ExclusiveValueLow, _context.ExclusiveValueHigh);

                success = _context.Memory.AtomicCompareExchangeInt128((long)address, expected, value);

                if (success)
                {
                    ClearExclusive();
                }
            }

            return success ? 0 : 1;
        }
#endregion

        private static ulong GetMaskedExclusiveAddress(ulong address)
        {
            return address & ~((4UL << ErgSizeLog2) - 1);
        }

        public static void ClearExclusive()
        {
            _context.ExclusiveAddress = ulong.MaxValue;
        }

        public static void CheckSynchronization()
        {
            Statistics.PauseTimer();

            GetContext().CheckInterrupt();

            Statistics.ResumeTimer();
        }

        public static ExecutionContext GetContext()
        {
            return _context.Context;
        }

        public static MemoryManager GetMemoryManager()
        {
            return _context.Memory;
        }
    }
}