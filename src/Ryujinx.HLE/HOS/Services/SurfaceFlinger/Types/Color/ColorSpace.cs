namespace Ryujinx.HLE.HOS.Services.SurfaceFlinger
{
    enum ColorSpace : ulong
    {
        NonColor = 0x0L << ColorShift.Space,
        LinearRGBA = 0x1L << ColorShift.Space,
        SRGB = 0x2L << ColorShift.Space,

        RGB709 = 0x3L << ColorShift.Space,
        LinearRGB709 = 0x4L << ColorShift.Space,

        LinearScRGB = 0x5L << ColorShift.Space,

        RGB2020 = 0x6L << ColorShift.Space,
        LinearRGB2020 = 0x7L << ColorShift.Space,
        RGB2020_PQ = 0x8L << ColorShift.Space,

        ColorIndex = 0x9L << ColorShift.Space,

        YCbCr601 = 0xAL << ColorShift.Space,
        YCbCr601_RR = 0xBL << ColorShift.Space,
        YCbCr601_ER = 0xCL << ColorShift.Space,
        YCbCr709 = 0xDL << ColorShift.Space,
        YCbCr709_ER = 0xEL << ColorShift.Space,

        BayerRGGB = 0x10L << ColorShift.Space,
        BayerBGGR = 0x11L << ColorShift.Space,
        BayerGRBG = 0x12L << ColorShift.Space,
        BayerGBRG = 0x13L << ColorShift.Space,

        XYZ = 0x14L << ColorShift.Space,
    }
}
