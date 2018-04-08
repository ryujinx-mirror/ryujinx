namespace Ryujinx.Graphics.Gal.Shader
{
    class ShaderIrAsg : ShaderIrNode
    {
        public ShaderIrNode Dst { get; set; }
        public ShaderIrNode Src { get; set; }

        public ShaderIrAsg(ShaderIrNode Dst, ShaderIrNode Src)
        {
            this.Dst = Dst;
            this.Src = Src;
        }
    }
}