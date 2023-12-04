namespace Ryujinx.HLE.HOS.Services.SurfaceFlinger
{
    enum ColorComponent : uint
    {
#pragma warning disable IDE0055 // Disable formatting
        X1           = (0x01 << ColorShift.Component) | ColorBytePerPixel.Bpp1,
        X2           = (0x02 << ColorShift.Component) | ColorBytePerPixel.Bpp2,
        X4           = (0x03 << ColorShift.Component) | ColorBytePerPixel.Bpp4,
        X8           = (0x04 << ColorShift.Component) | ColorBytePerPixel.Bpp8,
        Y4X4         = (0x05 << ColorShift.Component) | ColorBytePerPixel.Bpp8,
        X3Y3Z2       = (0x06 << ColorShift.Component) | ColorBytePerPixel.Bpp8,
        X8Y8         = (0x07 << ColorShift.Component) | ColorBytePerPixel.Bpp16,
        X8Y8X8Z8     = (0x08 << ColorShift.Component) | ColorBytePerPixel.Bpp16,
        Y8X8Z8X8     = (0x09 << ColorShift.Component) | ColorBytePerPixel.Bpp16,
        X16          = (0x0A << ColorShift.Component) | ColorBytePerPixel.Bpp16,
        Y2X14        = (0x0B << ColorShift.Component) | ColorBytePerPixel.Bpp16,
        Y4X12        = (0x0C << ColorShift.Component) | ColorBytePerPixel.Bpp16,
        Y6X10        = (0x0D << ColorShift.Component) | ColorBytePerPixel.Bpp16,
        Y8X8         = (0x0E << ColorShift.Component) | ColorBytePerPixel.Bpp16,
        X10          = (0x0F << ColorShift.Component) | ColorBytePerPixel.Bpp16,
        X12          = (0x10 << ColorShift.Component) | ColorBytePerPixel.Bpp16,
        Z5Y5X6       = (0x11 << ColorShift.Component) | ColorBytePerPixel.Bpp16,
        X5Y6Z5       = (0x12 << ColorShift.Component) | ColorBytePerPixel.Bpp16,
        X6Y5Z5       = (0x13 << ColorShift.Component) | ColorBytePerPixel.Bpp16,
        X1Y5Z5W5     = (0x14 << ColorShift.Component) | ColorBytePerPixel.Bpp16,
        X4Y4Z4W4     = (0x15 << ColorShift.Component) | ColorBytePerPixel.Bpp16,
        X5Y1Z5W5     = (0x16 << ColorShift.Component) | ColorBytePerPixel.Bpp16,
        X5Y5Z1W5     = (0x17 << ColorShift.Component) | ColorBytePerPixel.Bpp16,
        X5Y5Z5W1     = (0x18 << ColorShift.Component) | ColorBytePerPixel.Bpp16,
        X8Y8Z8       = (0x19 << ColorShift.Component) | ColorBytePerPixel.Bpp24,
        X24          = (0x1A << ColorShift.Component) | ColorBytePerPixel.Bpp24,
        X32          = (0x1C << ColorShift.Component) | ColorBytePerPixel.Bpp32,
        X16Y16       = (0x1D << ColorShift.Component) | ColorBytePerPixel.Bpp32,
        X11Y11Z10    = (0x1E << ColorShift.Component) | ColorBytePerPixel.Bpp32,
        X2Y10Z10W10  = (0x20 << ColorShift.Component) | ColorBytePerPixel.Bpp32,
        X8Y8Z8W8     = (0x21 << ColorShift.Component) | ColorBytePerPixel.Bpp32,
        Y10X10       = (0x22 << ColorShift.Component) | ColorBytePerPixel.Bpp32,
        X10Y10Z10W2  = (0x23 << ColorShift.Component) | ColorBytePerPixel.Bpp32,
        Y12X12       = (0x24 << ColorShift.Component) | ColorBytePerPixel.Bpp32,
        X20Y20Z20    = (0x26 << ColorShift.Component) | ColorBytePerPixel.Bpp64,
        X16Y16Z16W16 = (0x27 << ColorShift.Component) | ColorBytePerPixel.Bpp64,
#pragma warning restore IDE0055
    }
}
