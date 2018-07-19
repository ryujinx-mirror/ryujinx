namespace Ryujinx.Graphics.Gal.Shader
{
    class ShaderIrOperAbuf : ShaderIrNode
    {
        public int Offs { get; private set; }

        public ShaderIrNode Vertex { get; private set; }

        public ShaderIrOperAbuf(int Offs, ShaderIrNode Vertex)
        {
            this.Offs   = Offs;
            this.Vertex = Vertex;
        }
    }
}