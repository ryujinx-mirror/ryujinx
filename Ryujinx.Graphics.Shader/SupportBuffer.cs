namespace Ryujinx.Graphics.Shader
{
    public static class SupportBuffer
    {
        public const int FieldSize = 16; // Each field takes 16 bytes on default layout, even bool.

        public const int FragmentAlphaTestOffset = 0;
        public const int FragmentIsBgraOffset = FieldSize;
        public const int FragmentIsBgraCount = 8;
        public const int FragmentRenderScaleOffset = FragmentIsBgraOffset + FragmentIsBgraCount * FieldSize;
        public const int ComputeRenderScaleOffset = FragmentRenderScaleOffset + FieldSize; // Skip first scale that is used for the render target

        // One for the render target, 32 for the textures, and 8 for the images.
        public const int RenderScaleMaxCount = 1 + 32 + 8;

        public const int RequiredSize = FragmentRenderScaleOffset + RenderScaleMaxCount * FieldSize;
    }
}