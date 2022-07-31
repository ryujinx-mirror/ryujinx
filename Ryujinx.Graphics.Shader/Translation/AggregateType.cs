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

        Vector = 1 << 8,
        Array  = 1 << 9
    }
}
