using Ryujinx.Audio.Integration;
using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.OsTypes;
using System;

namespace Ryujinx.Horizon.Sdk.Audio
{
    class AudioEvent : IWritableEvent, IDisposable
    {
        private SystemEventType _systemEvent;
        private readonly IExternalEvent _externalEvent;

        public AudioEvent()
        {
            Os.CreateSystemEvent(out _systemEvent, EventClearMode.ManualClear, interProcess: true);

            // We need to do this because the event will be signalled from a different thread.
            _externalEvent = HorizonStatic.Syscall.GetExternalEvent(Os.GetWritableHandleOfSystemEvent(ref _systemEvent));
        }

        public void Signal()
        {
            _externalEvent.Signal();
        }

        public void Clear()
        {
            _externalEvent.Clear();
        }

        public int GetReadableHandle()
        {
            return Os.GetReadableHandleOfSystemEvent(ref _systemEvent);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Os.DestroySystemEvent(ref _systemEvent);
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
