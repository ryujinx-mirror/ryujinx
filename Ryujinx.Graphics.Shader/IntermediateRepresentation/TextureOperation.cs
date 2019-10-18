namespace Ryujinx.Graphics.Shader.IntermediateRepresentation
{
    class TextureOperation : Operation
    {
        public SamplerType  Type  { get; }
        public TextureFlags Flags { get; }

        public int Handle { get; }

        public TextureOperation(
            Instruction      inst,
            SamplerType      type,
            TextureFlags     flags,
            int              handle,
            int              compIndex,
            Operand          dest,
            params Operand[] sources) : base(inst, compIndex, dest, sources)
        {
            Type   = type;
            Flags  = flags;
            Handle = handle;
        }
    }
}