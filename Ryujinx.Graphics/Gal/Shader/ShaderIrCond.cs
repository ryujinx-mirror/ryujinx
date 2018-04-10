namespace Ryujinx.Graphics.Gal.Shader
{
    class ShaderIrCond : ShaderIrNode
    {
        public ShaderIrNode Pred  { get; set; }
        public ShaderIrNode Child { get; set; }

        public bool Not { get; private set; }

        public ShaderIrCond(ShaderIrNode Pred, ShaderIrNode Child, bool Not)
        {
            this.Pred  = Pred;
            this.Child = Child;
            this.Not   = Not;
        }
    }
}