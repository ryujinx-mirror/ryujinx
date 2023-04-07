namespace Ryujinx.Graphics.Shader
{
    public struct TextureDescriptor
    {
        // New fields should be added to the end of the struct to keep disk shader cache compatibility.

        public readonly int Binding;

        public readonly SamplerType Type;
        public readonly TextureFormat Format;

        public readonly int CbufSlot;
        public readonly int HandleIndex;

        public TextureUsageFlags Flags;

        public TextureDescriptor(int binding, SamplerType type, TextureFormat format, int cbufSlot, int handleIndex)
        {
            Binding     = binding;
            Type        = type;
            Format      = format;
            CbufSlot    = cbufSlot;
            HandleIndex = handleIndex;
            Flags       = TextureUsageFlags.None;
        }

        public TextureDescriptor SetFlag(TextureUsageFlags flag)
        {
            Flags |= flag;

            return this;
        }
    }
}