namespace Ryujinx.Graphics.Shader.Translation
{
    enum HelperFunctionName
    {
        Invalid,

        ConvertDoubleToFloat,
        ConvertFloatToDouble,
        SharedAtomicMaxS32,
        SharedAtomicMinS32,
        SharedStore8,
        SharedStore16,
        Shuffle,
        ShuffleDown,
        ShuffleUp,
        ShuffleXor,
        TexelFetchScale,
        TextureSizeUnscale,
    }
}
