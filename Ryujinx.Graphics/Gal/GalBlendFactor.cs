namespace Ryujinx.Graphics.Gal
{
    public enum GalBlendFactor
    {
        Zero                  = 0x1,
        One                   = 0x2,
        SrcColor              = 0x3,
        OneMinusSrcColor      = 0x4,
        SrcAlpha              = 0x5,
        OneMinusSrcAlpha      = 0x6,
        DstAlpha              = 0x7,
        OneMinusDstAlpha      = 0x8,
        DstColor              = 0x9,
        OneMinusDstColor      = 0xa,
        SrcAlphaSaturate      = 0xb,
        Src1Color             = 0x10,
        OneMinusSrc1Color     = 0x11,
        Src1Alpha             = 0x12,
        OneMinusSrc1Alpha     = 0x13,
        ConstantColor         = 0x61,
        OneMinusConstantColor = 0x62,
        ConstantAlpha         = 0x63,
        OneMinusConstantAlpha = 0x64,
        ConstantColorG80      = 0xc001
    }
}