using ChocolArm64;

namespace Ryujinx.Core.OsHle.Handles
{
    public class HThread
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