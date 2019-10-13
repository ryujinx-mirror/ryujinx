namespace Ryujinx.Graphics.GAL.Blend
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
        OneMinusConstantAlpha
    }
}