using Ryujinx.Graphics.Shader.Decoders;
using Ryujinx.Graphics.Shader.Translation;

using static Ryujinx.Graphics.Shader.Instructions.InstEmitHelper;

namespace Ryujinx.Graphics.Shader.Instructions
{
    static partial class InstEmit
    {
        public static void Vmad(EmitterContext context)
        {
            InstVmad op = context.GetOp<InstVmad>();

            // TODO: Implement properly.
            context.Copy(GetDest(op.Dest), GetSrcReg(context, op.SrcC));
        }
    }
}