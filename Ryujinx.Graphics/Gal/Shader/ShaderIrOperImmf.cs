namespace Ryujinx.Graphics.Gal.Shader
{
    class ShaderIrOperImmf : ShaderIrNode
    {
        public float Value { get; private set; }

        public ShaderIrOperImmf(float Value)
        {
            this.Value = Value;
        }
    }
}