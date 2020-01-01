namespace Ryujinx.Graphics
{
    struct ChCommand
    {
        public ChClassId ClassId { get; private set; }

        public int MethodOffset { get; private set; }

        public int[] Arguments { get; private set; }

        public ChCommand(ChClassId classId, int methodOffset, params int[] arguments)
        {
            ClassId      = classId;
            MethodOffset = methodOffset;
            Arguments    = arguments;
        }
    }
}