using ARMeilleure.Memory;
using System;
using System.Diagnostics;

namespace ARMeilleure.State
{
    public class ExecutionContext
    {
        private const int MinCountForCheck = 4000;

        private NativeContext _nativeContext;

        internal IntPtr NativeContextPtr => _nativeContext.BasePtr;

        private bool _interrupted;

        private static Stopwatch _tickCounter;

        private static double _hostTickFreq;

        public uint CtrEl0   => 0x8444c004;
        public uint DczidEl0 => 0x00000004;

        public ulong CntfrqEl0 { get; set; }
        public ulong CntpctEl0
        {
            get
            {
                double ticks = _tickCounter.ElapsedTicks * _hostTickFreq;

                return (ulong)(ticks * CntfrqEl0);
            }
        }

        // CNTVCT_EL0 = CNTPCT_EL0 - CNTVOFF_EL2
        // Since EL2 isn't implemented, CNTVOFF_EL2 = 0
        public ulong CntvctEl0 => CntpctEl0;

        public static TimeSpan ElapsedTime => _tickCounter.Elapsed;
        public static long ElapsedTicks => _tickCounter.ElapsedTicks;
        public static double TickFrequency => _hostTickFreq;

        public long TpidrEl0 { get; set; }
        public long Tpidr    { get; set; }

        public FPCR Fpcr { get; set; }
        public FPSR Fpsr { get; set; }
        public FPCR StandardFpcrValue => (Fpcr & (FPCR.Ahp)) | FPCR.Dn | FPCR.Fz;

        public bool IsAarch32 { get; set; }

        internal ExecutionMode ExecutionMode
        {
            get
            {
                if (IsAarch32)
                {
                    return GetPstateFlag(PState.TFlag)
                        ? ExecutionMode.Aarch32Thumb
                        : ExecutionMode.Aarch32Arm;
                }
                else
                {
                    return ExecutionMode.Aarch64;
                }
            }
        }

        public bool Running { get; private set; }

        public event EventHandler<EventArgs>              Interrupt;
        public event EventHandler<InstExceptionEventArgs> Break;
        public event EventHandler<InstExceptionEventArgs> SupervisorCall;
        public event EventHandler<InstUndefinedEventArgs> Undefined;

        static ExecutionContext()
        {
            _hostTickFreq = 1.0 / Stopwatch.Frequency;

            _tickCounter = new Stopwatch();

            _tickCounter.Start();
        }

        public ExecutionContext(IJitMemoryAllocator allocator)
        {
            _nativeContext = new NativeContext(allocator);

            Running = true;

            _nativeContext.SetCounter(MinCountForCheck);
        }

        public ulong GetX(int index)              => _nativeContext.GetX(index);
        public void  SetX(int index, ulong value) => _nativeContext.SetX(index, value);

        public V128 GetV(int index)             => _nativeContext.GetV(index);
        public void SetV(int index, V128 value) => _nativeContext.SetV(index, value);

        public bool GetPstateFlag(PState flag)             => _nativeContext.GetPstateFlag(flag);
        public void SetPstateFlag(PState flag, bool value) => _nativeContext.SetPstateFlag(flag, value);

        public bool GetFPstateFlag(FPState flag) => _nativeContext.GetFPStateFlag(flag);
        public void SetFPstateFlag(FPState flag, bool value) => _nativeContext.SetFPStateFlag(flag, value);

        internal void CheckInterrupt()
        {
            if (_interrupted)
            {
                _interrupted = false;

                Interrupt?.Invoke(this, EventArgs.Empty);
            }

            _nativeContext.SetCounter(MinCountForCheck);
        }

        public void RequestInterrupt()
        {
            _interrupted = true;
        }

        internal void OnBreak(ulong address, int imm)
        {
            Break?.Invoke(this, new InstExceptionEventArgs(address, imm));
        }

        internal void OnSupervisorCall(ulong address, int imm)
        {
            SupervisorCall?.Invoke(this, new InstExceptionEventArgs(address, imm));
        }

        internal void OnUndefined(ulong address, int opCode)
        {
            Undefined?.Invoke(this, new InstUndefinedEventArgs(address, opCode));
        }

        public void StopRunning()
        {
            Running = false;
            _nativeContext.SetCounter(0);
        }

        public void Dispose()
        {
            _nativeContext.Dispose();
        }
    }
}