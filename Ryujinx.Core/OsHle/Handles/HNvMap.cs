namespace Ryujinx.Core.OsHle.Handles
{
    class HNvMap
    {
        public int Id    { get; private set; }
        public int Size  { get; private set; }

        public int  Align   { get; set; }
        public int  Kind    { get; set; }
        public long Address { get; set; }
        
        public HNvMap(int Id, int Size)
        {
            this.Id   = Id;
            this.Size = Size;
        }
    }
}