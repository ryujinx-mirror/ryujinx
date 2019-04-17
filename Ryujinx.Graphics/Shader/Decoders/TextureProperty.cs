namespace Ryujinx.Graphics.Shader.Decoders
{
    enum TextureProperty
    {
        Dimensions  = 0x1,
        Type        = 0x2,
        SamplePos   = 0x5,
        Filter      = 0xa,
        Lod         = 0xc,
        Wrap        = 0xe,
        BorderColor = 0x10
    }
}