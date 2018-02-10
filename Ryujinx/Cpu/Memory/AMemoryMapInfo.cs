namespace ChocolArm64.Memory
{
    public struct AMemoryMapInfo
    {
        public long Position { get; private set; }
        public long Size     { get; private set; }
        public int  Type     { get; private set; }
        public int  Attr     { get; private set; }

        public AMemoryPerm Perm { get; private set; }

        public AMemoryMapInfo(long Position, long Size, int Type, int Attr, AMemoryPerm Perm)
        {
            this.Position = Position;
            this.Size     = Size;
            this.Type     = Type;
            this.Attr     = Attr;
            this.Perm     = Perm;
        }
    }
}