using Ryujinx.Graphics.Shader.IntermediateRepresentation;

namespace Ryujinx.Graphics.Shader.StructuredIr
{
    class AstTextureOperation : AstOperation
    {
        public TextureType  Type  { get; }
        public TextureFlags Flags { get; }

        public int Handle { get; }

        public AstTextureOperation(
            Instruction       inst,
            TextureType       type,
            TextureFlags      flags,
            int               handle,
            int               compMask,
            params IAstNode[] sources) : base(inst, compMask, sources)
        {
            Type   = type;
            Flags  = flags;
            Handle = handle;
        }
    }
}