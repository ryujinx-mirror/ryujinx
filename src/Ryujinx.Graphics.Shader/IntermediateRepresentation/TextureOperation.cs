namespace Ryujinx.Graphics.Shader.IntermediateRepresentation
{
    class TextureOperation : Operation
    {
        public const int DefaultCbufSlot = -1;

        public SamplerType Type { get; set; }
        public TextureFormat Format { get; set; }
        public TextureFlags Flags { get; private set; }

        public int Set { get; private set; }
        public int Binding { get; private set; }
        public int SamplerSet { get; private set; }
        public int SamplerBinding { get; private set; }

        public TextureOperation(
            Instruction inst,
            SamplerType type,
            TextureFormat format,
            TextureFlags flags,
            int set,
            int binding,
            int compIndex,
            Operand[] dests,
            Operand[] sources) : base(inst, compIndex, dests, sources)
        {
            Type = type;
            Format = format;
            Flags = flags;
            Set = set;
            Binding = binding;
            SamplerSet = -1;
            SamplerBinding = -1;
        }

        public void TurnIntoArray(SetBindingPair setAndBinding)
        {
            Flags &= ~TextureFlags.Bindless;
            Set = setAndBinding.SetIndex;
            Binding = setAndBinding.Binding;
        }

        public void TurnIntoArray(SetBindingPair textureSetAndBinding, SetBindingPair samplerSetAndBinding)
        {
            TurnIntoArray(textureSetAndBinding);

            SamplerSet = samplerSetAndBinding.SetIndex;
            SamplerBinding = samplerSetAndBinding.Binding;
        }

        public void SetBinding(SetBindingPair setAndBinding)
        {
            if ((Flags & TextureFlags.Bindless) != 0)
            {
                Flags &= ~TextureFlags.Bindless;

                RemoveSource(0);
            }

            Set = setAndBinding.SetIndex;
            Binding = setAndBinding.Binding;
        }

        public void SetLodLevelFlag()
        {
            Flags |= TextureFlags.LodLevel;
        }
    }
}
