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
        TextureBuffer,
    }

    public static class TargetExtensions
    {
        public static bool IsMultisample(this Target target)
        {
            return target == Target.Texture2DMultisample || target == Target.Texture2DMultisampleArray;
        }

        public static bool HasDepthOrLayers(this Target target)
        {
            return target == Target.Texture3D ||
                target == Target.Texture1DArray ||
                target == Target.Texture2DArray ||
                target == Target.Texture2DMultisampleArray ||
                target == Target.Cubemap ||
                target == Target.CubemapArray;
        }
    }
}
