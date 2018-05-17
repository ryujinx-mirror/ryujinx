namespace Ryujinx.Graphics.Gal.Shader
{
    class ShaderIrOperCbuf : ShaderIrNode
    {
        public int Index { get; private set; }
        public int Pos   { get; set; }

        public ShaderIrNode Offs { get; private set; }

        public ShaderIrOperCbuf(int Index, int Pos, ShaderIrNode Offs = null)
        {
            this.Index = Index;
            this.Pos   = Pos;
            this.Offs  = Offs;
        }
    }
}