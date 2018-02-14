using ChocolArm64;

namespace Ryujinx.OsHle.Handles
{
    class HThread
    {
        public AThread Thread { get; private set; }

        public int ProcessorId { get; private set; }
        public int Priority    { get; private set; }

        public int ThreadId => Thread.ThreadId;

        public HThread(AThread Thread, int ProcessorId, int Priority)
        {
            this.Thread      = Thread;
            this.ProcessorId = ProcessorId;
            this.Priority    = Priority;
        }
    }
}