using Ryujinx.Graphics.Shader.Decoders;
using Ryujinx.Graphics.Shader.Translation;

using static Ryujinx.Graphics.Shader.Instructions.InstEmitHelper;

namespace Ryujinx.Graphics.Shader.Instructions
{
    static partial class InstEmit
    {
        public static void Vmad(EmitterContext context)
        {
            OpCodeVideo op = (OpCodeVideo)context.CurrOp;

            context.Copy(GetDest(context), GetSrcC(context));
        }
    }
}