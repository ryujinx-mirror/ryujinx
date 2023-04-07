namespace Ryujinx.Graphics.Shader.Translation
{
    enum AggregateType
    {
        Invalid,
        Void,
        Bool,
        FP32,
        FP64,
        S32,
        U32,

        ElementTypeMask = 0xff,

        ElementCountShift = 8,
        ElementCountMask = 3 << ElementCountShift,

        Scalar = 0 << ElementCountShift,
        Vector2 = 1 << ElementCountShift,
        Vector3 = 2 << ElementCountShift,
        Vector4 = 3 << ElementCountShift,

        Array  = 1 << 10
    }
}
