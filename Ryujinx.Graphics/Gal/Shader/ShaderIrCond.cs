namespace Ryujinx.Graphics.Gal.Shader
{
    class ShaderIrCond : ShaderIrNode
    {
        public ShaderIrNode Pred  { get; set; }
        public ShaderIrNode Child { get; set; }

        public ShaderIrCond(ShaderIrNode Pred, ShaderIrNode Child)
        {
            this.Pred  = Pred;
            this.Child = Child;
        }
    }
}