namespace Ryujinx.Graphics.Gal.Shader
{
    class ShaderIrOperGpr : ShaderIrNode
    {
        public const int ZRIndex = 0xff;

        public bool IsConst => Index == ZRIndex;

        public bool IsValidRegister => (uint)Index <= ZRIndex;

        public int Index    { get; set; }
        public int HalfPart { get; set; }

        public ShaderRegisterSize RegisterSize { get; private set; }

        public ShaderIrOperGpr(int Index)
        {
            this.Index = Index;

            RegisterSize = ShaderRegisterSize.Single;
        }

        public ShaderIrOperGpr(int Index, int HalfPart)
        {
            this.Index    = Index;
            this.HalfPart = HalfPart;

            RegisterSize = ShaderRegisterSize.Half;
        }

        public static ShaderIrOperGpr MakeTemporary(int Index = 0)
        {
            return new ShaderIrOperGpr(0x100 + Index);
        }
    }
}