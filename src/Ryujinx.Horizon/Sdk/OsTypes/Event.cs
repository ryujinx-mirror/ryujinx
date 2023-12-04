using System;
using System.Collections.Generic;

namespace Ryujinx.Horizon.Sdk.OsTypes
{
    class Event : IDisposable
    {
        private EventType _event;

        public object EventLock => _event.Lock;
        public LinkedList<MultiWaitHolderBase> MultiWaitHolders => _event.MultiWaitHolders;

        public Event(EventClearMode clearMode)
        {
            Os.InitializeEvent(out _event, signaled: false, clearMode);
        }

        public TriBool IsSignaledThreadUnsafe()
        {
            return _event.Signaled ? TriBool.True : TriBool.False;
        }

        public void Wait()
        {
            Os.WaitEvent(ref _event);
        }

        public bool TryWait()
        {
            return Os.TryWaitEvent(ref _event);
        }

        public bool TimedWait(TimeSpan timeout)
        {
            return Os.TimedWaitEvent(ref _event, timeout);
        }

        public void Signal()
        {
            Os.SignalEvent(ref _event);
        }

        public void Clear()
        {
            Os.ClearEvent(ref _event);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Os.FinalizeEvent(ref _event);
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
