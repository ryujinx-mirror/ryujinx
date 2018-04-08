namespace Ryujinx.Graphics.Gal
{
    public enum GalBlendFactor
    {
        Zero                  = 0x4000,
        One                   = 0x4001,
        SrcColor              = 0x4300,
        OneMinusSrcColor      = 0x4301,
        SrcAlpha              = 0x4302,
        OneMinusSrcAlpha      = 0x4303,
        DstAlpha              = 0x4304,
        OneMinusDstAlpha      = 0x4305,
        DstColor              = 0x4306,
        OneMinusDstColor      = 0x4307,
        SrcAlphaSaturate      = 0x4308,
        ConstantColor         = 0xc001,
        OneMinusConstantColor = 0xc002,
        ConstantAlpha         = 0xc003,
        OneMinusConstantAlpha = 0xc004,
        Src1Color             = 0xc900,
        OneMinusSrc1Color     = 0xc901,
        Src1Alpha             = 0xc902,
        OneMinusSrc1Alpha     = 0xc903
    }
}