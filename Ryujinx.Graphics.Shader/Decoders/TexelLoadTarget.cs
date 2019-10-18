namespace Ryujinx.Graphics.Shader.Decoders
{
    enum TexelLoadTarget
    {
        Texture1DLodZero            = 0x0,
        Texture1DLodLevel           = 0x1,
        Texture2DLodZero            = 0x2,
        Texture2DLodZeroOffset      = 0x4,
        Texture2DLodLevel           = 0x5,
        Texture2DLodZeroMultisample = 0x6,
        Texture3DLodZero            = 0x7,
        Texture2DArrayLodZero       = 0x8,
        Texture2DLodLevelOffset     = 0xc
    }
}