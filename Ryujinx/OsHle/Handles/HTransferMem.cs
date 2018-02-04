using ChocolArm64.Memory;

namespace Ryujinx.OsHle.Handles
{
    class HTransferMem
    {
        public AMemory     Memory { get; private set; }
        public AMemoryPerm Perm   { get; private set; }

        public long Position { get; private set; }
        public long Size     { get; private set; }
        public long PhysPos  { get; private set; }

        public HTransferMem(AMemory Memory, AMemoryPerm Perm, long Position, long Size, long PhysPos)
        {
            this.Memory   = Memory;
            this.Perm     = Perm;
            this.Position = Position;
            this.Size     = Size;
            this.PhysPos  = PhysPos;
        }
    }
}