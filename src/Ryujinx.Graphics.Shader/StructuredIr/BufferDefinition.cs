namespace Ryujinx.Graphics.Shader.StructuredIr
{
    readonly struct BufferDefinition
    {
        public BufferLayout Layout { get; }
        public int Set { get; }
        public int Binding { get; }
        public string Name { get; }
        public StructureType Type { get; }

        public BufferDefinition(BufferLayout layout, int set, int binding, string name, StructureType type)
        {
            Layout = layout;
            Set = set;
            Binding = binding;
            Name = name;
            Type = type;
        }
    }
}
