namespace Ryujinx.Graphics.Gal.Shader
{
    class ShaderIrOperImm : ShaderIrNode
    {
        public int Value { get; private set; }

        public ShaderIrOperImm(int Value)
        {
            this.Value = Value;
        }
    }
}