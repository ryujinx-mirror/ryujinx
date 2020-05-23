using ARMeilleure.Memory;
using ARMeilleure.State;
using ARMeilleure.Translation;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace ARMeilleure.Instructions
{
    static class NativeInterface
    {
        private const int ErgSizeLog2 = 4;

        private class ThreadContext
        {
            public State.ExecutionContext Context { get; }
            public IMemoryManager Memory { get; }
            public Translator Translator { get; }

            public ulong ExclusiveAddress { get; set; }
            public ulong ExclusiveValueLow { get; set; }
            public ulong ExclusiveValueHigh { get; set; }

            public ThreadContext(State.ExecutionContext context, IMemoryManager memory, Translator translator)
            {
                Context = context;
                Memory = memory;
                Translator = translator;

                ExclusiveAddress = ulong.MaxValue;
            }
        }

        [ThreadStatic]
        private static ThreadContext _context;

        public static void RegisterThread(State.ExecutionContext context, IMemoryManager memory, Translator translator)
        {
            _context = new ThreadContext(context, memory, translator);
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
            var context = GetContext();

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

        public static ulong GetCntvctEl0()
        {
            return GetContext().CntvctEl0;
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
            var context = GetContext();

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
            return GetMemoryManager().Read<byte>(address);
        }

        public static ushort ReadUInt16(ulong address)
        {
            return GetMemoryManager().Read<ushort>(address);
        }

        public static uint ReadUInt32(ulong address)
        {
            return GetMemoryManager().Read<uint>(address);
        }

        public static ulong ReadUInt64(ulong address)
        {
            return GetMemoryManager().Read<ulong>(address);
        }

        public static V128 ReadVector128(ulong address)
        {
            return GetMemoryManager().Read<V128>(address);
        }
        #endregion

        #region "Read exclusive"
        public static byte ReadByteExclusive(ulong address)
        {
            byte value = _context.Memory.Read<byte>(address);

            _context.ExclusiveAddress = GetMaskedExclusiveAddress(address);
            _context.ExclusiveValueLow = value;
            _context.ExclusiveValueHigh = 0;

            return value;
        }

        public static ushort ReadUInt16Exclusive(ulong address)
        {
            ushort value = _context.Memory.Read<ushort>(address);

            _context.ExclusiveAddress = GetMaskedExclusiveAddress(address);
            _context.ExclusiveValueLow = value;
            _context.ExclusiveValueHigh = 0;

            return value;
        }

        public static uint ReadUInt32Exclusive(ulong address)
        {
            uint value = _context.Memory.Read<uint>(address);

            _context.ExclusiveAddress = GetMaskedExclusiveAddress(address);
            _context.ExclusiveValueLow = value;
            _context.ExclusiveValueHigh = 0;

            return value;
        }

        public static ulong ReadUInt64Exclusive(ulong address)
        {
            ulong value = _context.Memory.Read<ulong>(address);

            _context.ExclusiveAddress = GetMaskedExclusiveAddress(address);
            _context.ExclusiveValueLow = value;
            _context.ExclusiveValueHigh = 0;

            return value;
        }

        public static V128 ReadVector128Exclusive(ulong address)
        {
            V128 value = MemoryManagerPal.AtomicLoad128(ref _context.Memory.GetRef<V128>(address));

            _context.ExclusiveAddress = GetMaskedExclusiveAddress(address);
            _context.ExclusiveValueLow = value.Extract<ulong>(0);
            _context.ExclusiveValueHigh = value.Extract<ulong>(1);

            return value;
        }
        #endregion

        #region "Write"
        public static void WriteByte(ulong address, byte value)
        {
            GetMemoryManager().Write(address, value);
        }

        public static void WriteUInt16(ulong address, ushort value)
        {
            GetMemoryManager().Write(address, value);
        }

        public static void WriteUInt32(ulong address, uint value)
        {
            GetMemoryManager().Write(address, value);
        }

        public static void WriteUInt64(ulong address, ulong value)
        {
            GetMemoryManager().Write(address, value);
        }

        public static void WriteVector128(ulong address, V128 value)
        {
            GetMemoryManager().Write(address, value);
        }
        #endregion

        #region "Write exclusive"
        public static int WriteByteExclusive(ulong address, byte value)
        {
            bool success = _context.ExclusiveAddress == GetMaskedExclusiveAddress(address);

            if (success)
            {
                ref int valueRef = ref _context.Memory.GetRefNoChecks<int>(address);

                int currentValue = valueRef;

                byte expected = (byte)_context.ExclusiveValueLow;

                int expected32 = (currentValue & ~byte.MaxValue) | expected;
                int desired32 = (currentValue & ~byte.MaxValue) | value;

                success = Interlocked.CompareExchange(ref valueRef, desired32, expected32) == expected32;

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
                ref int valueRef = ref _context.Memory.GetRefNoChecks<int>(address);

                int currentValue = valueRef;

                ushort expected = (ushort)_context.ExclusiveValueLow;

                int expected32 = (currentValue & ~ushort.MaxValue) | expected;
                int desired32 = (currentValue & ~ushort.MaxValue) | value;

                success = Interlocked.CompareExchange(ref valueRef, desired32, expected32) == expected32;

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
                ref int valueRef = ref _context.Memory.GetRef<int>(address);

                success = Interlocked.CompareExchange(ref valueRef, (int)value, (int)_context.ExclusiveValueLow) == (int)_context.ExclusiveValueLow;

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
                ref long valueRef = ref _context.Memory.GetRef<long>(address);

                success = Interlocked.CompareExchange(ref valueRef, (long)value, (long)_context.ExclusiveValueLow) == (long)_context.ExclusiveValueLow;

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

                ref V128 location = ref _context.Memory.GetRef<V128>(address);

                success = MemoryManagerPal.CompareAndSwap128(ref location, expected, value) == expected;

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

        public static ulong GetFunctionAddress(ulong address)
        {
            TranslatedFunction function = _context.Translator.GetOrTranslate(address, GetContext().ExecutionMode);
            return (ulong)function.GetPointer().ToInt64();
        }

        public static ulong GetIndirectFunctionAddress(ulong address, ulong entryAddress)
        {
            TranslatedFunction function = _context.Translator.GetOrTranslate(address, GetContext().ExecutionMode);
            ulong ptr = (ulong)function.GetPointer().ToInt64();
            if (function.HighCq)
            {
                // Rewrite the host function address in the table to point to the highCq function.
                Marshal.WriteInt64((IntPtr)entryAddress, 8, (long)ptr);
            }
            return ptr;
        }

        public static void ClearExclusive()
        {
            _context.ExclusiveAddress = ulong.MaxValue;
        }

        public static bool CheckSynchronization()
        {
            Statistics.PauseTimer();

            var context = GetContext();

            context.CheckInterrupt();

            Statistics.ResumeTimer();

            return context.Running;
        }

        public static State.ExecutionContext GetContext()
        {
            return _context.Context;
        }

        public static IMemoryManager GetMemoryManager()
        {
            return _context.Memory;
        }
    }
}