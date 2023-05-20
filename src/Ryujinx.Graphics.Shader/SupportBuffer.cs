using Ryujinx.Common.Memory;
using Ryujinx.Graphics.Shader.StructuredIr;
using Ryujinx.Graphics.Shader.Translation;
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

    enum SupportBufferField
    {
        // Must match the order of the fields on the struct.
        FragmentAlphaTest,
        FragmentIsBgra,
        ViewportInverse,
        FragmentRenderScaleCount,
        RenderScale
    }

    public struct SupportBuffer
    {
        internal const int Binding = 0;

        public static int FieldSize;
        public static int RequiredSize;

        public static int FragmentAlphaTestOffset;
        public static int FragmentIsBgraOffset;
        public static int ViewportInverseOffset;
        public static int FragmentRenderScaleCountOffset;
        public static int GraphicsRenderScaleOffset;
        public static int ComputeRenderScaleOffset;

        public const int FragmentIsBgraCount = 8;
        // One for the render target, 64 for the textures, and 8 for the images.
        public const int RenderScaleMaxCount = 1 + 64 + 8;

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

        internal static StructureType GetStructureType()
        {
            return new StructureType(new[]
            {
                new StructureField(AggregateType.U32, "s_alpha_test"),
                new StructureField(AggregateType.Array | AggregateType.U32, "s_is_bgra", FragmentIsBgraCount),
                new StructureField(AggregateType.Vector4 | AggregateType.FP32, "s_viewport_inverse"),
                new StructureField(AggregateType.S32, "s_frag_scale_count"),
                new StructureField(AggregateType.Array | AggregateType.FP32, "s_render_scale", RenderScaleMaxCount)
            });
        }

        public Vector4<int> FragmentAlphaTest;
        public Array8<Vector4<int>> FragmentIsBgra;
        public Vector4<float> ViewportInverse;
        public Vector4<int> FragmentRenderScaleCount;

        // Render scale max count: 1 + 64 + 8. First scale is fragment output scale, others are textures/image inputs.
        public Array73<Vector4<float>> RenderScale;
    }
}