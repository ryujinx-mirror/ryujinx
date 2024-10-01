using Ryujinx.Horizon.Sdk.OsTypes;
using System;
using System.Threading;

namespace Ryujinx.Horizon.Sdk.Arp.Detail
{
    class ApplicationInstanceManager : IDisposable
    {
        private int _disposalState;

        public SystemEventType SystemEvent;
        public int EventHandle;

        public readonly ApplicationInstance[] Entries = new ApplicationInstance[2];

        public ApplicationInstanceManager()
        {
            Os.CreateSystemEvent(out SystemEvent, EventClearMode.ManualClear, true).AbortOnFailure();

            EventHandle = Os.GetReadableHandleOfSystemEvent(ref SystemEvent);
        }

        public void Dispose()
        {
            if (EventHandle != 0 && Interlocked.Exchange(ref _disposalState, 1) == 0)
            {
                Os.DestroySystemEvent(ref SystemEvent);
            }
        }
    }
}
