namespace Ryujinx.Graphics.Shader.IntermediateRepresentation
{
    class TextureOperation : Operation
    {
        public SamplerType  Type  { get; private set; }
        public TextureFlags Flags { get; private set; }

        public int Handle { get; private set; }

        public TextureFormat Format { get; set; }

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

        public void TurnIntoIndexed(int handle)
        {
            Type |= SamplerType.Indexed;

            Flags &= ~TextureFlags.Bindless;

            Handle = handle;
        }
    }
}