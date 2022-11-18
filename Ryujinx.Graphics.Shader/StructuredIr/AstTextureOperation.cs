using Ryujinx.Graphics.Shader.IntermediateRepresentation;

namespace Ryujinx.Graphics.Shader.StructuredIr
{
    class AstTextureOperation : AstOperation
    {
        public SamplerType Type { get; }
        public TextureFormat Format { get; }
        public TextureFlags Flags { get; }

        public int CbufSlot { get; }
        public int Handle { get; }

        public AstTextureOperation(
            Instruction inst,
            SamplerType type,
            TextureFormat format,
            TextureFlags flags,
            int cbufSlot,
            int handle,
            int index,
            params IAstNode[] sources) : base(inst, index, sources, sources.Length)
        {
            Type = type;
            Format = format;
            Flags = flags;
            CbufSlot = cbufSlot;
            Handle = handle;
        }

        public AstTextureOperation WithType(SamplerType type)
        {
            return new AstTextureOperation(Inst, type, Format, Flags, CbufSlot, Handle, Index);
        }
    }
}