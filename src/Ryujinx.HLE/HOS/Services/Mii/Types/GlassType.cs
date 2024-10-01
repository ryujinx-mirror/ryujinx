namespace Ryujinx.HLE.HOS.Services.Mii.Types
{
    enum GlassType : byte
    {
        None,
        Oval,
        Wayfarer,
        Rectangle,
        TopRimless,
        Rounded,
        Oversized,
        CatEye,
        Square,
        BottomRimless,
        SemiOpaqueRounded,
        SemiOpaqueCatEye,
        SemiOpaqueOval,
        SemiOpaqueRectangle,
        SemiOpaqueAviator,
        OpaqueRounded,
        OpaqueCatEye,
        OpaqueOval,
        OpaqueRectangle,
        OpaqueAviator,

        Min = None,
        Max = OpaqueAviator,
    }
}
