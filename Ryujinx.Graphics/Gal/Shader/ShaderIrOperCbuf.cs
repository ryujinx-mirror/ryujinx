namespace Ryujinx.Graphics.Gal.Shader
{
    class ShaderIrOperCbuf : ShaderIrNode
    {
        public int Index { get; private set; }
        public int Pos   { get; set; }

        public ShaderIrNode Offs { get; private set; }

        public ShaderIrOperCbuf(int index, int pos, ShaderIrNode offs = null)
        {
            Index = index;
            Pos   = pos;
            Offs  = offs;
        }
    }
}