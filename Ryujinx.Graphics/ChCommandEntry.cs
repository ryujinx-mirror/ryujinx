namespace Ryujinx.Graphics
{
    struct ChCommand
    {
        public ChClassId ClassId { get; private set; }

        public int MethodOffset { get; private set; }

        public int[] Arguments { get; private set; }

        public ChCommand(ChClassId ClassId, int MethodOffset, params int[] Arguments)
        {
            this.ClassId      = ClassId;
            this.MethodOffset = MethodOffset;
            this.Arguments    = Arguments;
        }
    }
}