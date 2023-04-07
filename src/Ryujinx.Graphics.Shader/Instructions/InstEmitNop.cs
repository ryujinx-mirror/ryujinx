using Ryujinx.Graphics.Shader.Decoders;
using Ryujinx.Graphics.Shader.Translation;

namespace Ryujinx.Graphics.Shader.Instructions
{
    static partial class InstEmit
    {
        public static void Nop(EmitterContext context)
        {
            InstNop op = context.GetOp<InstNop>();

            // No operation.
        }
    }
}