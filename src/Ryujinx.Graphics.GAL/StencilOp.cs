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
        DecrementAndWrap,

        ZeroGl = 0x0,
        InvertGl = 0x150a,
        KeepGl = 0x1e00,
        ReplaceGl = 0x1e01,
        IncrementAndClampGl = 0x1e02,
        DecrementAndClampGl = 0x1e03,
        IncrementAndWrapGl = 0x8507,
        DecrementAndWrapGl = 0x8508,
    }
}
