namespace Ryujinx.Graphics.Gal.Shader
{
    class ShaderIrOperAbuf : ShaderIrNode
    {
        public int Offs     { get; private set; }
        public int GprIndex { get; private set; }

        public ShaderIrOperAbuf(int Offs, int GprIndex)
        {
            this.Offs     = Offs;
            this.GprIndex = GprIndex;
        }
    }
}