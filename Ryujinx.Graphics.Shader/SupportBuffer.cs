using Ryujinx.Common.Memory;
using System.Runtime.CompilerServices;

namespace Ryujinx.Graphics.Shader
{
    public struct Vector4<T>
    {
        public T X;
        public T Y;
        public T Z;
        public T W;
    }

    public struct SupportBuffer
    {
        public static int FieldSize;
        public static int RequiredSize;

        public static int FragmentAlphaTestOffset;
        public static int FragmentIsBgraOffset;
        public static int ViewportInverseOffset;
        public static int FragmentRenderScaleCountOffset;
        public static int GraphicsRenderScaleOffset;
        public static int ComputeRenderScaleOffset;

        public const int FragmentIsBgraCount = 8;
        // One for the render target, 32 for the textures, and 8 for the images.
        public const int RenderScaleMaxCount = 1 + 32 + 8;

        private static int OffsetOf<T>(ref SupportBuffer storage, ref T target)
        {
            return (int)Unsafe.ByteOffset(ref Unsafe.As<SupportBuffer, T>(ref storage), ref target);
        }

        static SupportBuffer()
        {
            FieldSize = Unsafe.SizeOf<Vector4<float>>();
            RequiredSize = Unsafe.SizeOf<SupportBuffer>();

            SupportBuffer instance = new SupportBuffer();

            FragmentAlphaTestOffset = OffsetOf(ref instance, ref instance.FragmentAlphaTest);
            FragmentIsBgraOffset = OffsetOf(ref instance, ref instance.FragmentIsBgra);
            ViewportInverseOffset = OffsetOf(ref instance, ref instance.ViewportInverse);
            FragmentRenderScaleCountOffset = OffsetOf(ref instance, ref instance.FragmentRenderScaleCount);
            GraphicsRenderScaleOffset = OffsetOf(ref instance, ref instance.RenderScale);
            ComputeRenderScaleOffset = GraphicsRenderScaleOffset + FieldSize;
        }

        public Vector4<int> FragmentAlphaTest;
        public Array8<Vector4<int>> FragmentIsBgra;
        public Vector4<float> ViewportInverse;
        public Vector4<int> FragmentRenderScaleCount;

        // Render scale max count: 1 + 32 + 8. First scale is fragment output scale, others are textures/image inputs.
        public Array41<Vector4<float>> RenderScale;
    }
}