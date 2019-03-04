namespace Ryujinx.Graphics.Gal.Shader
{
    class ShaderIrOperImm : ShaderIrNode
    {
        public int Value { get; private set; }

        public ShaderIrOperImm(int value)
        {
            Value = value;
        }
    }
}