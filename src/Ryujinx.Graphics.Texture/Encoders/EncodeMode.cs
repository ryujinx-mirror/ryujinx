namespace Ryujinx.Graphics.Texture.Encoders
{
    enum EncodeMode
    {
        Fast,
        Exhaustive,
        ModeMask = 0xff,
        Multithreaded = 1 << 8,
    }
}
