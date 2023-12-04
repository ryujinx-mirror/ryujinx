namespace Ryujinx.HLE.HOS.Services.SurfaceFlinger
{
    enum ColorSwizzle
    {
#pragma warning disable IDE0055 // Disable formatting
        XYZW = 0x688 << ColorShift.Swizzle,
        ZYXW = 0x60a << ColorShift.Swizzle,
        WZYX = 0x053 << ColorShift.Swizzle,
        YZWX = 0x0d1 << ColorShift.Swizzle,
        XYZ1 = 0xa88 << ColorShift.Swizzle,
        YZW1 = 0xad1 << ColorShift.Swizzle,
        XXX1 = 0xa00 << ColorShift.Swizzle,
        XZY1 = 0xa50 << ColorShift.Swizzle,
        ZYX1 = 0xa0a << ColorShift.Swizzle,
        WZY1 = 0xa53 << ColorShift.Swizzle,
        X000 = 0x920 << ColorShift.Swizzle,
        Y000 = 0x921 << ColorShift.Swizzle,
        XY01 = 0xb08 << ColorShift.Swizzle,
        X001 = 0xb20 << ColorShift.Swizzle,
        X00X = 0x121 << ColorShift.Swizzle,
        X00Y = 0x320 << ColorShift.Swizzle,
       _0YX0 = 0x80c << ColorShift.Swizzle,
       _0ZY0 = 0x814 << ColorShift.Swizzle,
       _0XZ0 = 0x884 << ColorShift.Swizzle,
       _0X00 = 0x904 << ColorShift.Swizzle,
       _00X0 = 0x824 << ColorShift.Swizzle,
       _000X = 0x124 << ColorShift.Swizzle,
       _0XY0 = 0x844 << ColorShift.Swizzle,
        XXXY = 0x200 << ColorShift.Swizzle,
        YYYX = 0x049 << ColorShift.Swizzle,
#pragma warning restore IDE0055
    }
}
