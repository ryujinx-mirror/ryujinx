namespace Ryujinx.Graphics.GAL
{
    public enum Target
    {
        Texture1D,
        Texture2D,
        Texture3D,
        Texture1DArray,
        Texture2DArray,
        Texture2DMultisample,
        Texture2DMultisampleArray,
        Cubemap,
        CubemapArray,
        TextureBuffer
    }

    public static class TargetExtensions
    {
        public static bool IsMultisample(this Target target)
        {
            return target == Target.Texture2DMultisample || target == Target.Texture2DMultisampleArray;
        }
    }
}