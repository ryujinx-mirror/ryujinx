namespace Ryujinx.Graphics.Shader.IntermediateRepresentation
{
    class TextureOperation : Operation
    {
        public TextureType  Type  { get; }
        public TextureFlags Flags { get; }

        public int Handle { get; }

        public TextureOperation(
            Instruction      inst,
            TextureType      type,
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