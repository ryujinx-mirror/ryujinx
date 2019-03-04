namespace Ryujinx.Graphics.Gal.Shader
{
    class ShaderIrMetaIpa : ShaderIrMeta
    {
        public ShaderIpaMode Mode { get; private set; }

        public ShaderIrMetaIpa(ShaderIpaMode mode)
        {
            Mode = mode;
        }
    }
}