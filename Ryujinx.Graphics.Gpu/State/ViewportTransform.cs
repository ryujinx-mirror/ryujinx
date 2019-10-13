using Ryujinx.Graphics.GAL;

namespace Ryujinx.Graphics.Gpu.State
{
    struct ViewportTransform
    {
        public float ScaleX;
        public float ScaleY;
        public float ScaleZ;
        public float TranslateX;
        public float TranslateY;
        public float TranslateZ;
        public uint  Swizzle;
        public uint  SubpixelPrecisionBias;

        public ViewportSwizzle UnpackSwizzleX()
        {
            return (ViewportSwizzle)(Swizzle & 7);
        }

        public ViewportSwizzle UnpackSwizzleY()
        {
            return (ViewportSwizzle)((Swizzle >> 4) & 7);
        }

        public ViewportSwizzle UnpackSwizzleZ()
        {
            return (ViewportSwizzle)((Swizzle >> 8) & 7);
        }

        public ViewportSwizzle UnpackSwizzleW()
        {
            return (ViewportSwizzle)((Swizzle >> 12) & 7);
        }
    }
}
