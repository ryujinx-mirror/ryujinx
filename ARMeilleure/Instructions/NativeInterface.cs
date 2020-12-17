using ARMeilleure.Memory;
using ARMeilleure.State;
using ARMeilleure.Translation;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ARMeilleure.Instructions
{
    static class NativeInterface
    {
        private class ThreadContext
        {
            public ExecutionContext Context { get; }
            public IMemoryManager Memory { get; }
            public Translator Translator { get; }

            public ThreadContext(ExecutionContext context, IMemoryManager memory, Translator translator)
            {
                Context = context;
                Memory = memory;
                Translator = translator;
            }
        }

        [ThreadStatic]
        private static ThreadContext Context;

        public static void RegisterThread(ExecutionContext context, IMemoryManager memory, Translator translator)
        {
            Context = new ThreadContext(context, memory, translator);
        }

        public static void UnregisterThread()
        {
            Context = null;
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

        public static bool GetFpcrFz()
        {
            return (GetContext().Fpcr & FPCR.Fz) != 0;
        }

        public static ulong GetFpsr()
        {
            return (ulong)GetContext().Fpsr;
        }

        public static uint GetFpscr()
        {
            ExecutionContext context = GetContext();

            return (uint)(context.Fpsr & FPSR.A32Mask & ~FPSR.Nzcv) |
                   (uint)(context.Fpcr & FPCR.A32Mask);
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

        public static void SetFpsrQc()
        {
            GetContext().Fpsr |= FPSR.Qc;
        }

        public static void SetFpscr(uint fpscr)
        {
            ExecutionContext context = GetContext();

            context.Fpsr = FPSR.A32Mask & (FPSR)fpscr;
            context.Fpcr = FPCR.A32Mask & (FPCR)fpscr;
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
            return GetMemoryManager().ReadTracked<byte>(address);
        }

        public static ushort ReadUInt16(ulong address)
        {
            return GetMemoryManager().ReadTracked<ushort>(address);
        }

        public static uint ReadUInt32(ulong address)
        {
            return GetMemoryManager().ReadTracked<uint>(address);
        }

        public static ulong ReadUInt64(ulong address)
        {
            return GetMemoryManager().ReadTracked<ulong>(address);
        }

        public static V128 ReadVector128(ulong address)
        {
            return GetMemoryManager().ReadTracked<V128>(address);
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

        public static void SignalMemoryTracking(ulong address, ulong size, bool write)
        {
            GetMemoryManager().SignalMemoryTracking(address, size, write);
        }

        public static void ThrowInvalidMemoryAccess(ulong address)
        {
            throw new InvalidAccessException(address);
        }

        public static ulong GetFunctionAddress(ulong address)
        {
            return GetFunctionAddressWithHint(address, true);
        }

        public static ulong GetFunctionAddressWithoutRejit(ulong address)
        {
            return GetFunctionAddressWithHint(address, false);
        }

        private static ulong GetFunctionAddressWithHint(ulong address, bool hintRejit)
        {
            TranslatedFunction function = Context.Translator.GetOrTranslate(address, GetContext().ExecutionMode, hintRejit);

            return (ulong)function.FuncPtr.ToInt64();
        }

        public static ulong GetIndirectFunctionAddress(ulong address, ulong entryAddress)
        {
            TranslatedFunction function = Context.Translator.GetOrTranslate(address, GetContext().ExecutionMode, hintRejit: true);

            ulong ptr = (ulong)function.FuncPtr.ToInt64();

            if (function.HighCq)
            {
                Debug.Assert(Context.Translator.JumpTable.CheckEntryFromAddressDynamicTable((IntPtr)entryAddress));

                // Rewrite the host function address in the table to point to the highCq function.
                Marshal.WriteInt64((IntPtr)entryAddress, 8, (long)ptr);
            }

            return ptr;
        }

        public static bool CheckSynchronization()
        {
            Statistics.PauseTimer();

            ExecutionContext context = GetContext();

            context.CheckInterrupt();

            Statistics.ResumeTimer();

            return context.Running;
        }

        public static ExecutionContext GetContext()
        {
            return Context.Context;
        }

        public static IMemoryManager GetMemoryManager()
        {
            return Context.Memory;
        }
    }
}