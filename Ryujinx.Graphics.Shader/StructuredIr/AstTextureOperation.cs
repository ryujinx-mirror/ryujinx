using Ryujinx.Graphics.Shader.IntermediateRepresentation;

namespace Ryujinx.Graphics.Shader.StructuredIr
{
    class AstTextureOperation : AstOperation
    {
        public TextureTarget Target { get; }
        public TextureFlags  Flags  { get; }

        public int Handle { get; }

        public AstTextureOperation(
            Instruction       inst,
            TextureTarget     target,
            TextureFlags      flags,
            int               handle,
            int               compMask,
            params IAstNode[] sources) : base(inst, compMask, sources)
        {
            Target = target;
            Flags  = flags;
            Handle = handle;
        }
    }
}