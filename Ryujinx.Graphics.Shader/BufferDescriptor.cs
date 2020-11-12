namespace Ryujinx.Graphics.Shader
{
    public struct BufferDescriptor
    {
        public readonly int Binding;
        public readonly int Slot;

        public BufferDescriptor(int binding, int slot)
        {
            Binding = binding;
            Slot = slot;
        }
    }
}