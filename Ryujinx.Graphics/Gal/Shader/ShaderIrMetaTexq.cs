namespace Ryujinx.Graphics.Gal.Shader
{
    class ShaderIrMetaTexq : ShaderIrMeta
    {
        public ShaderTexqInfo Info { get; private set; }

        public int Elem { get; private set; }

        public ShaderIrMetaTexq(ShaderTexqInfo info, int elem)
        {
            Info = info;
            Elem = elem;
        }
    }
}