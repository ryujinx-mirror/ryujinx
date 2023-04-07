using Ryujinx.Audio.Integration;
using Ryujinx.HLE.HOS.Kernel.Threading;

namespace Ryujinx.HLE.HOS.Services.Audio.AudioRenderer
{
    class AudioKernelEvent : IWritableEvent
    {
        public KEvent Event { get; }

        public AudioKernelEvent(KEvent evnt)
        {
            Event = evnt;
        }

        public void Clear()
        {
            Event.WritableEvent.Clear();
        }

        public void Signal()
        {
            Event.WritableEvent.Signal();
        }
    }
}
