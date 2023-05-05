namespace Ryujinx.Graphics.Shader
{
    public struct BufferDescriptor
    {
        // New fields should be added to the end of the struct to keep disk shader cache compatibility.

        public readonly int Binding;
        public readonly byte Slot;
        public readonly byte SbCbSlot;
        public readonly ushort SbCbOffset;
        public BufferUsageFlags Flags;

        public BufferDescriptor(int binding, int slot)
        {
            Binding = binding;
            Slot = (byte)slot;
            SbCbSlot = 0;
            SbCbOffset = 0;

            Flags = BufferUsageFlags.None;
        }

        public BufferDescriptor(int binding, int slot, int sbCbSlot, int sbCbOffset)
        {
            Binding = binding;
            Slot = (byte)slot;
            SbCbSlot = (byte)sbCbSlot;
            SbCbOffset = (ushort)sbCbOffset;

            Flags = BufferUsageFlags.None;
        }

        public BufferDescriptor SetFlag(BufferUsageFlags flag)
        {
            Flags |= flag;

            return this;
        }
    }
}