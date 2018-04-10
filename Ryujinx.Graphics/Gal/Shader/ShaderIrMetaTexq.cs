namespace Ryujinx.Graphics.Gal.Shader
{
    class ShaderIrMetaTexq : ShaderIrMeta
    {
        public ShaderTexqInfo Info { get; private set; }

        public int Elem { get; private set; }

        public ShaderIrMetaTexq(ShaderTexqInfo Info, int Elem)
        {
            this.Info = Info;
            this.Elem = Elem;
        }
    }
}