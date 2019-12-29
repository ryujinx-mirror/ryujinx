namespace Ryujinx.Graphics.GAL
{
    public enum StencilOp
    {
        Keep = 1,
        Zero,
        Replace,
        IncrementAndClamp,
        DecrementAndClamp,
        Invert,
        IncrementAndWrap,
        DecrementAndWrap
    }
}