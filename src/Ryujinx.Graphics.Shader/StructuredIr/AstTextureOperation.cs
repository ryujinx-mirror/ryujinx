using Ryujinx.Graphics.Shader.IntermediateRepresentation;

namespace Ryujinx.Graphics.Shader.StructuredIr
{
    class AstTextureOperation : AstOperation
    {
        public SamplerType Type { get; }
        public TextureFormat Format { get; }
        public TextureFlags Flags { get; }

        public int Set { get; }
        public int Binding { get; }
        public int SamplerSet { get; }
        public int SamplerBinding { get; }

        public bool IsSeparate => SamplerBinding >= 0;

        public AstTextureOperation(
            Instruction inst,
            SamplerType type,
            TextureFormat format,
            TextureFlags flags,
            int set,
            int binding,
            int samplerSet,
            int samplerBinding,
            int index,
            params IAstNode[] sources) : base(inst, StorageKind.None, false, index, sources, sources.Length)
        {
            Type = type;
            Format = format;
            Flags = flags;
            Set = set;
            Binding = binding;
            SamplerSet = samplerSet;
            SamplerBinding = samplerBinding;
        }

        public SetBindingPair GetTextureSetAndBinding()
        {
            return new SetBindingPair(Set, Binding);
        }

        public SetBindingPair GetSamplerSetAndBinding()
        {
            return new SetBindingPair(SamplerSet, SamplerBinding);
        }
    }
}
