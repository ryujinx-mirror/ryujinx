namespace Ryujinx.Graphics.Gal
{
    public enum GalStencilOp
    {
        Keep     = 0x1,
        Zero     = 0x2,
        Replace  = 0x3,
        Incr     = 0x4,
        Decr     = 0x5,
        Invert   = 0x6,
        IncrWrap = 0x7,
        DecrWrap = 0x8
    }
}