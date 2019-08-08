using System;

namespace ARMeilleure.State
{
    public interface IExecutionContext : IDisposable
    {
        uint CtrEl0   { get; }
        uint DczidEl0 { get; }

        ulong CntfrqEl0 { get; set; }
        ulong CntpctEl0 { get; }

        long TpidrEl0 { get; set; }
        long Tpidr    { get; set; }

        FPCR Fpcr { get; set; }
        FPSR Fpsr { get; set; }

        bool IsAarch32 { get; set; }

        bool Running { get; set; }

        event EventHandler<EventArgs>              Interrupt;
        event EventHandler<InstExceptionEventArgs> Break;
        event EventHandler<InstExceptionEventArgs> SupervisorCall;
        event EventHandler<InstUndefinedEventArgs> Undefined;

        ulong GetX(int index);
        void  SetX(int index, ulong value);

        V128 GetV(int index);

        bool GetPstateFlag(PState flag);

        void RequestInterrupt();
    }
}