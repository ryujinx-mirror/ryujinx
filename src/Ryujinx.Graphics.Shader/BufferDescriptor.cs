namespace Ryujinx.Graphics.Shader
{
    public readonly struct BufferDescriptor
    {
        // New fields should be added to the end of the struct to keep disk shader cache compatibility.

        public readonly int Set;
        public readonly int Binding;
        public readonly byte Slot;
        public readonly byte SbCbSlot;
        public readonly ushort SbCbOffset;
        public readonly BufferUsageFlags Flags;

        public BufferDescriptor(int set, int binding, int slot)
        {
            Set = set;
            Binding = binding;
            Slot = (byte)slot;
            SbCbSlot = 0;
            SbCbOffset = 0;
            Flags = BufferUsageFlags.None;
        }

        public BufferDescriptor(int set, int binding, int slot, int sbCbSlot, int sbCbOffset, BufferUsageFlags flags)
        {
            Set = set;
            Binding = binding;
            Slot = (byte)slot;
            SbCbSlot = (byte)sbCbSlot;
            SbCbOffset = (ushort)sbCbOffset;
            Flags = flags;
        }
    }
}
