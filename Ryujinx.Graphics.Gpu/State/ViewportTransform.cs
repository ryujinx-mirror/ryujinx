using Ryujinx.Graphics.GAL;

namespace Ryujinx.Graphics.Gpu.State
{
    /// <summary>
    /// Viewport transform parameters, for viewport transformation.
    /// </summary>
    struct ViewportTransform
    {
#pragma warning disable CS0649
        public float ScaleX;
        public float ScaleY;
        public float ScaleZ;
        public float TranslateX;
        public float TranslateY;
        public float TranslateZ;
        public uint  Swizzle;
        public uint  SubpixelPrecisionBias;
#pragma warning restore CS0649

        /// <summary>
        /// Unpacks viewport swizzle of the position X component.
        /// </summary>
        /// <returns>Swizzle enum value</returns>
        public ViewportSwizzle UnpackSwizzleX()
        {
            return (ViewportSwizzle)(Swizzle & 7);
        }

        /// <summary>
        /// Unpacks viewport swizzle of the position Y component.
        /// </summary>
        /// <returns>Swizzle enum value</returns>
        public ViewportSwizzle UnpackSwizzleY()
        {
            return (ViewportSwizzle)((Swizzle >> 4) & 7);
        }

        /// <summary>
        /// Unpacks viewport swizzle of the position Z component.
        /// </summary>
        /// <returns>Swizzle enum value</returns>
        public ViewportSwizzle UnpackSwizzleZ()
        {
            return (ViewportSwizzle)((Swizzle >> 8) & 7);
        }

        /// <summary>
        /// Unpacks viewport swizzle of the position W component.
        /// </summary>
        /// <returns>Swizzle enum value</returns>
        public ViewportSwizzle UnpackSwizzleW()
        {
            return (ViewportSwizzle)((Swizzle >> 12) & 7);
        }
    }
}
