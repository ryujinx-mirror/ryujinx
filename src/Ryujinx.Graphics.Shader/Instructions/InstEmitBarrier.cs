using Ryujinx.Graphics.Shader.Decoders;
using Ryujinx.Graphics.Shader.Translation;

namespace Ryujinx.Graphics.Shader.Instructions
{
    static partial class InstEmit
    {
        public static void Bar(EmitterContext context)
        {
            InstBar op = context.GetOp<InstBar>();

            // TODO: Support other modes.
            if (op.BarOp == BarOp.Sync)
            {
                context.Barrier();
            }
            else
            {
                context.TranslatorContext.GpuAccessor.Log($"Invalid barrier mode: {op.BarOp}.");
            }
        }

        public static void Depbar(EmitterContext context)
        {
#pragma warning disable IDE0059 // Remove unnecessary value assignment
            InstDepbar op = context.GetOp<InstDepbar>();
#pragma warning restore IDE0059

            // No operation.
        }

        public static void Membar(EmitterContext context)
        {
            InstMembar op = context.GetOp<InstMembar>();

            if (op.Membar == Decoders.Membar.Cta)
            {
                context.GroupMemoryBarrier();
            }
            else
            {
                context.MemoryBarrier();
            }
        }
    }
}
