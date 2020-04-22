namespace Ryujinx.HLE.HOS.Services.SurfaceFlinger
{
    enum NativeWindowScalingMode : uint
    {
        Freeze        = 0,
        ScaleToWindow = 1,
        ScaleCrop     = 2,
        Unknown       = 3,
        NoScaleCrop   = 4,
    }
}
