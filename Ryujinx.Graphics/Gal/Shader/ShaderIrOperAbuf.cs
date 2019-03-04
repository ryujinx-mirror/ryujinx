namespace Ryujinx.Graphics.Gal.Shader
{
    class ShaderIrOperAbuf : ShaderIrNode
    {
        public int Offs { get; private set; }

        public ShaderIrNode Vertex { get; private set; }

        public ShaderIrOperAbuf(int offs, ShaderIrNode vertex)
        {
            Offs   = offs;
            Vertex = vertex;
        }
    }
}