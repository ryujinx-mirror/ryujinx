namespace Ryujinx.Graphics.Shader
{
    public struct TextureDescriptor
    {
        public int Binding { get; }

        public SamplerType Type { get; }
        public TextureFormat Format { get; }

        public int CbufSlot { get; }
        public int HandleIndex { get; }

        public TextureUsageFlags Flags { get; set; }

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