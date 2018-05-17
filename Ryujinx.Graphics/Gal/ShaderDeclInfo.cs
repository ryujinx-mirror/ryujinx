namespace Ryujinx.Graphics.Gal
{
    public class ShaderDeclInfo
    {
        public string Name { get; private set; }

        public int Index { get; private set; }
        public int Cbuf  { get; private set; }
        public int Size  { get; private set; }

        public ShaderDeclInfo(string Name, int Index, int Cbuf = 0, int Size = 1)
        {
            this.Name  = Name;
            this.Index = Index;
            this.Cbuf  = Cbuf;
            this.Size  = Size;
        }

        internal void Enlarge(int NewSize)
        {
            if (Size < NewSize)
            {
                Size = NewSize;
            }
        }

        internal void SetCbufOffs(int Offs)
        {
            if (Index < Offs)
            {
                Index = Offs;
            }
        }
    }
}