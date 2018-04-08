namespace Ryujinx.Graphics.Gal.Shader
{
    class ShaderIrOperCbuf : ShaderIrNode
    {
        public int Index { get; private set; }
        public int Offs  { get; private set; }

        public ShaderIrOperCbuf(int Index, int Offs)
        {
            this.Index = Index;
            this.Offs  = Offs;
        }
    }
}