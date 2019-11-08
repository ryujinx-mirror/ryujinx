using Ryujinx.Graphics.Shader.IntermediateRepresentation;

namespace Ryujinx.Graphics.Shader.StructuredIr
{
    class AstTextureOperation : AstOperation
    {
        public SamplerType  Type  { get; }
        public TextureFlags Flags { get; }

        public int Handle    { get; }
        public int ArraySize { get; }

        public AstTextureOperation(
            Instruction       inst,
            SamplerType       type,
            TextureFlags      flags,
            int               handle,
            int               arraySize,
            int               index,
            params IAstNode[] sources) : base(inst, index, sources)
        {
            Type      = type;
            Flags     = flags;
            Handle    = handle;
            ArraySize = arraySize;
        }
    }
}