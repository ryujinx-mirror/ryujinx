using ChocolArm64;

namespace Ryujinx.Core.OsHle.Handles
{
    class KThread : KSynchronizationObject
    {
        public AThread Thread { get; private set; }

        public int ProcessorId { get; private set; }
        public int Priority    { get; set; }

        public int ThreadId => Thread.ThreadId;

        public KThread(AThread Thread, int ProcessorId, int Priority)
        {
            this.Thread      = Thread;
            this.ProcessorId = ProcessorId;
            this.Priority    = Priority;
        }
    }
}