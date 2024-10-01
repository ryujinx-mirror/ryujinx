namespace Ryujinx.Graphics.GAL
{
    public enum BlendFactor
    {
        Zero = 1,
        One,
        SrcColor,
        OneMinusSrcColor,
        SrcAlpha,
        OneMinusSrcAlpha,
        DstAlpha,
        OneMinusDstAlpha,
        DstColor,
        OneMinusDstColor,
        SrcAlphaSaturate,
        Src1Color = 0x10,
        OneMinusSrc1Color,
        Src1Alpha,
        OneMinusSrc1Alpha,
        ConstantColor = 0xc001,
        OneMinusConstantColor,
        ConstantAlpha,
        OneMinusConstantAlpha,

        ZeroGl = 0x4000,
        OneGl = 0x4001,
        SrcColorGl = 0x4300,
        OneMinusSrcColorGl = 0x4301,
        SrcAlphaGl = 0x4302,
        OneMinusSrcAlphaGl = 0x4303,
        DstAlphaGl = 0x4304,
        OneMinusDstAlphaGl = 0x4305,
        DstColorGl = 0x4306,
        OneMinusDstColorGl = 0x4307,
        SrcAlphaSaturateGl = 0x4308,
        Src1ColorGl = 0xc900,
        OneMinusSrc1ColorGl = 0xc901,
        Src1AlphaGl = 0xc902,
        OneMinusSrc1AlphaGl = 0xc903,
    }

    public static class BlendFactorExtensions
    {
        public static bool IsDualSource(this BlendFactor factor)
        {
            switch (factor)
            {
                case BlendFactor.Src1Color:
                case BlendFactor.Src1ColorGl:
                case BlendFactor.Src1Alpha:
                case BlendFactor.Src1AlphaGl:
                case BlendFactor.OneMinusSrc1Color:
                case BlendFactor.OneMinusSrc1ColorGl:
                case BlendFactor.OneMinusSrc1Alpha:
                case BlendFactor.OneMinusSrc1AlphaGl:
                    return true;
                default:
                    return false;
            }
        }
    }
}
