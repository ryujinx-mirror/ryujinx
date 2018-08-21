namespace Ryujinx.Graphics.Gal.Shader
{
    class ShaderIrOperGpr : ShaderIrNode
    {
        public const int ZRIndex = 0xff;

        public bool IsConst => Index == ZRIndex;

        public bool IsValidRegister => (Index <= ZRIndex);

        public int Index { get; set; }

        public ShaderIrOperGpr(int Index)
        {
            this.Index = Index;
        }

        public static ShaderIrOperGpr MakeTemporary(int Index = 0)
        {
            return new ShaderIrOperGpr(0x100 + Index);
        }
    }
}