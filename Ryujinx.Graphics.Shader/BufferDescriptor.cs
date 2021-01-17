namespace Ryujinx.Graphics.Shader
{
    public struct BufferDescriptor
    {
        public readonly int Binding;
        public readonly int Slot;
        public BufferUsageFlags Flags;

        public BufferDescriptor(int binding, int slot)
        {
            Binding = binding;
            Slot = slot;

            Flags = BufferUsageFlags.None;
        }

        public BufferDescriptor SetFlag(BufferUsageFlags flag)
        {
            Flags |= flag;

            return this;
        }
    }
}