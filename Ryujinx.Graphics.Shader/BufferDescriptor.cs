namespace Ryujinx.Graphics.Shader
{
    public struct BufferDescriptor
    {
        public int Binding { get; }
        public int Slot { get; }

        public BufferDescriptor(int binding, int slot)
        {
            Binding = binding;
            Slot = slot;
        }
    }
}