namespace Ryujinx.Graphics.Shader
{
    public readonly struct TextureDescriptor
    {
        // New fields should be added to the end of the struct to keep disk shader cache compatibility.

        public readonly int Set;
        public readonly int Binding;

        public readonly SamplerType Type;
        public readonly TextureFormat Format;

        public readonly int CbufSlot;
        public readonly int HandleIndex;
        public readonly int ArrayLength;

        public readonly bool Separate;

        public readonly TextureUsageFlags Flags;

        public TextureDescriptor(
            int set,
            int binding,
            SamplerType type,
            TextureFormat format,
            int cbufSlot,
            int handleIndex,
            int arrayLength,
            bool separate,
            TextureUsageFlags flags)
        {
            Set = set;
            Binding = binding;
            Type = type;
            Format = format;
            CbufSlot = cbufSlot;
            HandleIndex = handleIndex;
            ArrayLength = arrayLength;
            Separate = separate;
            Flags = flags;
        }
    }
}
