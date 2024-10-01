namespace Ryujinx.Graphics.Shader
{
    readonly struct TextureDefinition
    {
        public int Set { get; }
        public int Binding { get; }
        public int ArrayLength { get; }
        public bool Separate { get; }
        public string Name { get; }
        public SamplerType Type { get; }
        public TextureFormat Format { get; }
        public TextureUsageFlags Flags { get; }

        public TextureDefinition(
            int set,
            int binding,
            int arrayLength,
            bool separate,
            string name,
            SamplerType type,
            TextureFormat format,
            TextureUsageFlags flags)
        {
            Set = set;
            Binding = binding;
            ArrayLength = arrayLength;
            Separate = separate;
            Name = name;
            Type = type;
            Format = format;
            Flags = flags;
        }

        public TextureDefinition(int set, int binding, string name, SamplerType type) : this(set, binding, 1, false, name, type, TextureFormat.Unknown, TextureUsageFlags.None)
        {
        }

        public TextureDefinition SetFlag(TextureUsageFlags flag)
        {
            return new TextureDefinition(Set, Binding, ArrayLength, Separate, Name, Type, Format, Flags | flag);
        }
    }
}
