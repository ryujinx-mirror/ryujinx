namespace Ryujinx.Graphics.Shader.IntermediateRepresentation
{
    class TextureOperation : Operation
    {
        public TextureTarget Target { get; }
        public TextureFlags  Flags  { get; }

        public int Handle { get; }

        public TextureOperation(
            Instruction      inst,
            TextureTarget    target,
            TextureFlags     flags,
            int              handle,
            int              compIndex,
            Operand          dest,
            params Operand[] sources) : base(inst, compIndex, dest, sources)
        {
            Target = target;
            Flags  = flags;
            Handle = handle;
        }
    }
}