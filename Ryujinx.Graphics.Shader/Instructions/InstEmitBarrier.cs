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
                context.Config.GpuAccessor.Log($"Invalid barrier mode: {op.BarOp}.");
            }
        }

        public static void Depbar(EmitterContext context)
        {
            InstDepbar op = context.GetOp<InstDepbar>();

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