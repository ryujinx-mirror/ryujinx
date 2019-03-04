namespace Ryujinx.Graphics.Gal.Shader
{
    class ShaderIrOperGpr : ShaderIrNode
    {
        public const int ZrIndex = 0xff;

        public bool IsConst => Index == ZrIndex;

        public bool IsValidRegister => (uint)Index <= ZrIndex;

        public int Index    { get; set; }
        public int HalfPart { get; set; }

        public ShaderRegisterSize RegisterSize { get; private set; }

        public ShaderIrOperGpr(int index)
        {
            Index = index;

            RegisterSize = ShaderRegisterSize.Single;
        }

        public ShaderIrOperGpr(int index, int halfPart)
        {
            Index    = index;
            HalfPart = halfPart;

            RegisterSize = ShaderRegisterSize.Half;
        }

        public static ShaderIrOperGpr MakeTemporary(int index = 0)
        {
            return new ShaderIrOperGpr(0x100 + index);
        }
    }
}