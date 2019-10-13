namespace Ryujinx.Graphics.GAL.DepthStencil
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