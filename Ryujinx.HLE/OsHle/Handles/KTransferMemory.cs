namespace Ryujinx.HLE.OsHle.Handles
{
    class KTransferMemory
    {
        public long Position { get; private set; }
        public long Size     { get; private set; }

        public KTransferMemory(long Position, long Size)
        {
            this.Position = Position;
            this.Size     = Size;
        }
    }
}